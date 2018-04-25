using System;
using System.Globalization;
using System.Windows.Data;

namespace vrClusterManager.ValueConversion
{
	public class BoolToVerticalBorderConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool val = System.Convert.ToBoolean(value);
			if (val)
			{
				return System.Windows.VerticalAlignment.Top;
			}
			else
			{
				return System.Windows.VerticalAlignment.Bottom;
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
