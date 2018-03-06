using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Bacc_background
{
    //[PropertyChanged.ImplementPropertyChanged]
    //public class WhoWin : INotifyPropertyChanged
    //{
    //    private Winner winnerSide;
    //    public WhoWin()
    //    {
    //        WinnerSide = Winner.none;
    //    }

    //    public Winner WinnerSide
    //    {
    //        get { return winnerSide; }
    //        set
    //        {
    //            winnerSide = value;
    //            if (PropertyChanged != null)
    //            {
    //                PropertyChanged(this, new PropertyChangedEventArgs("WinnerSide"));
    //            }
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //}

    [PropertyChanged.ImplementPropertyChanged]
    public class WhoWin
    {
        public int Winner { get; set; }
        public WhoWin()
        {
            Winner = -1;
        }
    }

    public enum Winner
    {
        none = -1,
        banker,
        tie,
        player
    }
    public class WayBillBackgroundConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var side = System.Convert.ToInt32(value);
            if (side == 0)
            {
                return new SolidColorBrush(Colors.Red);
            }
            if (side == 2)
            {
                return new SolidColorBrush(Colors.Blue);
            }
            if (side == 1)
            {
                return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.AliceBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class WayBillContentConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var side = ((WhoWin)value).Winner;
            var side = System.Convert.ToInt32(value);
            switch (side)
            {
                case 0:
                    return "庄";
                case 2:
                    return "闲";
                case 1:
                    return "和";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class WayBillVisibleConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var side = ((WhoWin)value).Winner;
            var side = System.Convert.ToInt32(value);
            switch (side)
            {
                case 0:
                case 1:
                case 2:
                    return Visibility.Visible;
                case -1:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NextPageButtonEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var num = System.Convert.ToInt32(value);
            if(MainWindow.Instance.LocalSessions.Count - 1<= num)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
