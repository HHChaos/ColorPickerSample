using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ILaiHua.UWP.App.Converters
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null) return "";
            SolidColorBrush brush = value as SolidColorBrush;
            string type = parameter as string;
            switch (type)
            {
                case "HEX":
                    return brush.Color.ToString();
                case "R":
                    return brush.Color.R.ToString();
                case "G":
                    return brush.Color.G.ToString();
                case "B":
                    return brush.Color.B.ToString();
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
