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

        // Входящее значение узла (числовое).
        public double InValue { get; set; }

        // Выходящее значение узла (числовое).
        public double OutValue { get; set; }

        // Тип узла (Consumer, Source, Transitive).
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; }

        /// <summary>Строковое представление узла.</summary>
        public override string ToString() => $"{Name} (In: {InValue}, Out: {OutValue}) - {Type}";
    }
}