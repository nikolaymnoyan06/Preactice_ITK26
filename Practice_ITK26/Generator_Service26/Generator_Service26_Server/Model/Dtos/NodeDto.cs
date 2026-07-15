using Generator_Service26_Server.Model.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Generator_Service26.Model.Dtos
{
    /// <summary>DTO для передачи данных об узле графа.</summary>
    public class NodeDto
    {
        // Уникальный идентификатор узла.
        public int Id { get; set; }

        // Имя узла
        public string Name { get; set; } = string.Empty;

        // Произвольное значение, хранящееся в узле (числовое).
        public double Value { get; set; }

        // Тип узла (Consumer, Source, Transitive).
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; }

        /// <summary>Строковое представление узла.</summary>
        public override string ToString() => $"{Name} ({Value}) - {Type}";
    }
}