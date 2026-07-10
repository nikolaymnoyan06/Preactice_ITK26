using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model;
using Generator_Service26_Server.Model.Dtos;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

/// <summary>Настройка JSON: Enum -> строки.</summary>
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

var app = builder.Build();

// ============================================================
// ХРАНИЛИЩЕ В ПАМЯТИ
// ============================================================

/// <summary>Хранилище узлов и рёбер в оперативной памяти.</summary>
var nodes = new List<NodeDto>();
var edges = new List<EdgeDto>();

// ============================================================
// ЭНДПОИНТЫ УЗЛОВ
// ============================================================

/// <summary>Получить все узлы.</summary>
app.MapGet("/api/nodes", () => Results.Ok(nodes));

/// <summary>Добавить новый узел.</summary>
app.MapPost("/api/nodes", (NodeDto dto) =>
{
    if (nodes.Any(n => n.Id == dto.Id))
        return Results.BadRequest(new { error = "Узел с таким ID уже существует" });

    nodes.Add(dto);
    return Results.Ok(new { message = "Узел добавлен", id = dto.Id });
});

/// <summary>Удалить узел по ID (вместе со связанными рёбрами).</summary>
app.MapDelete("/api/nodes/{id}", (int id) =>
{
    var node = nodes.FirstOrDefault(n => n.Id == id);
    if (node == null) return Results.NotFound(new { error = "Узел не найден" });

    nodes.Remove(node);
    edges.RemoveAll(e => e.SourceId == id || e.TargetId == id); // Удаляем рёбра

    return Results.Ok(new { message = "Узел удален", id = id });
});

/// <summary>Сгенерировать узлы через Generator.</summary>
app.MapPost("/api/generate-nodes", (int count) =>
{
    if (count <= 0 || count > 100)
        return Results.BadRequest("Количество должно быть от 1 до 100");

    var generator = new Generator();
    var newNodes = generator.GenerateNodes(count);

    foreach (var node in newNodes)
    {
        if (!nodes.Any(n => n.Id == node.Id)) // Если ID свободен
        {
            nodes.Add(node);
        }
    }

    return Results.Ok(new { message = $"Сгенерировано {count} узлов", totalNodes = nodes.Count });
});

// ============================================================
// ЭНДПОИНТЫ РЁБЕР
// ============================================================

/// <summary>Получить все рёбра.</summary>
app.MapGet("/api/edges", () => Results.Ok(edges));

/// <summary>Добавить ребро между узлами.</summary>
app.MapPost("/api/edges", (EdgeDto dto) =>
{
    if (!nodes.Any(n => n.Id == dto.SourceId))
        return Results.BadRequest($"Узел-источник {dto.SourceId} не найден");
    if (!nodes.Any(n => n.Id == dto.TargetId))
        return Results.BadRequest($"Узел-приёмник {dto.TargetId} не найден");

    if (dto.Id == 0) dto.Id = edges.Count + 1; // Авто-ID, если не указан

    edges.Add(dto);
    return Results.Ok(new { message = "Ребро добавлено", id = dto.Id });
});

/// <summary>Удалить ребро по ID.</summary>
app.MapDelete("/api/edges/{id}", (int id) =>
{
    var edge = edges.FirstOrDefault(e => e.Id == id);
    if (edge == null) return Results.NotFound("Ребро не найдено");

    edges.Remove(edge);
    return Results.Ok(new { message = "Ребро удалено" });
});

// ============================================================
// ОКРУЖЕНИЕ
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();