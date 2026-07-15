using Bogus;
using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model.Dtos;
using System.Collections.Generic;

namespace Generator_Service26_Server.Model
{
    /// <summary>Генератор тестовых узлов на основе DTO.</summary>
    public class Generator
    {
        /// <summary>Генерирует список объектов NodeDto.</summary>
        public List<NodeDto> GenerateNodes(int count)
        {
            // Создаем фейкер для NodeDto
            var faker = new Faker<NodeDto>()
                .RuleFor(n => n.Id, f => f.UniqueIndex + 1)                  // Уникальный ID
                .RuleFor(n => n.Name, f => f.Name.FullName())                // Имя узла
                .RuleFor(n => n.Value, f => f.Random.Double(1, 100))         // Числовое значение
                .RuleFor(n => n.Type, f => f.PickRandom<NodeType>());        // Случайный тип из DTO

            // Возвращаем сгенерированный список DTO
            return faker.Generate(count);
        }
    }
}