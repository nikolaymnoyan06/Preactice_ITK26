using Client.Models.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Dtos
{
    /// <summary>
    /// Используется для передачи данных о вершине и её типе.
    /// </summary>
    public partial class NodeDto : ObservableObject
    {
        // Уникальный идентификатор узла.
        public int Id { get; set; }

        // Имя узла
        public string Name { get; set; } = string.Empty;

        // Входящее значение узла (числовое).
        public double InValue { get; set; }

        // Выходящее значение узла (числовое).
        public double OutValue { get; set; }

        // Тип узла 
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NodeType Type { get; set; }

        // Статус узла (OK, NOT OK, NOT STATED) 
        [ObservableProperty]
        private string _status = "❓ NOT STATED";

        /// <summary>
        /// Возвращает строковое представление узла в формате: "Имя (In: X, Out: Y) - Тип".
        /// </summary>
        public override string ToString() => $"{Name} (In: {InValue}, Out: {OutValue}) - {Type}";
    }
}