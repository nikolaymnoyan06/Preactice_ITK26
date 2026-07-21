using Grpc.Core;
using Generator_Service26.Model.Dtos;          
using Generator_Service26_Server.Model;        


namespace Generator_Service26_Server.Services
{
   
    public class GraphService : global::Graph.GraphService.GraphServiceBase
    {
        private readonly GraphRepository _repository;
        private readonly Generator _generator;

        public GraphService(GraphRepository repository, Generator generator)
        {
            _repository = repository;
            _generator = generator;
        }

        public override Task<global::Graph.NodeList> GetNodes(global::Graph.Empty request, ServerCallContext context)
        {
            
            var nodes = _repository.Nodes.Select(n => new global::Graph.NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                InValue = n.InValue,
                OutValue = n.OutValue,
                Type = (global::Graph.NodeType)n.Type   
            });
            var reply = new global::Graph.NodeList();
            reply.Nodes.AddRange(nodes);
            return Task.FromResult(reply);
        }

        public override Task<global::Graph.AddNodeResponse> AddNode(global::Graph.AddNodeRequest request, ServerCallContext context)
        {
            var protoNode = request.Node;
            
            var node = new NodeDto
            {
                Id = protoNode.Id,
                Name = protoNode.Name,
                InValue = protoNode.InValue,
                OutValue = protoNode.OutValue,
                Type = (Generator_Service26_Server.Model.Dtos.NodeType)protoNode.Type
            };

            if (string.IsNullOrEmpty(node.Name))
                node.Name = $"Узел {node.Id}";

            _repository.AddNode(node);

            return Task.FromResult(new global::Graph.AddNodeResponse
            {
                Message = "Узел добавлен",
                Id = node.Id
            });
        }

        public override Task<global::Graph.DeleteNodeResponse> DeleteNode(global::Graph.DeleteNodeRequest request, ServerCallContext context)
        {
            bool removed = _repository.RemoveNode(request.Id);
            if (!removed)
                throw new RpcException(new Status(StatusCode.NotFound, $"Узел с ID {request.Id} не найден"));
            return Task.FromResult(new global::Graph.DeleteNodeResponse { Message = "Узел удалён" });
        }

        public override Task<global::Graph.EdgeList> GetEdges(global::Graph.Empty request, ServerCallContext context)
        {
            var edges = _repository.Edges.Select(e => new global::Graph.EdgeDto
            {
                Id = e.Id,
                SourceId = e.SourceId,
                TargetId = e.TargetId,
                Weight = e.Weight
            });
            var reply = new global::Graph.EdgeList();
            reply.Edges.AddRange(edges);
            return Task.FromResult(reply);
        }

        public override Task<global::Graph.AddEdgeResponse> AddEdge(global::Graph.AddEdgeRequest request, ServerCallContext context)
        {
            var protoEdge = request.Edge;
            var edge = new EdgeDto
            {
                Id = protoEdge.Id,
                SourceId = protoEdge.SourceId,
                TargetId = protoEdge.TargetId,
                Weight = protoEdge.Weight
            };
            _repository.AddEdge(edge);
            return Task.FromResult(new global::Graph.AddEdgeResponse
            {
                Message = "Ребро добавлено",
                Id = edge.Id
            });
        }

        public override Task<global::Graph.DeleteEdgeResponse> DeleteEdge(global::Graph.DeleteEdgeRequest request, ServerCallContext context)
        {
            bool removed = _repository.RemoveEdge(request.Id);
            if (!removed)
                throw new RpcException(new Status(StatusCode.NotFound, $"Ребро с ID {request.Id} не найдено"));
            return Task.FromResult(new global::Graph.DeleteEdgeResponse { Message = "Ребро удалено" });
        }

        public override Task<global::Graph.GenerateNodesResponse> GenerateNodes(global::Graph.GenerateNodesRequest request, ServerCallContext context)
        {
            int count = request.Count;
            if (count <= 0 || count > 100)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Количество должно быть от 1 до 100"));

            var newNodes = _repository.GenerateAndAddNodes(count, _generator);
            return Task.FromResult(new global::Graph.GenerateNodesResponse
            {
                Message = $"Сгенерировано {count} узлов",
                TotalNodes = _repository.Nodes.Count
            });
        }
    }
}