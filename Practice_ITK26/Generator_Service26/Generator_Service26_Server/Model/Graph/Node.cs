using Generator_Service26_Server.Model.Dtos;
using System;

namespace GraphModule
{
    public class Node
    {
        public int Id { get; set; }
        public object Value { get; set; }
        public NodeType Type { get; set; } // новое свойство
        public Node() { }

        public Node(int id, object value = null, NodeType type = NodeType.Transitive)
        {
            Id = id;
            Value = value;
        }

        public override string ToString() => $"Node({Id}: {Value})";
        public override bool Equals(object obj) => obj is Node node && Id == node.Id;
        public override int GetHashCode() => Id.GetHashCode();
    }
}