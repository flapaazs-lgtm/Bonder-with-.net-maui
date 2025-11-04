using System.Globalization;

namespace Bonder.Converters;

public class GenreTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return Application.Current.Resources.TryGetValue("White", out var color)
                ? color
                : Colors.White;
        }

        return Application.Current.Resources.TryGetValue("TextPrimary", out var textColor)
            ? textColor
            : Color.FromArgb("#3A3A3A");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}