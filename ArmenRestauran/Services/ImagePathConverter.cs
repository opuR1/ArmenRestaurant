using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ArmenRestauran.Services
{
    public class ImagePathConverter : IValueConverter
    {
        private const string BaseImagePath = @"D:\Praktika2026-2\restaurant\ArmenRestauran\images\";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return GetDefaultImage();
            }

            string imageName = value.ToString();
            string fullPath = Path.Combine(BaseImagePath, imageName);

            if (File.Exists(fullPath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(fullPath);
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    return GetDefaultImage();
                }
            }
            else
            {
                return GetDefaultImage();
            }
        }

        private BitmapImage GetDefaultImage()
        {
            var defaultImage = new BitmapImage();
            defaultImage.BeginInit();
            defaultImage.UriSource = new Uri("pack://application:,,,/Resources/food_placeholder.png");
            defaultImage.EndInit();
            return defaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
