using Generator_Service26;
using Generator_Service26.Model;// для GraphDataGenerator (если он в корне)
using Generator_Service26.Model.Dtos;
using Generator_Service26_Server;
using Generator_Service26_Server.Model; // для NodeDto и EdgeDto
using Generator_Service26_Server.Model.Dtos;
using GraphModule;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

// Регистрируем граф как синглтон
builder.Services.AddSingleton<GraphModule.Graph>(provider => new GraphModule.Graph(isDirected: false));

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();



// 1. Получить все узлы
app.MapGet("/api/nodes", (Graph graph) =>
{
    var nodes = graph.GetAllNodes();
    var result = nodes.Select(n => new NodeDto
    {
        Id = n.Id,
        Value = n.Value,
        Type = n.Type   // ← вот это замените
    });
    return Results.Ok(result);
});

// 2. Добавить новый узел
app.MapPost("/api/nodes", (NodeDto dto, Graph graph) =>
{
    try
    {
        graph.AddNode(dto.Id, dto.Value, dto.Type);
        return Results.Ok(new { message = "Узел добавлен", id = dto.Id });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 3. Удалить узел по ID
app.MapDelete("/api/nodes/{id}", (int id, Graph graph) =>
{
    var success = graph.RemoveNode(id);
    if (success)
    {
        return Results.Ok(new { message = "Узел удален", id = id });
    }
    else
    {
        return Results.NotFound(new { error = "Узел не найден" });
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Генерация узлов
app.MapPost("/api/generate-nodes", (int count, Graph graph) =>
{
    if (count <= 0 || count > 100)
        return Results.BadRequest("Количество должно быть от 1 до 100");

    var generator = new Generator(graph); // теперь класс виден
    generator.GenerateNodes(count);
    return Results.Ok(new { message = $"Сгенерировано {count} узлов", totalNodes = graph.GetNodeCount() });
});

// Получить все рёбра
app.MapGet("/api/edges", (Graph graph) =>
{
    var edges = graph.GetAllEdges();
    var result = edges.Select(e => new EdgeDto
    {
        Id = e.Id,
        SourceId = e.Source.Id,
        TargetId = e.Target.Id,
        Weight = e.Weight
    });
    return Results.Ok(result);
});

// Добавить ребро
app.MapPost("/api/edges", (EdgeDto dto, Graph graph) =>
{
    try
    {
        if (!graph.ContainsNode(dto.SourceId) || !graph.ContainsNode(dto.TargetId))
            return Results.BadRequest("Один из узлов не найден");

        var edge = graph.AddEdge(dto.SourceId, dto.TargetId, dto.Weight);
        return Results.Ok(new { message = "Ребро добавлено", id = edge.Id });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Удалить ребро по ID
app.MapDelete("/api/edges/{id}", (int id, Graph graph) =>
{
    var allEdges = graph.GetAllEdges();
    var edge = allEdges.FirstOrDefault(e => e.Id == id);
    if (edge == null)
        return Results.NotFound("Ребро не найдено");

    graph.RemoveEdge(edge);
    return Results.Ok(new { message = "Ребро удалено" });
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}