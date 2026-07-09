using System;
using System.Xml.Linq;

namespace GraphModule
{
    public class Edge
    {
        public int Id { get; set; }
        public Node Source { get; set; }
        public Node Target { get; set; }
        public double Weight { get; set; }

        public Edge(int id, Node source, Node target, double weight = 1.0)
        {
            Id = id;
            Source = source;
            Target = target;
            Weight = weight;
        }

        public override string ToString() => $"Edge({Id}: {Source.Id} -> {Target.Id}, w={Weight})";
    }
}