using Npgsql;
using Generator_Service26.Model.Dtos; // Подключаем DTO


namespace Generator_Service26_Server.Services;

public class GraphDatabaseService
{
    private readonly string _connectionString;

    // Получаем строку подключения через конструктор
    public GraphDatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnection")
                            ?? throw new ArgumentNullException("Строка подключения не найдена!");
    }

    // Добавление узла
    public async Task AddNodeAsync(NodeDto node)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        
        var sql = "INSERT INTO nodes (id, name, type, value) VALUES (@id, @name, @type, @value)";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", node.Id); 
        command.Parameters.AddWithValue("name", node.Name);
        command.Parameters.AddWithValue("type", (int)node.Type); 
        command.Parameters.AddWithValue("value", node.Value);

        await command.ExecuteNonQueryAsync();
    }

    // Добавление ветви (ребра)
    public async Task AddEdgeAsync(EdgeDto edge)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO edges (id, source_node_id, target_node_id, weight) VALUES (@id, @source, @target, @weight)";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", edge.Id);
        command.Parameters.AddWithValue("source", edge.SourceId);
        command.Parameters.AddWithValue("target", edge.TargetId);
        command.Parameters.AddWithValue("weight", edge.Weight);

        await command.ExecuteNonQueryAsync();
    }
}