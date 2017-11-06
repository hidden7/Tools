using System;
using System.Globalization;
using System.Windows.Data;

namespace vrClusterManager.ValueConversion
{
	public class BoolToHorizontalBorderConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool val = System.Convert.ToBoolean(value);
			if (val)
			{
				return System.Windows.HorizontalAlignment.Left;
			}
			else
			{
				return System.Windows.HorizontalAlignment.Right;
			}

		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
