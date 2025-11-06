using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonder.Converters
{
    public class TabColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedTab = value as string;
            var thisTab = parameter as string;

            if (selectedTab == thisTab)
            {
                return Application.Current.Resources.TryGetValue("PrimaryAccent", out var color)
                    ? color
                    : Color.FromArgb("#D4A574");
            }

            return Application.Current.Resources.TryGetValue("TextSecondary", out var textColor)
                ? textColor
                : Color.FromArgb("#666666");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
