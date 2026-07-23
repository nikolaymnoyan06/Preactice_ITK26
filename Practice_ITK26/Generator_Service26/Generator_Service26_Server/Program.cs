using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model;
using Generator_Service26_Server.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

// Настройки JSON для десериализации параметров (camelCase + enum в строки)
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() }
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddScoped<GraphDatabaseService>();


builder.Services.AddScoped<GraphDatabaseService>();


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
app.MapGrpcService<GraphService>();
app.MapGrpcService<GraphGrpcServiceImpl>();
app.MapGet("/", () => "gRPC сервер запущен и готов принимать соединения.");

app.UseCors("AllowAll");
app.Run();

// ХРАНИЛИЩЕ В ПАМЯТИ 
// ------------------------------------------------------------------
var nodes = new List<NodeDto>();
var edges = new List<EdgeDto>();
var nextEdgeId = 1;
//нужно ли это вообще?



// ОБРАБОТЧИКИ ОПЕРАЦИЙ 
// ------------------------------------------------------------------

/// <summary>Получить все узлы.</summary>
object HandleGetNodes() => nodes;

/// <summary>Добавить новый узел.</summary>
async Task<object> HandleAddNode(JsonElement parameters, GraphDatabaseService dbService)
{
    var dto = parameters.GetProperty("node").Deserialize<NodeDto>(jsonOptions)
        ?? throw new ArgumentException("Неверный формат узла");

    if (nodes.Any(n => n.Id == dto.Id))
        throw new ArgumentException($"Узел с ID {dto.Id} уже существует");

    if (string.IsNullOrEmpty(dto.Name))
        dto.Name = $"Узел {dto.Id}";

    nodes.Add(dto); // Оставляем в памяти (если нужно)

    // --- ОТПРАВЛЯЕМ В БАЗУ ДАННЫХ ITK ---
    await dbService.AddNodeAsync(dto);

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
async Task<object> HandleAddEdge(JsonElement parameters, GraphDatabaseService dbService)
{
    var dto = parameters.GetProperty("edge").Deserialize<EdgeDto>(jsonOptions)
        ?? throw new ArgumentException("Неверный формат ребра");

    if (!nodes.Any(n => n.Id == dto.SourceId))
        throw new ArgumentException($"Узел-источник {dto.SourceId} не найден");
    if (!nodes.Any(n => n.Id == dto.TargetId))
        throw new ArgumentException($"Узел-приёмник {dto.TargetId} не найден");

    if (dto.Id == 0) dto.Id = nextEdgeId++;
    else
    {
        if (edges.Any(e => e.Id == dto.Id))
            throw new ArgumentException($"Ребро с ID {dto.Id} уже существует");
        if (dto.Id >= nextEdgeId)
            nextEdgeId = dto.Id + 1;
    }

    edges.Add(dto);

    // --- ОТПРАВЛЯЕМ В БАЗУ ДАННЫХ ITK ---
    await dbService.AddEdgeAsync(dto);

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
async Task<object> HandleGenerateNodes(JsonElement parameters, GraphDatabaseService dbService)
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

        // --- СОХРАНЯЕМ КАЖДЫЙ СГЕНЕРИРОВАННЫЙ УЗЕЛ В БД ---
        await dbService.AddNodeAsync(node);
    }

    return new { message = $"Сгенерировано {count} узлов", totalNodes = nodes.Count };

    // ДИСПЕТЧЕР МЕТОДОВ 
    // ------------------------------------------------------------------
    async Task<object?> DispatchMethod(string method, JsonElement? parameters, GraphDatabaseService dbService)
    {
        return method switch
        {
            "getNodes" => HandleGetNodes(),
            "addNode" => await HandleAddNode(parameters!.Value, dbService),
            "deleteNode" => HandleDeleteNode(parameters!.Value),
            "getEdges" => HandleGetEdges(),
            "addEdge" => await HandleAddEdge(parameters!.Value, dbService),
            "deleteEdge" => HandleDeleteEdge(parameters!.Value),
            "generateNodes" => await HandleGenerateNodes(parameters!.Value, dbService),
            _ => throw new InvalidOperationException($"Неизвестный метод: {method}")
        };
    }
    }


    
 

