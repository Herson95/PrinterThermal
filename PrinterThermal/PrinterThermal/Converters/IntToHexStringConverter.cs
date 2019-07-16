namespace PrinterThermal.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class IntToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num = 0;
            var values = value.ToString();
            var splits = values.Split(' ');
            bool canConvert = int.TryParse(splits[0], out num);
            if (canConvert)
            {
                return $"USB:{int.Parse(splits[0]).ToString("X2")}:{int.Parse(splits[1]).ToString("X2")}";
            }
            return values;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
