using Generator_Service26_Server.Model;
using Generator_Service26_Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Явно настраиваем Kestrel на HTTP/2 без TLS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Регистрируем gRPC
builder.Services.AddGrpc();

// Регистрируем наши сервисы
builder.Services.AddSingleton<GraphRepository>();
builder.Services.AddSingleton<Generator>();

// CORS – для возможности тестирования (необязательно)
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

// Подключаем gRPC-сервис
app.MapGrpcService<GraphService>();

// Корневой endpoint для проверки (не gRPC)
app.MapGet("/", () => "gRPC сервер запущен");

app.Run();