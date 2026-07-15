using Client.Models.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Dtos
{
    /// <summary>
    /// Data Transfer Object (DTO) для представления узла графа.
    /// Используется для передачи данных о вершине и её типе.
    /// </summary>
    public partial class NodeDto : ObservableObject
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

        // НОВОЕ: Статус узла (OK, NOT OK, NOT STATED) - вычисляемое поле.
        // Теперь это ObservableProperty, чтобы изменения автоматически уведомляли UI.
        [ObservableProperty]
        private string _status = "❓ NOT STATED";

        /// <summary>
        /// Возвращает строковое представление узла в формате: "Имя (Значение) - Тип".
        /// </summary>
        public override string ToString() => $"{Name} ({Value}) - {Type}";
    }
}