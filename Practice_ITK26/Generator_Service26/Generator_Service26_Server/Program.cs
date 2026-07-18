using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model;
using Generator_Service26_Server.Model.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

// Настройки JSON для десериализации параметров (camelCase + enum в строки)
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
};

var builder = WebApplication.CreateBuilder(args);

// Принудительно слушаем порт 5000 
builder.WebHost.UseUrls("http://localhost:5000");

// Настройка JSON: enum → строки 
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// CORS – разрешаем все запросы с клиента 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// ХРАНИЛИЩЕ В ПАМЯТИ 
// ------------------------------------------------------------------
var nodes = new List<NodeDto>();
var edges = new List<EdgeDto>();
var nextEdgeId = 1;


// ОБРАБОТЧИКИ ОПЕРАЦИЙ 
// ------------------------------------------------------------------

/// <summary>Получить все узлы.</summary>
object HandleGetNodes() => nodes;

/// <summary>Добавить новый узел.</summary>
object HandleAddNode(JsonElement parameters)
{
    // Из параметров извлекаем объект "node"
    var dto = parameters.GetProperty("node").Deserialize<NodeDto>(jsonOptions)
        ?? throw new ArgumentException("Неверный формат узла");

    if (nodes.Any(n => n.Id == dto.Id))
        throw new ArgumentException($"Узел с ID {dto.Id} уже существует");

    if (string.IsNullOrEmpty(dto.Name))
        dto.Name = $"Узел {dto.Id}";

    nodes.Add(dto);
    return new { message = "Узел добавлен", id = dto.Id };
}

/// <summary>Удалить узел по ID.</summary>
object HandleDeleteNode(JsonElement parameters)
{
    int id = parameters.GetProperty("id").GetInt32();

    var node = nodes.FirstOrDefault(n => n.Id == id);
    if (node == null)
        throw new KeyNotFoundException($"Узел с ID {id} не найден");

    nodes.Remove(node);
    // Удаляем все рёбра, связанные с этим узлом
    edges.RemoveAll(e => e.SourceId == id || e.TargetId == id);

    return new { message = "Узел удалён", id };
}

/// <summary>Получить все рёбра.</summary>
object HandleGetEdges() => edges;

/// <summary>Добавить ребро.</summary>
object HandleAddEdge(JsonElement parameters)
{
    var dto = parameters.GetProperty("edge").Deserialize<EdgeDto>(jsonOptions)
        ?? throw new ArgumentException("Неверный формат ребра");

    if (!nodes.Any(n => n.Id == dto.SourceId))
        throw new ArgumentException($"Узел-источник {dto.SourceId} не найден");
    if (!nodes.Any(n => n.Id == dto.TargetId))
        throw new ArgumentException($"Узел-приёмник {dto.TargetId} не найден");

    // Авто-ID, если не указан или указан 0
    if (dto.Id == 0)
    {
        dto.Id = nextEdgeId++;
    }
    else
    {
        if (edges.Any(e => e.Id == dto.Id))
            throw new ArgumentException($"Ребро с ID {dto.Id} уже существует");
        if (dto.Id >= nextEdgeId)
            nextEdgeId = dto.Id + 1;
    }

    edges.Add(dto);
    return new { message = "Ребро добавлено", id = dto.Id };
}

/// <summary>Удалить ребро по ID.</summary>
object HandleDeleteEdge(JsonElement parameters)
{
    int id = parameters.GetProperty("id").GetInt32();

    var edge = edges.FirstOrDefault(e => e.Id == id);
    if (edge == null)
        throw new KeyNotFoundException($"Ребро с ID {id} не найдено");

    edges.Remove(edge);
    return new { message = "Ребро удалено" };
}

/// <summary>Сгенерировать узлы через Generator.</summary>
object HandleGenerateNodes(JsonElement parameters)
{
    int count = parameters.GetProperty("count").GetInt32();

    if (count <= 0 || count > 100)
        throw new ArgumentException("Количество должно быть от 1 до 100");

    var generator = new Generator();
    var newNodes = generator.GenerateNodes(count);

    int maxId = nodes.Any() ? nodes.Max(n => n.Id) : 0;
    foreach (var node in newNodes)
    {
        maxId++;
        node.Id = maxId;
        nodes.Add(node);
    }

    return new { message = $"Сгенерировано {count} узлов", totalNodes = nodes.Count };
}

// ДИСПЕТЧЕР МЕТОДОВ 
// ------------------------------------------------------------------
object? DispatchMethod(string method, JsonElement? parameters)
{
    return method switch
    {
        "getNodes" => HandleGetNodes(),
        "addNode" => HandleAddNode(parameters!.Value),
        "deleteNode" => HandleDeleteNode(parameters!.Value),
        "getEdges" => HandleGetEdges(),
        "addEdge" => HandleAddEdge(parameters!.Value),
        "deleteEdge" => HandleDeleteEdge(parameters!.Value),
        "generateNodes" => HandleGenerateNodes(parameters!.Value),
        _ => throw new InvalidOperationException($"Неизвестный метод: {method}")
    };
}


// ЕДИНЫЙ JSON-RPC ЭНДПОИНТ
// ------------------------------------------------------------------
app.MapPost("/jsonrpc", async (HttpContext context) =>
{
    try
    {
        // 1. Читаем и парсим тело запроса
        using var doc = await JsonDocument.ParseAsync(context.Request.Body);
        var request = doc.RootElement;

        // 2. Проверяем версию JSON-RPC
        string? jsonrpc = request.GetProperty("jsonrpc").GetString();
        if (jsonrpc != "2.0")
            throw new Exception("Поддерживается только JSON-RPC 2.0");

        // 3. Извлекаем id 
        int? id = request.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : null;

        // 4. Имя метода
        string method = request.GetProperty("method").GetString()!;

        // 5. Параметры 
        JsonElement? parameters = request.TryGetProperty("params", out var paramsProp) ? paramsProp : null;

        // 6. Выполняем метод 
        object? result;
        try
        {
            result = DispatchMethod(method, parameters);
        }
        catch (Exception ex)
        {
            // Возвращаем ошибку JSON-RPC 
            var errorResponse = new
            {
                jsonrpc = "2.0",
                error = new { code = -32000, message = ex.Message },
                id
            };
            context.Response.StatusCode = 200; // согласно спецификации, ошибка возвращается с HTTP 200
            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        // 7. Успешный ответ
        var response = new
        {
            jsonrpc = "2.0",
            result,
            id
        };
        context.Response.StatusCode = 200;
        await context.Response.WriteAsJsonAsync(response);
    }
    catch (Exception ex)
    {
        // Если не удалось разобрать запрос 
        var errorResponse = new
        {
            jsonrpc = "2.0",
            error = new { code = -32700, message = "Parse error" },
            id = (int?)null
        };
        context.Response.StatusCode = 200;
        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// Запускаем сервер
app.Run();