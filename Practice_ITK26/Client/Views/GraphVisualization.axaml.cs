using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Client.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Client.Views
{
    public partial class GraphVisualization : UserControl
    {
        private readonly Dictionary<int, Point> _nodePositions = new();

        private bool _isDragging;
        private Point _dragStartPoint;
        private int? _draggedNodeId;

        private readonly SolidColorBrush _nodeBrush = new(Colors.LightBlue);
        private readonly SolidColorBrush _nodeBorderBrush = new(Colors.DarkBlue);
        private readonly Pen _edgePen = new(Brushes.Gray, 2);
        private readonly Pen _selectedNodePen = new(Brushes.Red, 3);

        public GraphVisualization()
        {
            InitializeComponent();
            GraphCanvas.PointerPressed += OnCanvasPointerPressed;
            GraphCanvas.PointerMoved += OnCanvasPointerMoved;
            GraphCanvas.PointerReleased += OnCanvasPointerReleased;
        }

        public static readonly StyledProperty<ObservableCollection<NodeDto>?> NodesProperty =
            AvaloniaProperty.Register<GraphVisualization, ObservableCollection<NodeDto>?>(nameof(Nodes));

        public ObservableCollection<NodeDto>? Nodes
        {
            get => GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public static readonly StyledProperty<ObservableCollection<EdgeDto>?> EdgesProperty =
            AvaloniaProperty.Register<GraphVisualization, ObservableCollection<EdgeDto>?>(nameof(Edges));

        public ObservableCollection<EdgeDto>? Edges
        {
            get => GetValue(EdgesProperty);
            set => SetValue(EdgesProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == NodesProperty)
            {
                if (change.OldValue is ObservableCollection<NodeDto> oldNodes)
                    oldNodes.CollectionChanged -= OnNodesCollectionChanged;

                if (change.NewValue is ObservableCollection<NodeDto> newNodes)
                {
                    newNodes.CollectionChanged += OnNodesCollectionChanged;
                    OnNodesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }

            if (change.Property == EdgesProperty)
            {
                if (change.OldValue is ObservableCollection<EdgeDto> oldEdges)
                    oldEdges.CollectionChanged -= OnEdgesCollectionChanged;

                if (change.NewValue is ObservableCollection<EdgeDto> newEdges)
                {
                    newEdges.CollectionChanged += OnEdgesCollectionChanged;
                    OnEdgesCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        private void OnNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Nodes != null)
            {
                foreach (var node in Nodes)
                {
                    if (!_nodePositions.ContainsKey(node.Id))
                    {
                        _nodePositions[node.Id] = new Point(
                            Random.Shared.Next(50, (int)Math.Max(GraphCanvas.Bounds.Width, 100) - 50),
                            Random.Shared.Next(50, (int)Math.Max(GraphCanvas.Bounds.Height, 100) - 50)
                        );
                    }
                }

                var toRemove = _nodePositions.Keys.Where(id => !Nodes.Any(n => n.Id == id)).ToList();
                foreach (var id in toRemove)
                    _nodePositions.Remove(id);
            }

            Redraw();
        }

        private void OnEdgesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            var canvas = GraphCanvas;
            canvas.Children.Clear();

            if (Nodes == null || Edges == null) return;

            // Рёбра
            foreach (var edge in Edges)
            {
                if (_nodePositions.TryGetValue(edge.SourceId, out var sourcePos) &&
                    _nodePositions.TryGetValue(edge.TargetId, out var targetPos))
                {
                    var line = new Avalonia.Controls.Shapes.Line
                    {
                        StartPoint = sourcePos,
                        EndPoint = targetPos,
                        Stroke = _edgePen.Brush,
                        StrokeThickness = _edgePen.Thickness,
                    };
                    canvas.Children.Add(line);

                    // Подпись веса (теперь просто edge.Weight, без .Value)
                    var weightText = edge.Weight.ToString("0.0");
                    var midX = (sourcePos.X + targetPos.X) / 2;
                    var midY = (sourcePos.Y + targetPos.Y) / 2;
                    var text = new TextBlock
                    {
                        Text = weightText,
                        Foreground = Brushes.DarkGray,
                        FontSize = 10
                    };
                    Canvas.SetLeft(text, midX);
                    Canvas.SetTop(text, midY);
                    canvas.Children.Add(text);
                }
            }

            // Узлы
            foreach (var node in Nodes)
            {
                if (_nodePositions.TryGetValue(node.Id, out var pos))
                {
                    var ellipse = new Avalonia.Controls.Shapes.Ellipse
                    {
                        Width = 40,
                        Height = 40,
                        Fill = _nodeBrush,
                        Stroke = _nodeBorderBrush,
                        StrokeThickness = 2,
                        Tag = node.Id
                    };
                    Canvas.SetLeft(ellipse, pos.X - 20);
                    Canvas.SetTop(ellipse, pos.Y - 20);
                    canvas.Children.Add(ellipse);

                    var text = new TextBlock
                    {
                        Text = node.Id.ToString(),
                        FontSize = 12,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    Canvas.SetLeft(text, pos.X - 10);
                    Canvas.SetTop(text, pos.Y - 8);
                    canvas.Children.Add(text);
                }
            }
        }

        // ========== Перетаскивание ==========
        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetPosition(GraphCanvas);
            foreach (var kvp in _nodePositions)
            {
                var pos = kvp.Value;
                if (Math.Abs(point.X - pos.X) < 20 && Math.Abs(point.Y - pos.Y) < 20)
                {
                    _isDragging = true;
                    _draggedNodeId = kvp.Key;
                    _dragStartPoint = point;
                    break;
                }
            }
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _draggedNodeId.HasValue)
            {
                var currentPoint = e.GetPosition(GraphCanvas);
                var delta = currentPoint - _dragStartPoint;
                if (_nodePositions.ContainsKey(_draggedNodeId.Value))
                {
                    var oldPos = _nodePositions[_draggedNodeId.Value];
                    var newPos = new Point(oldPos.X + delta.X, oldPos.Y + delta.Y);
                    _nodePositions[_draggedNodeId.Value] = newPos;
                    _dragStartPoint = currentPoint;
                    Redraw();
                }
            }
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            _draggedNodeId = null;
        }
    }
}