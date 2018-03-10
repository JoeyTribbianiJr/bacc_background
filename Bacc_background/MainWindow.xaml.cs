using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Data;
using SuperSocket.ClientEngine;
using System.Text;
using Newtonsoft.Json;
using System.IO.Compression;
using Bacc_front;
using System.Windows.Media;

namespace Bacc_background
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [PropertyChanged.ImplementPropertyChanged]
    public partial class MainWindow : Window
    {
        //[PropertyChanged.ImplementPropertyChanged]
        public BitmapFrame game_bitmap { get; set; }
        #region 网络变量
        private const int port = 54322;
        public IPAddress host_ip;
        public SuperClient SSClient;
        #endregion
        public ObservableCollection<WhoWin> Waybill { get; set; }
        public ObservableCollection<Session> LocalSessions { get; set; }
        public Session CurrentSession
        {
            get { return LocalSessions[LocalSessionIndex]; }
        }

        public int SessionPageIndex { get; set; }

        [PropertyChanged.ImplementPropertyChanged]
        public int LocalSessionIndex { get; set; }
        public int localSessionStrIndex { get { return (LocalSessionIndex + 1); } }

        private List<Button> sm_waybill_btns = new List<Button>();
        private int LocalRoundNum;

        public Dictionary<string, SettingItem> app_setting = new Dictionary<string, SettingItem>();
        public Dictionary<string, SettingItem> game_setting = new Dictionary<string, SettingItem>();

        public static MainWindow Instance { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;


            LocalSessions = new ObservableCollection<Bacc_front.Session>();
            Waybill = new ObservableCollection<WhoWin>();
            txtRoundNum.Text = 1000.ToString();

            spPageButtons.DataContext = this;

            LocalRoundNum = Setting.Instance.GetIntSetting("round_num_per_session");
            InitWaybillBindingForLocal();

            SSClient = new SuperClient();
            SSClient.DataReceived += Client_DataReceived;
        }
        private void InitWaybillBindingForLocal()
        {
            LocalSessions.Clear();
            var newsession = new Session(0);
            LocalSessions.Add(newsession);

            InitWaybillBinding(LocalRoundNum);

            SetWaybillFromSession(newsession);
            ResetSmWaybill();

            LocalSessionIndex = 0;
        }
        private void InitWaybillBinding(int round_num)
        {
            BigWaybill.Children.Clear();
            Waybill.Clear();
            sm_waybill_btns.Clear();
            grdSmallWaybill.Children.Clear();
            for (int i = 0; i < round_num; i++)
            {
                Waybill.Add(new WhoWin() { Winner = (int)WinnerEnum.none });

                //小路单按钮绑定
                var block = new Button();
                block.Style = (Style)Resources["SmallWaybillButton"];
                block.Tag = i;
                block.Click += BigWaybillBlockClick;
                grdSmallWaybill.Children.Add(block);
                sm_waybill_btns.Add(block);

                Binding myBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("Waybill[" + i + "].Winner"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(block, DataContextProperty, myBinding);

                //大路单按钮绑定
                var blockBig = new Button();
                blockBig.Style = (Style)Resources["BigWaybillButton"];
                blockBig.Tag = i;
                blockBig.Click += BigWaybillBlockClick;
                BigWaybill.Children.Add(blockBig);

                var bigBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("Waybill[" + i + "].Winner"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(blockBig, DataContextProperty, myBinding);

                var col = i / 6;
                var row = i % 6;
                Grid.SetRow(blockBig, row);
                Grid.SetColumn(blockBig, col);
            }
        }

        private void SetWaybillFromSession(Session session)
        {
            if (Waybill.Count == session.RoundNumber)
            {
                for (int i = 0; i < session.RoundNumber; i++)
                {
                    Waybill[i].Winner = (int)session.RoundsOfSession[i].Winner.Item1;
                }
            }
            else
            {
                Waybill.Clear();
                for (int i = 0; i < session.RoundNumber; i++)
                {
                    Waybill.Add(new WhoWin() { Winner = (int)session.RoundsOfSession[i].Winner.Item1 });
                }
            }
            Instance.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetSmWaybill));
        }
        private void BigWaybillBlockClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var idx = Convert.ToInt32(btn.Tag);
            UpdateWaybill(idx);
            ResetSmWaybill();
        }

        private void ResetSmWaybill()
        {
            int pre_row = 0, pre_col = 0, pre = 0;
            int cur_side = Waybill[0].Winner;

            sm_waybill_btns[0].SetValue(Grid.ColumnProperty, pre_row);
            sm_waybill_btns[0].SetValue(Grid.RowProperty, pre_col);

            for (int i = 1; i < Waybill.Count; i++)
            {
                var w_i = Waybill[i].Winner;
                var w_pre = Waybill[pre].Winner;

                if (w_pre == (int)Winner.tie)
                {
                    if (cur_side == (int)Winner.tie || w_i == (int)Winner.tie)
                    {
                        if (++pre_row > 10)
                        {
                            pre_row = 0;
                            pre_col++;
                        }
                        cur_side = w_i;
                    }
                    else
                    {
                        if (w_i == cur_side)
                        {
                            if (++pre_row > 10)
                            {
                                pre_row = 0;
                                pre_col++;
                            }
                        }
                        else
                        {
                            pre_row = 0;
                            pre_col++;
                            cur_side = w_i;
                        }
                    }
                }
                else
                {
                    if (w_i == (int)Winner.tie || w_i == w_pre)
                    {
                        if (++pre_row > 10)
                        {
                            pre_row = 0;
                            pre_col++;
                        }
                    }
                    else
                    {
                        pre_row = 0;
                        pre_col++;
                        cur_side = w_i;
                    }
                }


                sm_waybill_btns[i].SetValue(Grid.ColumnProperty, pre_col);
                sm_waybill_btns[i].SetValue(Grid.RowProperty, pre_row);
                pre++;
            }
        }

        private void UpdateWaybill(int idx)
        {
            var curWinner = Waybill[idx].Winner;

            var who = new WhoWin
            {
                Winner = SwitchWinner(curWinner),
            };
            Waybill[idx] = who;

            CurrentSession.RoundsOfSession[idx] = Session.CreateRoundByWinner((BetSide)who.Winner);
        }
        private int SwitchWinner(int curWinner)
        {
            switch (curWinner)
            {
                case 0:
                    return 2;
                case 2:
                    return 1;
                case 1:
                    return 0;
                default:
                    return 0;
            }
        }

        #region 按钮事件
        private void btnCurBillwayClick(object sender, RoutedEventArgs e)
        {
        }

        private void btnPrintBillway_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnTransmitBillway_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGeneralBillway_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int num = Convert.ToInt32(txtRoundNum.Text);
                LocalSessions.Clear();
                if (10000 >= num && 0 < num)
                {
                    for (int i = 0; i < num; i++)
                    {
                        LocalSessions.Add(new Session(i));
                    }
                    SetWaybillFromSession(LocalSessions[1]);
                    LocalSessionIndex = 1;
                    SetWaybillFromSession(LocalSessions[0]);
                    LocalSessionIndex = 0;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("请输入1~10000数字");
            }
        }

        private void btnImportBillway_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnInsertBillway_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReplcBillway_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveBillway_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRegenerateBillway_Click(object sender, RoutedEventArgs e)
        {
            LocalSessions[LocalSessionIndex] = new Session(LocalSessionIndex);
            SetWaybillFromSession(LocalSessions[LocalSessionIndex]);
        }

        private void Homepage_Click(object sender, RoutedEventArgs e)
        {
            SetWaybillFromSession(LocalSessions[0]);
            LocalSessionIndex = 0;
        }

        private void Prepage_Click(object sender, RoutedEventArgs e)
        {
            SetWaybillFromSession(LocalSessions[--LocalSessionIndex]);
        }

        private void Jumpto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var idx = Convert.ToInt32(txtPageIndex.Text) - 1;
                if (0 > idx || idx > LocalSessions.Count - 1)
                {
                    MessageBox.Show("跳不到那里 ^_^");
                }
                SetWaybillFromSession(LocalSessions[idx]);
                LocalSessionIndex = idx;
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确数字 ^_^");
            }
        }

        private void btnNextpage_Click(object sender, RoutedEventArgs e)
        {
            SetWaybillFromSession(LocalSessions[++LocalSessionIndex]);
        }

        private void btnLastpage_Click(object sender, RoutedEventArgs e)
        {
            LocalSessionIndex = LocalSessions.Count - 1;
            SetWaybillFromSession(LocalSessions[LocalSessionIndex]);
        }

        private void btnImportFrontWaybill_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ImportFront, "");
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            SSClient.Close();
            btnConnect.Visibility = Visibility.Visible;
            btnDisconnect.Visibility = Visibility.Collapsed;
            txtServerIP.IsEnabled = true;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                host_ip = IPAddress.Parse(txtServerIP.Text);
                SSClient.Connect(host_ip, port);
                btnDisconnect.Visibility = Visibility.Visible;
                btnConnect.Visibility = Visibility.Collapsed;
                txtServerIP.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败，检查网络或IP");
            }
        }
        #endregion

        #region SuperSocket监听
        void Client_DataReceived(object sender, DataEventArgs e)
        {
            string msg_type = Encoding.Default.GetString(e.Data, 0, 2);
            int length = BitConverter.ToInt32(e.Data, 2);

            byte[] data = new byte[e.Length - 6];
            Buffer.BlockCopy(e.Data, 6, data, 0, e.Length - 6);

            if (!Enum.TryParse(msg_type, out RemoteCommand rest))
            {
                return;
            }
            switch (rest)
            {
                case RemoteCommand.Image:
                    SetImage(length, data);
                    break;
                case RemoteCommand.ImportFront:
                    ImportFront(data);
                    break;
                default:
                    //MessageBox.Show(msg_type);
                    break;
            }
        }
        private int temp = 0;
        void SetImage(int len, byte[] buffer)
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
                //game_bitmap = bitmap;
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    imgGame.Source = bitmap;
                }), DispatcherPriority.Render);
            }
            catch (Exception ex)
            {
            }
        }
        void ImportFront(byte[] data)
        {
            try
            {
                string data_str = Encoding.Default.GetString(data, 0, data.Length);
                var sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(data_str);
                if (sessions.Count == 0)
                {
                    MessageBox.Show("前台路单尚未生成，请先开始游戏");
                    return;
                }
                LocalSessions = sessions;
                var idx = sessions.Count - 1;
                SetWaybillFromSession(LocalSessions[idx]);
                LocalSessionIndex = idx;
                MessageBox.Show("导入成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        public byte[] Decompress(Byte[] bytes, int len)//因为本例需求，我加了一个参数Len表示实际长度
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

        #endregion
        #region 视频监控
        //public void StartClient()
        //{
        //    setImageCallBack = new SetImageCallBack(SetImage);
        //    //thread = new Thread(new ThreadStart(ReceiveImage));
        //    remoteIPA = IPAddress.Parse(txtServerIP.Text);
        //    remoteIEP = new IPEndPoint(remoteIPA, port);
        //    byteBuffer = new Byte[1024 * 128];
        //    imgBuffer = new BitmapImage();
        //    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //    try
        //    {
        //        server.Connect(remoteIEP);
        //        if (server.Connected)
        //        {
        //            Thread threadReceive = new Thread(new ThreadStart(Receive));
        //            threadReceive.IsBackground = true;
        //            threadReceive.Start();
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //}

        //private void Receive()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            server.Receive(byteBuffer);
        //            try
        //            {
        //                imgBuffer = new BitmapImage();
        //                imgBuffer.BeginInit();
        //                imgBuffer.StreamSource = new MemoryStream(byteBuffer);
        //                imgBuffer.EndInit();
        //                imgBuffer.Freeze();
        //                imgGame.Dispatcher.BeginInvoke(setImageCallBack, imgBuffer);
        //            }
        //            catch
        //            {
        //                imgBuffer = null;
        //            }
        //        }
        //        catch
        //        {
        //            System.Windows.MessageBox.Show("与服务器断开连接！");
        //            this.Close();
        //        }

        //    }

        //}

        //private void SetImage(BitmapImage bitmapImage)
        //{
        //    imgGame.Source = bitmapImage;
        //}
        #endregion

    }
}
