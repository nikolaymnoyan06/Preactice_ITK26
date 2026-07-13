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

        private const double NodeRadius = 20;      // половина ширины эллипса (у вас 40x40)
        private const double MinDistance = 50;     // минимальное расстояние между центрами

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

        /// <summary>
        /// Пытается найти случайную позицию, которая не пересекается с уже занятыми.
        /// </summary>
        private Point FindFreePosition(List<Point> occupiedPositions, int maxAttempts = 100)
        {
            var width = Math.Max(GraphCanvas.Bounds.Width, 200);
            var height = Math.Max(GraphCanvas.Bounds.Height, 200);
            var margin = 50;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = new Point(
                    Random.Shared.Next(margin, (int)width - margin),
                    Random.Shared.Next(margin, (int)height - margin)
                );

                bool isFree = true;
                foreach (var pos in occupiedPositions)
                {
                    var dx = candidate.X - pos.X;
                    var dy = candidate.Y - pos.Y;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance < MinDistance)
                    {
                        isFree = false;
                        break;
                    }
                }
                if (isFree)
                    return candidate;
            }

            // Если не нашли – возвращаем центр с небольшим смещением
            return new Point(width / 2 + Random.Shared.Next(-30, 30), height / 2 + Random.Shared.Next(-30, 30));
        }


        /// <summary>
        /// Располагает все узлы равномерно по окружности.
        /// </summary>
        private void LayoutCircle()
        {
            if (Nodes == null || Nodes.Count == 0) return;

            double centerX = GraphCanvas.Bounds.Width / 2;
            double centerY = GraphCanvas.Bounds.Height / 2;
            double radius = Math.Min(centerX, centerY) * 0.8;
            if (radius < 50) radius = 50; // минимальный радиус

            int count = Nodes.Count;
            for (int i = 0; i < count; i++)
            {
                double angle = 2 * Math.PI * i / count;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                var node = Nodes[i];
                _nodePositions[node.Id] = new Point(x, y);
            }
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
                // Удаляем позиции для узлов, которых больше нет
                var toRemove = _nodePositions.Keys.Where(id => !Nodes.Any(n => n.Id == id)).ToList();
                foreach (var id in toRemove)
                    _nodePositions.Remove(id);

                // Собираем узлы, у которых ещё нет позиции (новые)
                var newNodes = Nodes.Where(n => !_nodePositions.ContainsKey(n.Id)).ToList();
                if (newNodes.Any())
                {
                    // Если новых узлов больше 5 – перестраиваем все узлы по кругу
                    if (newNodes.Count > 5)
                    {
                        // Сохраняем текущие позиции только для существующих узлов (если они есть)
                        // и добавляем новые, но проще сразу разложить всё по кругу.
                        // При этом все старые узлы тоже переедут – это нормально при массовом добавлении.
                        LayoutCircle();
                    }
                    else
                    {
                        // Иначе используем поиск свободного места (как раньше)
                        var occupied = new List<Point>(_nodePositions.Values);
                        foreach (var node in newNodes)
                        {
                            var newPos = FindFreePosition(occupied);
                            _nodePositions[node.Id] = newPos;
                            occupied.Add(newPos);
                        }
                    }
                }
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

                    // Рисуем стрелку на конце (у target)
                    DrawArrow(canvas, sourcePos, targetPos, _edgePen.Brush);

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

        private void DrawArrow(Canvas canvas, Point from, Point to, IBrush brush, double arrowSize = 12)
        {
            var direction = to - from;
            // Вычисляем длину вектора вручную
            var length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (length < 1) return; // слишком короткое ребро

            var dir = new Point(direction.X / length, direction.Y / length);
            double nodeRadius = 20; // половина ширины/высоты эллипса (у вас 40x40)
            var tip = to - dir * nodeRadius;
            var base1 = tip - dir * arrowSize + new Point(-dir.Y, dir.X) * (arrowSize * 0.4);
            var base2 = tip - dir * arrowSize - new Point(-dir.Y, dir.X) * (arrowSize * 0.4);

            // Создаём коллекцию точек (надёжный способ)
            var points = new Avalonia.Points();
            points.Add(tip);
            points.Add(base1);
            points.Add(base2);

            var polygon = new Avalonia.Controls.Shapes.Polygon
            {
                Points = points,
                Fill = brush,
                Stroke = brush,
                StrokeThickness = 1
            };
            canvas.Children.Add(polygon);
        }

    }
}