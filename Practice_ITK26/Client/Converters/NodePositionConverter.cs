using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Client.ViewModels;

namespace Client.Converters
{
    public class NodePositionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int nodeId && parameter is GraphVisualizationViewModel viewModel)
            {
                var pos = viewModel.GetNodePosition(nodeId);
                if (pos != null)
                {
                    // parameter: "X" или "Y"
                    if (parameter is string prop)
                    {
                        return prop == "X" ? pos.X : pos.Y;
                    }
                }
            }
            return 0.0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}