using Client.Dtos;
using Client.Models;

using Client.Models.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private string _greeting = "Граф";
        public string Greeting
        {
            get => _greeting;
            set => SetProperty(ref _greeting, value);
        }

        // Коллекция узлов для отображения
        private ObservableCollection<NodeDto> _nodes = new ObservableCollection<NodeDto>();
        public ObservableCollection<NodeDto> Nodes
        {
            get => _nodes;
            set => SetProperty(ref _nodes, value);
        }

        // Поля для ввода нового узла
        private string _newNodeId = "";
        public string NewNodeId
        {
            get => _newNodeId;
            set
            {
                if (SetProperty(ref _newNodeId, value))
                {
                    AddNodeCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private string _newNodeName = "";
        public string NewNodeName
        {
            get => _newNodeName;
            set => SetProperty(ref _newNodeName, value);
        }

        private string _newNodeInValue = "0";
        public string NewNodeInValue
        {
            get => _newNodeInValue;
            set => SetProperty(ref _newNodeInValue, value);
        }

        private string _newNodeOutValue = "0";
        public string NewNodeOutValue
        {
            get => _newNodeOutValue;
            set => SetProperty(ref _newNodeOutValue, value);
        }

        // Выбранный узел для удаления
        private NodeDto? _selectedNode;
        public NodeDto? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetProperty(ref _selectedNode, value))
                {
                    DeleteNodeCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        // Список типов для выпадающего списка
        public ObservableCollection<NodeType> NodeTypes { get; } = new ObservableCollection<NodeType>(
            Enum.GetValues(typeof(NodeType)).Cast<NodeType>()
        );

        private NodeType _selectedNodeType = NodeType.Transitive;
        public NodeType SelectedNodeType
        {
            get => _selectedNodeType;
            set => SetProperty(ref _selectedNodeType, value);
        }

        // ========== РЁБРА ==========
        private ObservableCollection<EdgeDto> _edges = new ObservableCollection<EdgeDto>();
        public ObservableCollection<EdgeDto> Edges
        {
            get => _edges;
            set => SetProperty(ref _edges, value);
        }

        private string _newEdgeSource = "";
        public string NewEdgeSource
        {
            get => _newEdgeSource;
            set
            {
                if (SetProperty(ref _newEdgeSource, value))
                {
                    AddEdgeCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private string _newEdgeTarget = "";
        public string NewEdgeTarget
        {
            get => _newEdgeTarget;
            set
            {
                if (SetProperty(ref _newEdgeTarget, value))
                {
                    AddEdgeCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private string _newEdgeWeight = "1";
        public string NewEdgeWeight
        {
            get => _newEdgeWeight;
            set
            {
                if (SetProperty(ref _newEdgeWeight, value))
                {
                    AddEdgeCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private EdgeDto? _selectedEdge;
        public EdgeDto? SelectedEdge
        {
            get => _selectedEdge;
            set
            {
                if (SetProperty(ref _selectedEdge, value))
                {
                    DeleteEdgeCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        // ========== поле для количества генерируемых узлов ==========
        private string _generateCount = "10";
        public string GenerateCount
        {
            get => _generateCount;
            set
            {
                if (SetProperty(ref _generateCount, value))
                {
                    GenerateNodesCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        // Команды
        public IRelayCommand LoadNodesCommand { get; }
        public IRelayCommand AddNodeCommand { get; }
        public IRelayCommand DeleteNodeCommand { get; }
        public IRelayCommand LoadEdgesCommand { get; }
        public IRelayCommand AddEdgeCommand { get; }
        public IRelayCommand DeleteEdgeCommand { get; }
        public IRelayCommand GenerateNodesCommand { get; }
        public IRelayCommand UpdateStatusesCommand { get; }

        // Базовый адрес сервиса
        private readonly global::Graph.GraphService.GraphServiceClient _grpcClient;

        public MainWindowViewModel()
        {
            System.Diagnostics.Debug.WriteLine("=== ViewModel создан ===");
            var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions
            {
                HttpHandler = new HttpClientHandler()
            });
            _grpcClient = new global::Graph.GraphService.GraphServiceClient(channel);
            _grpcClient = new Graph.GraphService.GraphServiceClient(channel);

            LoadNodesCommand = new RelayCommand(async () => await LoadNodesAsync());
            AddNodeCommand = new RelayCommand(async () => await AddNodeAsync(), () => CanAddNode());
            DeleteNodeCommand = new RelayCommand(async () => await DeleteNodeAsync(), () => SelectedNode != null);

            GenerateNodesCommand = new RelayCommand(
                async () => await GenerateNodesAsync(),
                () => CanGenerate()
            );
            LoadEdgesCommand = new RelayCommand(async () => await LoadEdgesAsync());
            AddEdgeCommand = new RelayCommand(async () => await AddEdgeAsync(), () => CanAddEdge());
            DeleteEdgeCommand = new RelayCommand(async () => await DeleteEdgeAsync(), () => SelectedEdge != null);

            // команда логики статуса
            UpdateStatusesCommand = new RelayCommand(UpdateNodeStatuses);

            GraphVisualizationViewModel.Nodes = Nodes;
            GraphVisualizationViewModel.Edges = Edges;

            LoadNodesCommand.Execute(null);
            LoadEdgesCommand.Execute(null);
        }

        private async Task LoadNodesAsync()
        {
            try
            {
                var response = await _grpcClient.GetNodesAsync(new global::Graph.Empty());
                Nodes.Clear();
                foreach (var protoNode in response.Nodes)
                    Nodes.Add(protoNode.ToClientDto());
                Greeting = $"Узлов: {Nodes.Count}";
                UpdateNodeStatuses();
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }

        private async Task AddNodeAsync()
        {
            try
            {
                if (!int.TryParse(NewNodeId, out int id) ||
                    !double.TryParse(NewNodeInValue, out double inValue) ||
                    !double.TryParse(NewNodeOutValue, out double outValue))
                {
                    Greeting = "ID или значение не число";
                    return;
                }

                var clientNode = new NodeDto // это Client.Dtos.NodeDto
                {
                    Id = id,
                    Name = NewNodeName,
                    InValue = inValue,
                    OutValue = outValue,
                    Type = SelectedNodeType
                };
                var request = new global::Graph.AddNodeRequest { Node = clientNode.ToProto() };
                await _grpcClient.AddNodeAsync(request);
                await LoadNodesAsync();

                NewNodeId = "";
                NewNodeName = "";
                NewNodeInValue = "0";
                NewNodeOutValue = "0";
                Greeting = "Узел добавлен";
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }

        private async Task DeleteNodeAsync()
        {
            try
            {
                if (SelectedNode == null) return;
                var request = new global::Graph.DeleteNodeRequest { Id = SelectedNode.Id };
                await _grpcClient.DeleteNodeAsync(request);
                await LoadNodesAsync();
                SelectedNode = null;
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        // ========== МЕТОДЫ ДЛЯ РЁБЕР ==========

        private async Task LoadEdgesAsync()
        {
            try
            {
                var response = await _grpcClient.GetEdgesAsync(new global::Graph.Empty());
                Edges.Clear();
                foreach (var protoEdge in response.Edges)
                    Edges.Add(protoEdge.ToClientDto());
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка загрузки рёбер: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка загрузки рёбер: {ex.Message}";
            }
        }

        private async Task AddEdgeAsync()
        {
            try
            {
                string sourceStr = NewEdgeSource?.Trim() ?? "";
                string targetStr = NewEdgeTarget?.Trim() ?? "";
                string weightStr = NewEdgeWeight?.Trim() ?? "1";

                if (!int.TryParse(sourceStr, out int sourceId) ||
                    !int.TryParse(targetStr, out int targetId) ||
                    !double.TryParse(weightStr, out double weight) || weight <= 0)
                {
                    Greeting = "Некорректные данные ребра";
                    return;
                }

                var clientEdge = new EdgeDto { SourceId = sourceId, TargetId = targetId, Weight = weight, Id = 0 };
                var request = new global::Graph.AddEdgeRequest { Edge = clientEdge.ToProto() };
                await _grpcClient.AddEdgeAsync(request);
                await LoadEdgesAsync();
                NewEdgeSource = "";
                NewEdgeTarget = "";
                NewEdgeWeight = "1";
                Greeting = "Ребро добавлено";
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }

        private async Task DeleteEdgeAsync()
        {
            try
            {
                if (SelectedEdge == null) return;
                var request = new global::Graph.DeleteEdgeRequest { Id = SelectedEdge.Id };
                await _grpcClient.DeleteEdgeAsync(request);
                await LoadEdgesAsync();
                SelectedEdge = null;
                Greeting = "Ребро удалено";
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }

        private bool CanAddEdge()
        {
            bool sourceOk = int.TryParse(NewEdgeSource, out int source);
            bool targetOk = int.TryParse(NewEdgeTarget, out int target);
            bool weightOk = double.TryParse(NewEdgeWeight, out double weight) && weight > 0;
            return sourceOk && targetOk && weightOk;
        }

        // ========== МЕТОДЫ ДЛЯ ГЕНЕРАЦИИ ==========

        private bool CanGenerate()
        {
            return int.TryParse(GenerateCount, out int count) && count > 0;
        }

        private async Task GenerateNodesAsync()
        {
            try
            {
                if (!int.TryParse(GenerateCount, out int count) || count <= 0)
                {
                    Greeting = "Введите положительное число";
                    return;
                }
                var request = new global::Graph.GenerateNodesRequest { Count = count };
                var response = await _grpcClient.GenerateNodesAsync(request);
                Greeting = response.Message;
                await LoadNodesAsync();
            }
            catch (RpcException ex)
            {
                Greeting = $"Ошибка: {ex.Status.Detail}";
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }

        private bool CanAddNode()
        {
            var result = !string.IsNullOrWhiteSpace(NewNodeId) && int.TryParse(NewNodeId, out _);
            return result;
        }

        #region Методы для обновления статусов (по In/Out и типу)

        /// <summary>
        /// Обновляет статусы всех узлов на основе их Входящего/Выходящего значения и типа.
        /// Логика вынесена в отдельный класс NodeStatusEvaluator.
        /// </summary>
        private void UpdateNodeStatuses()
        {
            if (Nodes.Count == 0) return;

            foreach (var node in Nodes)
            {
                node.Status = Client.Models.NodeStatusEvaluator.EvaluateStatus(node);
            }
        }

        #endregion

        private bool _showNodeDetails;
        public bool ShowNodeDetails
        {
            get => _showNodeDetails;
            set => SetProperty(ref _showNodeDetails, value);
        }

        private GraphVisualizationViewModel _graphVisualizationViewModel = new();
        public GraphVisualizationViewModel GraphVisualizationViewModel
        {
            get => _graphVisualizationViewModel;
            set => SetProperty(ref _graphVisualizationViewModel, value);
        }
    }
}