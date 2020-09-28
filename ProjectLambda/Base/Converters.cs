using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLambda.Base
{
    [ValueConversion(typeof(CopyJobState), typeof(string))]
    public class StateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (CopyJobState)value;
            switch (state)
            {
                case CopyJobState.Failed:
                    return "Failed";
                case CopyJobState.Finished:
                    return "Finished";
                case CopyJobState.InProgress:
                    return "In progress";
                case CopyJobState.Registered:
                    return "Awaiting start";
                default:
                    return "Unknown -> Contact Moe";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(CopyJobState), typeof(SolidColorBrush))]
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (CopyJobState)value;
            switch (state)
            {
                case CopyJobState.Failed:
                    return new SolidColorBrush(Colors.Firebrick);
                case CopyJobState.Finished:
                    return new SolidColorBrush(Colors.ForestGreen);
                case CopyJobState.InProgress:
                    return new SolidColorBrush(Colors.DarkGoldenrod);
                case CopyJobState.Registered:
                    return new SolidColorBrush(Colors.RoyalBlue);
                default:
                    return new SolidColorBrush(Colors.LightGray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LogLevel), typeof(SolidColorBrush))]
    public class LogLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = FromText((string)value);
            switch (level)
            {
                case LogLevel.CRITICAL:
                    return new SolidColorBrush(Colors.Firebrick);
                case LogLevel.DEBUG:
                    return new SolidColorBrush(Colors.AliceBlue);
                case LogLevel.ERROR:
                    return new SolidColorBrush(Colors.OrangeRed);
                case LogLevel.EXCEPTION:
                    return new SolidColorBrush(Colors.DarkRed);
                case LogLevel.INFO:
                    return new SolidColorBrush(Colors.Transparent);
                case LogLevel.WARNING:
                    return new SolidColorBrush(Colors.DarkOrange);
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static LogLevel FromText(string value)
        {
            foreach(var val in (LogLevel[])Enum.GetValues(typeof(LogLevel)))
            {
                if (value.Contains($"[{val}]"))
                {
                    return val;
                }
            }
            return LogLevel.INFO;
        }
    }
}
