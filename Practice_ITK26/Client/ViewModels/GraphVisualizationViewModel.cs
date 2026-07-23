using Client.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels
{
    public partial class GraphVisualizationViewModel : ObservableObject
    {
        private ObservableCollection<NodeDto>? _nodes;
        private ICommand? _updateStatusesCommand;
        public ICommand? UpdateStatusesCommand
        {
            get => _updateStatusesCommand;
            set => SetProperty(ref _updateStatusesCommand, value);
        }
        public ObservableCollection<NodeDto>? Nodes
        {
            get => _nodes;
            set
            {
                // Отписываемся от старой коллекции
                if (_nodes != null)
                {
                    _nodes.CollectionChanged -= OnNodesCollectionChanged;
                    foreach (var node in _nodes)
                    {
                        node.PropertyChanged -= OnNodePropertyChanged;
                    }
                }

                // Подписываемся на новую коллекцию
                if (SetProperty(ref _nodes, value) && _nodes != null)
                {
                    _nodes.CollectionChanged += OnNodesCollectionChanged;
                    foreach (var node in _nodes)
                    {
                        node.PropertyChanged += OnNodePropertyChanged;
                    }
                    OnNodesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        private ObservableCollection<EdgeDto>? _edges;
        public ObservableCollection<EdgeDto>? Edges
        {
            get => _edges;
            set
            {
                if (_edges != null)
                {
                    _edges.CollectionChanged -= OnEdgesCollectionChanged;
                }

                if (SetProperty(ref _edges, value) && _edges != null)
                {
                    _edges.CollectionChanged += OnEdgesCollectionChanged;
                    OnEdgesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        private bool _showDetails;
        public bool ShowDetails
        {
            get => _showDetails;
            set
            {
                if (SetProperty(ref _showDetails, value))
                {
                    // После изменения флага запросить перерисовку
                    RedrawRequested?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // Словарь позиций узлов (доступен для View через привязку)
        public ObservableCollection<NodePosition> NodePositions { get; } = new();

        public class NodePosition : ObservableObject
        {
            private int _nodeId;
            public int NodeId
            {
                get => _nodeId;
                set => SetProperty(ref _nodeId, value);
            }

            private double _x;
            public double X
            {
                get => _x;
                set => SetProperty(ref _x, value);
            }

            private double _y;
            public double Y
            {
                get => _y;
                set => SetProperty(ref _y, value);
            }

            public NodeDto? Node { get; set; }
        }

        // Событие для перерисовки View
        public event EventHandler? RedrawRequested;

        public GraphVisualizationViewModel()
        {
            // Инициализация
        }

        private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Nodes == null) return;

            // Обработка удаления
            if (e.OldItems != null)
            {
                foreach (NodeDto node in e.OldItems)
                {
                    node.PropertyChanged -= OnNodePropertyChanged;
                }
            }

            // Обработка добавления
            if (e.NewItems != null)
            {
                foreach (NodeDto node in e.NewItems)
                {
                    node.PropertyChanged += OnNodePropertyChanged;
                }
            }

            // Удаляем позиции для узлов, которых больше нет
            var toRemove = NodePositions.Where(p => !Nodes.Any(n => n.Id == p.NodeId)).ToList();
            foreach (var pos in toRemove)
            {
                NodePositions.Remove(pos);
            }

            // Добавляем позиции для новых узлов
            var existingIds = NodePositions.Select(p => p.NodeId).ToHashSet();
            var newNodes = Nodes.Where(n => !existingIds.Contains(n.Id)).ToList();

            if (newNodes.Any())
            {
                var random = new Random();
                foreach (var node in newNodes)
                {
                    NodePositions.Add(new NodePosition
                    {
                        NodeId = node.Id,
                        Node = node,
                        X = random.Next(50, 700),
                        Y = random.Next(50, 400)
                    });
                }
            }

            RedrawRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnEdgesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RedrawRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnNodePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NodeDto.Status))
            {
                RedrawRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обновляет позицию узла после перетаскивания
        /// </summary>
        public void UpdateNodePosition(int nodeId, double x, double y)
        {
            var pos = NodePositions.FirstOrDefault(p => p.NodeId == nodeId);
            if (pos != null)
            {
                pos.X = x;
                pos.Y = y;
            }
        }

        /// <summary>
        /// Получает позицию узла по ID
        /// </summary>
        public NodePosition? GetNodePosition(int nodeId)
        {
            return NodePositions.FirstOrDefault(p => p.NodeId == nodeId);
        }
    }
}
