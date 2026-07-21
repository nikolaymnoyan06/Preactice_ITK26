using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model.Dtos;
using Generator_Service26_Server.Protos;
using Grpc.Core;

namespace Generator_Service26_Server.Services;

public class GraphGrpcServiceImpl : GraphGrpc.GraphGrpcBase
{
    private readonly GraphDatabaseService _dbService;

    // Внедряем сервис БД через Dependency Injection
    public GraphGrpcServiceImpl(GraphDatabaseService dbService)
    {
        _dbService = dbService;
    }

    public override async Task<AddNodeResponse> AddNode(AddNodeRequest request, ServerCallContext context)
    {
        var dto = new NodeDto
        {
            Id = request.Node.Id,
            Name = string.IsNullOrEmpty(request.Node.Name) ? $"Узел {request.Node.Id}" : request.Node.Name,
            Value = request.Node.Value
        };

        // Пишем в БД PostgreSQL
        await _dbService.AddNodeAsync(dto);

        return new AddNodeResponse
        {
            Message = "Узел успешно добавлен в PostgreSQL",
            Id = dto.Id
        };
    }

    public override async Task<AddEdgeResponse> AddEdge(AddEdgeRequest request, ServerCallContext context)
    {
        var dto = new EdgeDto
        {
            Id = request.Edge.Id,
            SourceId = request.Edge.SourceId,
            TargetId = request.Edge.TargetId,
            Weight = request.Edge.Weight
        };

        // Пишем в БД PostgreSQL
        await _dbService.AddEdgeAsync(dto);

        return new AddEdgeResponse
        {
            Message = "Ребро успешно добавлено в PostgreSQL",
            Id = dto.Id
        };
    }
}