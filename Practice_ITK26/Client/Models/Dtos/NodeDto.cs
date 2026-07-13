using Client.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Client.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) для представления узла графа.
    /// Используется для передачи данных о вершине и её типе.
    /// </summary>
    public class NodeDto
    {
        // Уникальный идентификатор узла.
        public int Id { get; set; }

        // Имя узла
        public string Name { get; set; } = string.Empty;

        // Значение, хранящееся в узле (числовое).
        public double Value { get; set; }

        // Тип узла (например, начальный, конечный, промежуточный и т.п.).
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; }

        // НОВОЕ: Статус узла (OK, NOT OK, NOT STATED) - вычисляемое поле
        public string Status { get; set; } = "❓ NOT STATED";

        /// <summary>
        /// Возвращает строковое представление узла в формате: "Имя (Значение) - Тип".
        /// </summary>
        public override string ToString() => $"{Name} ({Value}) - {Type}";
    }
}