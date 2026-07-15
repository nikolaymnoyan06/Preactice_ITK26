using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Client.Dtos;
using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Views
{
    public partial class GraphVisualization : UserControl
    {
        private GraphVisualizationViewModel? ViewModel => DataContext as GraphVisualizationViewModel;

        private bool _isDragging;
        private Point _dragStartPoint;
        private int? _draggedNodeId;

        private readonly SolidColorBrush _nodeBrush = new(Colors.LightBlue);
        private readonly SolidColorBrush _nodeBorderBrush = new(Colors.DarkBlue);
        private readonly Pen _edgePen = new(Brushes.Gray, 2);
        private readonly Pen _selectedNodePen = new(Brushes.Red, 3);

        private readonly SolidColorBrush _okBrush = new(Colors.LightGreen);
        private readonly SolidColorBrush _notOkBrush = new(Colors.LightCoral);
        private readonly SolidColorBrush _notStatedBrush = new(Colors.LightGray);

        private const double NodeRadius = 20;

        public GraphVisualization()
        {
            InitializeComponent();

            GraphCanvas.PointerPressed += OnCanvasPointerPressed;
            GraphCanvas.PointerMoved += OnCanvasPointerMoved;
            GraphCanvas.PointerReleased += OnCanvasPointerReleased;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (ViewModel != null)
            {
                ViewModel.RedrawRequested += OnRedrawRequested;
                Redraw();
            }
        }

        private void OnRedrawRequested(object? sender, EventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            var canvas = GraphCanvas;
            canvas.Children.Clear();

            if (ViewModel?.Nodes == null || ViewModel?.Edges == null) return;

            // Рёбра
            foreach (var edge in ViewModel.Edges)
            {
                var sourcePos = ViewModel.GetNodePosition(edge.SourceId);
                var targetPos = ViewModel.GetNodePosition(edge.TargetId);

                if (sourcePos != null && targetPos != null)
                {
                    var sourcePoint = new Point(sourcePos.X, sourcePos.Y);
                    var targetPoint = new Point(targetPos.X, targetPos.Y);

                    var line = new Avalonia.Controls.Shapes.Line
                    {
                        StartPoint = sourcePoint,
                        EndPoint = targetPoint,
                        Stroke = _edgePen.Brush,
                        StrokeThickness = _edgePen.Thickness,
                    };
                    canvas.Children.Add(line);

                    DrawArrow(canvas, sourcePoint, targetPoint, _edgePen.Brush);

                    // Вес ребра
                    var weightText = edge.Weight.ToString("0.0");
                    var midX = (sourcePoint.X + targetPoint.X) / 2;
                    var midY = (sourcePoint.Y + targetPoint.Y) / 2;
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
            foreach (var nodePos in ViewModel.NodePositions)
            {
                var node = nodePos.Node;
                if (node == null) continue;

                var pos = new Point(nodePos.X, nodePos.Y);

                // Цвет заливки в зависимости от статуса
                SolidColorBrush fillBrush;
                if (node.Status?.Contains("NOT OK") == true)
                    fillBrush = _notOkBrush;
                else if (node.Status?.Contains("OK") == true)
                    fillBrush = _okBrush;
                else
                    fillBrush = _notStatedBrush;

                // Эллипс (круг) узла
                var ellipse = new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = 40,
                    Height = 40,
                    Fill = fillBrush,
                    Stroke = _nodeBorderBrush,
                    StrokeThickness = 2,
                    Tag = node.Id
                };
                Canvas.SetLeft(ellipse, pos.X - 20);
                Canvas.SetTop(ellipse, pos.Y - 20);
                canvas.Children.Add(ellipse);

                // Текст: Имя - Статус
                string statusText = node.Status ?? "";
                string shortStatus;
                if (statusText.Contains("NOT OK"))
                    shortStatus = "NOT OK";
                else if (statusText.Contains("OK"))
                    shortStatus = "OK";
                else
                    shortStatus = "NOT STATED";
                string displayText = $"{node.Name} - {shortStatus}";

                var text = new TextBlock
                {
                    Text = displayText,
                    FontSize = 10,
                    Foreground = Brushes.Black,
                };
                Canvas.SetLeft(text, pos.X - 30);
                Canvas.SetTop(text, pos.Y + 22);
                canvas.Children.Add(text);

                // Детали над узлом
                if (ViewModel.ShowDetails)
                {
                    string typeAbbr = node.Type.ToString().ToUpper();
                    if (typeAbbr == "CONSUMER") typeAbbr = "CONS";
                    else if (typeAbbr == "SOURCE") typeAbbr = "SOUR";
                    else if (typeAbbr == "TRANSITIVE") typeAbbr = "TRANS";

                    var details = new TextBlock
                    {
                        Text = $"ID{node.Id} - {typeAbbr}",
                        FontSize = 9,
                        Foreground = Brushes.DarkBlue,
                        FontWeight = FontWeight.Bold,
                        Background = new SolidColorBrush(Colors.WhiteSmoke)
                    };
                    Canvas.SetLeft(details, pos.X - 25);
                    Canvas.SetTop(details, pos.Y - 40);
                    canvas.Children.Add(details);
                }
            }
        }

        // ========== Перетаскивание ==========
        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel == null) return;

            var point = e.GetPosition(GraphCanvas);
            foreach (var nodePos in ViewModel.NodePositions)
            {
                var pos = new Point(nodePos.X, nodePos.Y);
                if (Math.Abs(point.X - pos.X) < 20 && Math.Abs(point.Y - pos.Y) < 20)
                {
                    _isDragging = true;
                    _draggedNodeId = nodePos.NodeId;
                    _dragStartPoint = point;
                    break;
                }
            }
        }

        private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _draggedNodeId.HasValue && ViewModel != null)
            {
                var currentPoint = e.GetPosition(GraphCanvas);
                var delta = currentPoint - _dragStartPoint;

                var pos = ViewModel.GetNodePosition(_draggedNodeId.Value);
                if (pos != null)
                {
                    // Новая позиция без ограничений
                    var newX = pos.X + delta.X;
                    var newY = pos.Y + delta.Y;

                    // ============================================================
                    //     ОГРАНИЧЕНИЕ ПЕРЕМЕЩЕНИЯ В ПРЕДЕЛАХ CANVAS 
                    // Бездарный ты кусок мяса хоть бы строчку сам написал
                    // ============================================================
                    // Получаем размеры Canvas
                    double canvasWidth = GraphCanvas.Bounds.Width;
                    double canvasHeight = GraphCanvas.Bounds.Height;

                    // Радиус узла (половина ширины/высоты = 20)
                    const double nodeRadius = 20;

                    // Ограничиваем X (не меньше 0 + радиус, не больше ширины - радиус)
                    newX = Math.Max(nodeRadius, Math.Min(canvasWidth - nodeRadius, newX));

                    // Ограничиваем Y (не меньше 0 + радиус, не больше высоты - радиус)
                    newY = Math.Max(nodeRadius, Math.Min(canvasHeight - nodeRadius, newY));
                    // ============================================================

                    ViewModel.UpdateNodePosition(_draggedNodeId.Value, newX, newY);
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

        // ========== Вспомогательные методы ==========
        private void DrawArrow(Canvas canvas, Point from, Point to, IBrush brush, double arrowSize = 12)
        {
            var direction = to - from;
            var length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (length < 1) return;

            var dir = new Point(direction.X / length, direction.Y / length);
            var tip = to - dir * NodeRadius;
            var base1 = tip - dir * arrowSize + new Point(-dir.Y, dir.X) * (arrowSize * 0.4);
            var base2 = tip - dir * arrowSize - new Point(-dir.Y, dir.X) * (arrowSize * 0.4);

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