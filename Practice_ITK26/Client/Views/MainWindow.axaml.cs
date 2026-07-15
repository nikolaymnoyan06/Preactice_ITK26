using Avalonia.Controls;
using Client.ViewModels;

namespace Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Находим вкладку по имени и подписываемся на событие
            var tabControl = this.FindControl<TabControl>("MainTabControl");
            if (tabControl != null)
            {
                tabControl.SelectionChanged += OnTabSelectionChanged;
            }
        }

        private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Проверяем, переключились ли на вкладку "Визуализация"
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem selectedTab)
            {
                var header = selectedTab.Header?.ToString();
                if (header == "??? Визуализация" || header?.Contains("Визуализация") == true)
                {
                    // Получаем ViewModel и вызываем команду обновления узлов
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.LoadNodesCommand?.Execute(null);
                    }
                }
            }
        }
    }
}