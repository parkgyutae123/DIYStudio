using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SF_DIY
{
    [ValueConversion(typeof(Double), typeof(Decimal))]
    public class DoubleToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Decimal num = new decimal((Double)value);

            if (parameter != null)
            {
                num = Decimal.Round(num, Int32.Parse(parameter as String));
            }
            return num;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Decimal.ToDouble((Decimal)value);
        }
    }
}
