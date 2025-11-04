using System.Globalization;

namespace Bonder.Converters;

public class GenreSelectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return Application.Current.Resources.TryGetValue("PrimaryAccent", out var color)
                ? color
                : Color.FromArgb("#D4A574");
        }

        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}