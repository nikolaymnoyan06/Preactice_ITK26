using Generator_Service26.Model.Dtos;
using Generator_Service26_Server.Model.Dtos;

namespace Generator_Service26_Server.Model;

public class GraphRepository
{
    private readonly List<NodeDto> _nodes = new();
    private readonly List<EdgeDto> _edges = new();
    private int _nextEdgeId = 1;

    public IReadOnlyList<NodeDto> Nodes => _nodes;
    public IReadOnlyList<EdgeDto> Edges => _edges;

    public void AddNode(NodeDto node)
    {
        if (_nodes.Any(n => n.Id == node.Id))
            throw new ArgumentException($"Узел с ID {node.Id} уже существует");
        _nodes.Add(node);
    }

    public bool RemoveNode(int id)
    {
        var node = _nodes.FirstOrDefault(n => n.Id == id);
        if (node == null) return false;
        _nodes.Remove(node);
        _edges.RemoveAll(e => e.SourceId == id || e.TargetId == id);
        return true;
    }

    public void AddEdge(EdgeDto edge)
    {
        if (!_nodes.Any(n => n.Id == edge.SourceId))
            throw new ArgumentException($"Узел-источник {edge.SourceId} не найден");
        if (!_nodes.Any(n => n.Id == edge.TargetId))
            throw new ArgumentException($"Узел-приёмник {edge.TargetId} не найден");

        if (edge.Id == 0)
        {
            edge.Id = _nextEdgeId++;
        }
        else
        {
            if (_edges.Any(e => e.Id == edge.Id))
                throw new ArgumentException($"Ребро с ID {edge.Id} уже существует");
            if (edge.Id >= _nextEdgeId)
                _nextEdgeId = edge.Id + 1;
        }
        _edges.Add(edge);
    }

    public bool RemoveEdge(int id)
    {
        var edge = _edges.FirstOrDefault(e => e.Id == id);
        if (edge == null) return false;
        _edges.Remove(edge);
        return true;
    }

    public List<NodeDto> GenerateAndAddNodes(int count, Generator generator)
    {
        var newNodes = generator.GenerateNodes(count);
        int maxId = _nodes.Any() ? _nodes.Max(n => n.Id) : 0;
        foreach (var node in newNodes)
        {
            maxId++;
            node.Id = maxId;
            _nodes.Add(node);
        }
        return newNodes;
    }
}