using Client.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace Client.Dtos
{
    public class NodeDto
    {
        public int Id { get; set; }
        public object Value { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; }

        public override string ToString() => $"{Value} ({Type})";
    }
}