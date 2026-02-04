using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ArmenRestauran.Converters
{
    public class DarkerBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;

                double factor = 0.8;
                if (parameter != null && double.TryParse(parameter.ToString(), out double customFactor))
                {
                    factor = customFactor;
                }

                var darkerColor = Color.FromRgb(
                    (byte)(color.R * factor),
                    (byte)(color.G * factor),
                    (byte)(color.B * factor)
                );
                return new SolidColorBrush(darkerColor);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
