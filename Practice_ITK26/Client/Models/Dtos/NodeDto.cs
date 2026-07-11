using Client.Models.Dtos;
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

        // Значение, хранящееся в узле (может быть любого типа).
        public object Value { get; set; }

        // Тип узла (например, начальный, конечный, промежуточный и т.п.).
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; }

        /// <summary>
        /// Возвращает строковое представление узла в формате: "Значение (Тип)".
        /// </summary>
        public override string ToString() => $"{Value} ({Type})";
    }
}