using Client.Dtos;
using Client.Models.Dtos;

namespace Client.Models
{
    public static class MappingExtensions
    {
        // Преобразование protobuf-узла в клиентский DTO
        public static NodeDto ToClientDto(this global::Graph.NodeDto proto)
        {
            return new NodeDto
            {
                Id = proto.Id,
                Name = proto.Name,
                InValue = proto.InValue,
                OutValue = proto.OutValue,
                Type = (NodeType)proto.Type,
                Status = "❓ NOT STATED"
            };
        }

        // Преобразование protobuf-ребра в клиентский DTO
        public static EdgeDto ToClientDto(this global::Graph.EdgeDto proto)
        {
            return new EdgeDto
            {
                Id = proto.Id,
                SourceId = proto.SourceId,
                TargetId = proto.TargetId,
                Weight = proto.Weight
            };
        }

        // Преобразование клиентского узла в protobuf
        public static global::Graph.NodeDto ToProto(this NodeDto client)
        {
            return new global::Graph.NodeDto
            {
                Id = client.Id,
                Name = client.Name,
                InValue = client.InValue,
                OutValue = client.OutValue,
                Type = (global::Graph.NodeType)client.Type
            };
        }

        // Преобразование клиентского ребра в protobuf
        public static global::Graph.EdgeDto ToProto(this EdgeDto client)
        {
            return new global::Graph.EdgeDto
            {
                Id = client.Id,
                SourceId = client.SourceId,
                TargetId = client.TargetId,
                Weight = client.Weight
            };
        }
    }
}