using Bogus;
using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model.Dtos;
using System.Collections.Generic;

namespace Generator_Service26_Server.Model
{
    public class Generator
    {
        // Убрали поле _graph. Генератор теперь не хранит граф, а только создает данные.

        public Generator()
        {
            // Конструктор теперь пустой
        }

        /// <summary>
        /// Генерирует список объектов NodeDto.
        /// </summary>
        public List<NodeDto> GenerateNodes(int count)
        {
            // Создаем фейкер исключительно для NodeDto
            var faker = new Faker<NodeDto>()
                .RuleFor(n => n.Id, f => f.UniqueIndex + 1)                  // Уникальный ID
                .RuleFor(n => n.Value, f => f.Lorem.Word())                  // Случайное текстовое значение
                .RuleFor(n => n.Type, f => f.PickRandom<NodeType>());        // Случайный тип из DTO

            // Возвращаем сгенерированный список DTO
            return faker.Generate(count);
        }
    }
}