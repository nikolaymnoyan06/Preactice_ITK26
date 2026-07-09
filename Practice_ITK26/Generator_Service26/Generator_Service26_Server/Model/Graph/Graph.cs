using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphModule
{
    public class Graph
    {
        private Dictionary<int, Node> nodes;
        private Dictionary<Node, List<Edge>> adjacencyList;
        private int edgeCounter;
        private bool isDirected;

        public Graph(bool isDirected = false)
        {
            nodes = new Dictionary<int, Node>();
            adjacencyList = new Dictionary<Node, List<Edge>>();
            edgeCounter = 0;
            this.isDirected = isDirected;
        }

        // ========== РАБОТА С УЗЛАМИ ==========

        public Node AddNode(int id, object value = null)
        {
            if (nodes.ContainsKey(id))
                throw new ArgumentException($"Узел с ID {id} уже существует");

            var node = new Node(id, value);
            nodes[id] = node;
            adjacencyList[node] = new List<Edge>();
            return node;
        }

        public bool RemoveNode(int id)
        {
            if (!nodes.ContainsKey(id))
                return false;

            var node = nodes[id];
            // Удаляем все рёбра, связанные с этим узлом (если они есть)
            foreach (var otherNode in adjacencyList.Keys)
            {
                adjacencyList[otherNode].RemoveAll(e => e.Source == node || e.Target == node);
            }
            adjacencyList.Remove(node);
            nodes.Remove(id);
            return true;
        }

        // Метод для получения всех узлов (мы добавили его сами)
        public List<Node> GetAllNodes()
        {
            return nodes.Values.ToList();
        }

        // ========== ОПЦИОНАЛЬНО: работа с рёбрами (если понадобятся) ==========
        // Можно оставить только если планируете использовать рёбра.
        // Пока лучше закомментировать или удалить.

        public Edge AddEdge(int sourceId, int targetId, double weight = 1.0)
        {
            if (!nodes.ContainsKey(sourceId) || !nodes.ContainsKey(targetId))
                throw new ArgumentException("Исходный или целевой узел не существует");

            var source = nodes[sourceId];
            var target = nodes[targetId];
            var edge = new Edge(edgeCounter++, source, target, weight);
            adjacencyList[source].Add(edge);

            if (!isDirected)
            {
                var reverseEdge = new Edge(edgeCounter++, target, source, weight);
                adjacencyList[target].Add(reverseEdge);
            }
            return edge;
        }


        public bool RemoveEdge(Edge edge)
        {
            return adjacencyList[edge.Source].Remove(edge);
        }

        public List<Edge> GetAllEdges()
        {
            var allEdges = new List<Edge>();
            foreach (var edges in adjacencyList.Values)
            {
                allEdges.AddRange(edges);
            }
            return isDirected ? allEdges : allEdges.Distinct().ToList();
        }

        // ========== ВСПОМОГАТЕЛЬНЫЕ (можно оставить для удобства) ==========
        public int GetNodeCount() => nodes.Count;
        public bool ContainsNode(int id) => nodes.ContainsKey(id);
        public Node FindNodeById(int id) => nodes.TryGetValue(id, out var node) ? node : null;
    }

}