using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Converters
{
    class SelectedBookBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return Application.Current.Resources.TryGetValue("PrimaryAccent", out var color)
                    ? color
                    : Color.FromArgb("#D4A574");
            }
            return Color.FromArgb("#EEEEEE");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
