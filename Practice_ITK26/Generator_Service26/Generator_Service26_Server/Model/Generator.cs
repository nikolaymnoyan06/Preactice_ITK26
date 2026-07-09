using Bogus;
using GraphModule;

namespace Generator_Service26_Server.Model
{
    public class Generator
    {
        private readonly Graph _graph;

        public Generator(Graph graph)
        {
            _graph = graph;
        }

        public void GenerateNodes(int count)
        {
            var faker = new Faker<Node>()
                .RuleFor(n => n.Id, f => f.UniqueIndex + 1)
                .RuleFor(n => n.Value, f => f.Lorem.Word());

            var nodes = faker.Generate(count);

            foreach (var node in nodes)
            {
                try
                {
                    _graph.AddNode(node.Id, node.Value);
                }
                catch (ArgumentException)
                {
                    // если узел с таким ID уже существует – пропускаем
                }
            }
        }
    }
}