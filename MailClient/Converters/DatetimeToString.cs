using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MailClient.Converters
{
    public class DatetimeToString: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var d = value as DateTime?;
            if (!d.HasValue) return value;
            return d.Value.ToString("yyyy/MM/dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
