using Generator_Service26_Server.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Generator_Service26.Model.Dtos
{
    public class NodeDto
    {
        public int Id { get; set; }
        public object Value { get; set; } // может быть строка, число и т.д.
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; } // новое
        public override string ToString() => $"{Value} ({Type})";
    }
}
