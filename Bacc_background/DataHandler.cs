using Bacc_front;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Bacc_background
{
    public static class DataHandler
    {

        public static void InitBetRecordDataToBack()
        {
            MainWindow.Instance.BetRecordDataFromFront = new ObservableCollection<BackBetRecord>();
            foreach (var p in Desk.Instance.Players)
            {
                MainWindow.Instance.BetRecordDataFromFront.Add(new BackBetRecord()
                {
                    PlayerId = p.Id.ToString(),
                    BetScore = 0,
                    DingFen = 0,
                    Profit = 0,
                    ZhongFen = 0
                });
            }
        }
        public static void SetImage(int len, byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }
            try
            {
                ImageSourceConverter converter = new ImageSourceConverter();
                Stream ms = new MemoryStream(buffer);
                var bitmap = converter.ConvertFrom(ms) as BitmapFrame;
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //imgGame.Source = bitmap;
                }), DispatcherPriority.Render);
            }
            catch (Exception ex)
            {
            }
        }
        public static byte[] Decompress(Byte[] bytes, int len)//因为本例需求，我加了一个参数Len表示实际长度
        {
            try
            {
                using (MemoryStream tempMs = new MemoryStream())
                {
                    using (MemoryStream ms = new MemoryStream(bytes, 0, len))
                    {
                        GZipStream Decompress = new GZipStream(ms, CompressionMode.Decompress);
                        Decompress.CopyTo(tempMs);
                        Decompress.Close();
                        return tempMs.ToArray();
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
