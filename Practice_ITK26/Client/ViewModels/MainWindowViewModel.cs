using Client.Dtos;
using Client.Models.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class MainWindowViewModel : ObservableObject
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

        private string _newNodeValue = "";
        public string NewNodeValue
        {
            get => _newNodeValue;
            set => SetProperty(ref _newNodeValue, value);
        }

        // Выбранный узел для удаления
        private NodeDto _selectedNode;
        public NodeDto SelectedNode
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

        private EdgeDto _selectedEdge;
        public EdgeDto SelectedEdge
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
        // ========== НОВОЕ: поле для количества генерируемых узлов ==========
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
        public IRelayCommand GenerateNodesCommand { get; } // НОВАЯ КОМАНДА

        // Базовый адрес сервиса
        private readonly string _baseUrl = "http://localhost:5000";

        public MainWindowViewModel()
        {
            System.Diagnostics.Debug.WriteLine("=== ViewModel создан ===");

            LoadNodesCommand = new RelayCommand(async () => await LoadNodesAsync());
            AddNodeCommand = new RelayCommand(async () => await AddNodeAsync(), () => CanAddNode());
            DeleteNodeCommand = new RelayCommand(async () => await DeleteNodeAsync(), () => SelectedNode != null);

            // ========== НОВОЕ: инициализация команды генерации ==========
            GenerateNodesCommand = new RelayCommand(
                async () => await GenerateNodesAsync(),
                () => CanGenerate()
            );
            LoadEdgesCommand = new RelayCommand(async () => await LoadEdgesAsync());
            AddEdgeCommand = new RelayCommand(async () => await AddEdgeAsync());
            DeleteEdgeCommand = new RelayCommand(async () => await DeleteEdgeAsync(), () => SelectedEdge != null);
            // При запуске сразу загружаем узлы
            LoadNodesCommand.Execute(null);
            LoadEdgesCommand.Execute(null);
        }

        private async Task LoadNodesAsync()
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{_baseUrl}/api/nodes");
                if (response.IsSuccessStatusCode)
                {
                    var nodes = await response.Content.ReadFromJsonAsync<NodeDto[]>();
                    Nodes.Clear();
                    foreach (var node in nodes ?? Array.Empty<NodeDto>())
                    {
                        Nodes.Add(node);
                    }
                    Greeting = $"Узлов: {Nodes.Count}";
                }
                else
                {
                    Greeting = "Ошибка загрузки узлов";
                }
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
                System.Diagnostics.Debug.WriteLine("=== AddNodeAsync вызван ===");

                if (!int.TryParse(NewNodeId, out int id))
                {
                    Greeting = "ID должен быть числом";
                    System.Diagnostics.Debug.WriteLine("Ошибка: ID не число");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"ID: {id}, Value: {NewNodeValue}");

                System.Diagnostics.Debug.WriteLine($"Перед отправкой: SelectedNodeType = {SelectedNodeType}");

                var dto = new NodeDto { Id = id, Value = NewNodeValue, Type = SelectedNodeType };
                using var httpClient = new HttpClient();

                System.Diagnostics.Debug.WriteLine($"Отправка POST на {_baseUrl}/api/nodes");
                var response = await httpClient.PostAsJsonAsync($"{_baseUrl}/api/nodes", dto);

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Статус ответа: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Тело ответа: {content}");

                if (response.IsSuccessStatusCode)
                {
                    await LoadNodesAsync();
                    NewNodeId = "";
                    NewNodeValue = "";
                    Greeting = "Узел добавлен";
                }
                else
                {
                    Greeting = $"Ошибка добавления: {content}";
                }
            }
            catch (Exception ex)
            {
                Greeting = $"Исключение: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ИСКЛЮЧЕНИЕ: {ex}");
            }
        }

        private async Task DeleteNodeAsync()
        {
            try
            {
                if (SelectedNode == null) return;

                using var httpClient = new HttpClient();
                var response = await httpClient.DeleteAsync($"{_baseUrl}/api/nodes/{SelectedNode.Id}");
                if (response.IsSuccessStatusCode)
                {
                    await LoadNodesAsync();
                    SelectedNode = null;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Greeting = $"Ошибка удаления: {error}";
                }
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
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{_baseUrl}/api/edges");
                if (response.IsSuccessStatusCode)
                {
                    var edges = await response.Content.ReadFromJsonAsync<EdgeDto[]>();
                    Edges.Clear();
                    foreach (var edge in edges ?? Array.Empty<EdgeDto>())
                    {
                        Edges.Add(edge);
                    }
                }
                else
                {
                    Greeting = "Ошибка загрузки рёбер";
                }
            }
            catch (Exception ex)
            {
                Greeting = $"Ошибка: {ex.Message}";
            }
        }


        private async Task AddEdgeAsync()
        {
            try
            {
                string sourceStr = NewEdgeSource?.Trim() ?? "";
                string targetStr = NewEdgeTarget?.Trim() ?? "";
                string weightStr = NewEdgeWeight?.Trim() ?? "1";

                System.Diagnostics.Debug.WriteLine($"AddEdgeAsync: source='{sourceStr}', target='{targetStr}', weight='{weightStr}'");

                if (!int.TryParse(sourceStr, out int sourceId))
                {
                    Greeting = "Source ID должен быть числом";
                    return;
                }

                if (!int.TryParse(targetStr, out int targetId))
                {
                    Greeting = "Target ID должен быть числом";
                    return;
                }

                if (!double.TryParse(weightStr, out double weight) || weight <= 0)
                {
                    Greeting = "Вес должен быть положительным числом";
                    return;
                }

                var dto = new EdgeDto { SourceId = sourceId, TargetId = targetId, Weight = weight };
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync($"{_baseUrl}/api/edges", dto);

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Ответ сервера: {response.StatusCode} - {content}");

                if (response.IsSuccessStatusCode)
                {
                    await LoadEdgesAsync();
                    NewEdgeSource = "";
                    NewEdgeTarget = "";
                    NewEdgeWeight = "1";
                    Greeting = "Ребро добавлено";
                }
                else
                {
                    Greeting = $"Ошибка: {content}";
                }
            }
            catch (Exception ex)
            {
                Greeting = $"Исключение: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ИСКЛЮЧЕНИЕ: {ex}");
            }
        }


        private async Task DeleteEdgeAsync()
        {
            try
            {
                if (SelectedEdge == null) return;

                using var httpClient = new HttpClient();
                var response = await httpClient.DeleteAsync($"{_baseUrl}/api/edges/{SelectedEdge.Id}");
                if (response.IsSuccessStatusCode)
                {
                    await LoadEdgesAsync();
                    SelectedEdge = null;
                    Greeting = "Ребро удалено";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Greeting = $"Ошибка: {error}";
                }
            }
            catch (Exception ex)
            {
                Greeting = $"Исключение: {ex.Message}";
            }
        }

        private bool CanAddEdge()
        {
            bool sourceOk = int.TryParse(NewEdgeSource, out int source);
            bool targetOk = int.TryParse(NewEdgeTarget, out int target);
            bool weightOk = double.TryParse(NewEdgeWeight, out double weight) && weight > 0;
            bool result = sourceOk && targetOk && weightOk;

            System.Diagnostics.Debug.WriteLine($"CanAddEdge: source='{NewEdgeSource}' ({sourceOk}), target='{NewEdgeTarget}' ({targetOk}), weight='{NewEdgeWeight}' ({weightOk}) -> {result}");
            return result;
        }

        //МЕТОДЫ ДЛЯ ГЕНЕРАЦИИ

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

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync($"{_baseUrl}/api/generate-nodes?count={count}", null);
                if (response.IsSuccessStatusCode)
                {
                    Greeting = $"Сгенерировано {count} узлов";
                    await LoadNodesAsync(); // обновляем список
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Greeting = $"Ошибка: {error}";
                }
            }
            catch (Exception ex)
            {
                Greeting = $"Исключение: {ex.Message}";
            }
        }

        private bool CanAddNode()
        {
            var result = !string.IsNullOrWhiteSpace(NewNodeId) && int.TryParse(NewNodeId, out _);
            System.Diagnostics.Debug.WriteLine($"CanAddNode: {result} (NewNodeId='{NewNodeId}')");
            return result;
        }
    }
}