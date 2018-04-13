using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Data;
using SuperSocket.ClientEngine;
using System.Text;
using Newtonsoft.Json;
using Bacc_front;
using Microsoft.Win32;
using WsUtils;
using System.Linq;

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
        public const int port = 54322;
        public IPAddress host_ip;
        public SuperClient SSClient;
        #endregion
        #region 游戏变量
        public BackLiveData FrontRecord { get; set; }

        /// <summary>
        /// 倒计时器
        /// </summary>
        public int CountDown { get; set; }
        /// <summary>
        /// 状态显示器
        /// </summary>
        public string StateText { get; set; }
        public string BankerStateText { get; set; }
        public string PlayerStateText { get; set; }
        /// <summary>
        /// 轮次计数器
        /// </summary>
        public int RoundIndex { get; set; }
        public string _roundStrIndex { get { return (RoundIndex + 1).ToString(); } }
        /// <summary>
        /// 局数计数器
        /// </summary>
        public int SessionIndex { get; set; }
        public string _sessionStrIndex { get { return (SessionIndex + 1).ToString(); } }

        public int DeskBanker { get; set; }
        public int DeskTie { get; set; }
        public int Differ { get; set; }
        public int Profit { get; set; }
        public int MostPlayer { get; set; }
        public int DeskScore { get; set; }
        public int DeskPlayer { get; set; }

        public ObservableCollection<WhoWin> FrontWaybill { get; set; }
        public Session FrontCurrentSession { get; set; }

        public ObservableCollection<WhoWin> Waybill { get; set; }
        public ObservableCollection<Session> LocalSessions { get; set; }
        //public Session CurrentDisplaySession { get; set; }
        public Round CurrentLocalRound { get { return LocalSessions[SessionIndex].RoundsOfSession[RoundIndex]; } }
        public Session CurrentLocalSession
        {
            get { return LocalSessions[LocalSessionIndex]; }
        }
        public int SessionPageIndex { get; set; }

        [PropertyChanged.ImplementPropertyChanged]
        public int LocalSessionIndex { get; set; }
        public int localSessionStrIndex { get { return (LocalSessionIndex + 1); } }

        public int AllMostLimit { get; set; }
        public int LeastBet { get; set; }
        public int TieMostBet { get; set; }

        private List<Button> sm_waybill_btns = new List<Button>();
        private List<Button> front_sm_waybill_btns = new List<Button>();
        private int LocalRoundNum;

        public Dictionary<string, SettingItem> app_setting = new Dictionary<string, SettingItem>();
        public Dictionary<string, SettingItem> game_setting = new Dictionary<string, SettingItem>();
        public Dictionary<string, string> PasswordMap = new Dictionary<string, string>();
        public BackLiveData FrontLiveData = new BackLiveData();
        public List<int> FrontBetRecordIdList = new List<int>();
        public BetRecord BetRecordWindow = new BetRecord();

        public ObservableCollection<Player> Players { get; set; }

        #endregion

        public static MainWindow Instance { get; set; }
        public ObservableCollection<BackBetRecord> BetRecordDataFromFront { get; internal set; }
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Closed += MainWindow_Closed;

            Players = Desk.Instance.Players;
            LocalSessions = new ObservableCollection<Bacc_front.Session>();
            Waybill = new ObservableCollection<WhoWin>();
            DataHandler.InitBetRecordDataToBack();
            txtRoundNum.Text = 1000.ToString();
            Casino.DataContext = this;
            dgProfit.DataContext = BetRecordDataFromFront;
            dgProfit.ItemsSource = BetRecordDataFromFront;

            spPageButtons.DataContext = this;
            grdFrontBet.DataContext = this;
            lstButton.DataContext = Setting.Instance;
            gdPwd.DataContext = PasswordMap;

            LocalRoundNum = Setting.Instance.GetIntSetting("round_num_per_session");
            InitWaybillBindingForLocal();

            SSClient = new SuperClient();
            SSClient.DataReceived += Client_DataReceived;

            Login login = new Login();
            login.ShowDialog();
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Login.Instance.Visibility = Visibility.Visible;
        }

        #region 路单绑定等
        /// <summary>
        /// 前台当前局的绑定
        /// </summary>
        private void BindWaybills()
        {
            var round_num = FrontCurrentSession.RoundNumber;
            front_sm_waybill_btns = new List<Button>();
            try
            {
                for (int i = 0; i < round_num; i++)
                {
                    //小路单按钮绑定
                    var block = new Button();
                    block.Style = (Style)Resources["SmallWaybillButton"];
                    block.Tag = i;
                    grdSmallWaybill.Children.Add(block);
                    front_sm_waybill_btns.Add(block);

                    Binding myBinding = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("FrontWaybill[" + i + "].Winner"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding(block, DataContextProperty, myBinding);

                    //大路单按钮绑定
                    var blockBig = new Button();
                    blockBig.Style = (Style)Resources["WaybillBlock"];
                    blockBig.Tag = i;
                    grdBigWaybill.Children.Add(blockBig);

                    var myBindingBig = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("FrontWaybill[" + i + "].Winner"),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding(blockBig, DataContextProperty, myBindingBig);

                    var col = i / 6;
                    var row = i % 6;
                    Grid.SetRow(blockBig, row);
                    Grid.SetColumn(blockBig, col);
                }

                grdBigWaybill.DataContext = this;
                grdSmallWaybill.DataContext = this;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void ResetSmWaybill()
        {
            var Waybill = FrontWaybill;
            int pre_row = 0, pre_col = 0, pre = 0;
            int cur_side = Waybill[0].Winner;

            front_sm_waybill_btns[0].SetValue(Grid.ColumnProperty, pre_row);
            front_sm_waybill_btns[0].SetValue(Grid.RowProperty, pre_col);

            for (int i = 1; i < Waybill.Count; i++)
            {
                var w_i = Waybill[i].Winner;
                var w_pre = Waybill[pre].Winner;

                if (w_pre == (int)WinnerEnum.tie)
                {
                    if (cur_side == (int)WinnerEnum.tie || w_i == (int)WinnerEnum.tie)
                    {
                        if (++pre_row >= 9)
                        {
                            pre_row = 0;
                            pre_col++;
                        }
                        if (w_i != (int)WinnerEnum.tie)
                        {

                            cur_side = w_i;
                        }
                    }
                    else
                    {
                        if (w_i == cur_side)
                        {
                            if (++pre_row >= 9)
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
                    if (w_i == (int)WinnerEnum.tie || w_i == w_pre)
                    {
                        if (++pre_row >= 9)
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

                front_sm_waybill_btns[i].SetValue(Grid.ColumnProperty, pre_col);
                front_sm_waybill_btns[i].SetValue(Grid.RowProperty, pre_row);
                pre++;
            }
        }
        private void SetFrontWaybillFromFrontSession()
        {
            var session = FrontCurrentSession;
            var Waybill = FrontWaybill;
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

        /// <summary>
        /// ###################后台本地路单的绑定########################
        /// </summary>
        private void InitWaybillBindingForLocal()
        {
            LocalSessions.Clear();
            var newsession = new Session(0);
            LocalSessions.Add(newsession);

            InitWaybillBinding(LocalRoundNum);

            SetWaybillFromSession(newsession);
            ResetSmWaybillForLocal();

            LocalSessionIndex = 0;
        }
        private void InitWaybillBinding(int round_num)
        {
            gdBigWaybill.Children.Clear();
            Waybill.Clear();
            sm_waybill_btns.Clear();
            gdSmallWaybill.Children.Clear();
            for (int i = 0; i < round_num; i++)
            {
                Waybill.Add(new WhoWin() { Winner = (int)WinnerEnum.none });

                //小路单按钮绑定
                var block = new Button();
                block.Style = (Style)Resources["SmallWaybillButton"];
                block.Tag = i;
                block.Click += BigWaybillBlockClick;
                gdSmallWaybill.Children.Add(block);
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
                gdBigWaybill.Children.Add(blockBig);

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
            Instance.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetSmWaybillForLocal));
        }
        private void ResetSmWaybillForLocal()
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
                        if (++pre_row >= 9)
                        {
                            pre_row = 0;
                            pre_col++;
                        }
                        if (w_i != (int)Winner.tie)
                        {

                            cur_side = w_i;
                        }
                    }
                    else
                    {
                        if (w_i == cur_side)
                        {
                            if (++pre_row >= 9)
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
                        if (++pre_row >= 9)
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

        private void BigWaybillBlockClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var idx = Convert.ToInt32(btn.Tag);
            UpdateWaybill(idx);
            ResetSmWaybillForLocal();
        }
        private void UpdateWaybill(int idx)
        {
            var curWinner = Waybill[idx].Winner;

            var who = new WhoWin
            {
                Winner = SwitchWinner(curWinner),
            };
            Waybill[idx] = who;

            CurrentLocalSession.RoundsOfSession[idx] = Session.CreateRoundByWinner((BetSide)who.Winner);
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
        #endregion
        #region 按钮事件
        private void btnFrontCurSessionClick(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.SendFrontCurSession, "");
        }
        private void btnPrintBillway_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 传送全部路单到前台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTransmitLocalSessions_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ImportBack, LocalSessions);
        }
        /// <summary>
        /// 传送当前显示的路单，没开局改变当前局，开局改变下局
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTransmitNextSession_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ImportBackNextSession, CurrentLocalSession);
        }
        private void btnGeneralLocalSessions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int num = Convert.ToInt32(txtRoundNum.Text);
                LocalSessions.Clear();
                if (1000 >= num && 0 < num)
                {
                    for (int i = 0; i < num; i++)
                    {
                        LocalSessions.Add(new Session(i));
                    }
                    LocalSessionIndex = LocalSessionIndex < num - 1 ? 0 : LocalSessionIndex;
                    SetWaybillFromSession(CurrentLocalSession);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("请输入1~1000数字");
            }
        }
        private void btnImportLocalSessionFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            if (op.ShowDialog() == true)
            {
                try
                {
                    var file = op.FileName;
                    var content = new FileUtils().ReadFileFromAbsolute(file);
                    var sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(content);
                    LocalSessions = sessions;
                    SetWaybillFromSession(CurrentLocalSession);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void btnSaveLocalSessionsToFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            if (op.ShowDialog() == true)
            {
                try
                {
                    var file = op.FileName;
                    var sessions = JsonConvert.SerializeObject(LocalSessions);
                    new FileUtils().WriteFileFromAbsolute(file, sessions, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void btnRegenerateCurrentSession_Click(object sender, RoutedEventArgs e)
        {
            LocalSessions[LocalSessionIndex] = new Session(LocalSessionIndex);
            SetWaybillFromSession(LocalSessions[LocalSessionIndex]);
        }
        private void btnImportFrontLocalSessions_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ImportFrontLocalSessions, "");
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
                Login login = new Login();
                login.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败，检查网络或IP");
            }
        }
        private void btnConfirmPwd(object sender, RoutedEventArgs e)
        {
            var pwd = txtPwd.Password;
            //var key = Setting.Instance.PasswordMap.FirstOrDefault(p => p.Value == pwd).Key;
            var key = pwd;
            if (string.IsNullOrEmpty(key))
            {
                txtPwd.Password = "";
                return;
            }
            switch (key)
            {
                case Setting.waybill_pwd:
                    gdBigWaybill.Visibility = Visibility.Visible;
                    gdSmallWaybill.Visibility = Visibility.Visible;
                    break;
                case Setting.front_setting_pwd:
                    gdPwdAndSetting.Visibility = Visibility.Visible;
                    //dgProfit.Visibility = Visibility.Hidden;
                    //gpAccount.Visibility = Visibility.Hidden;
                    lstButton.Visibility = Visibility.Visible;
                    lstButton.ItemsSource = Setting.Instance.game_setting.Where(kv => Setting.Instance.manager_menu_items.Contains(kv.Key));
                    break;
                case Setting.bet_record_pwd:
                    SSClient.SendCommand(RemoteCommand.SendFrontBetRecordIdList, "");
                    break;
                case Setting.robot_pwd:
                    gdRobot.Visibility = Visibility.Visible;
                    break;
                case Setting.kill_big_pwd:
                    gdKillBig.Visibility = Visibility.Visible;
                    break;
                case Setting.hide_pwd:
                    gdBigWaybill.Visibility = Visibility.Hidden;
                    gdSmallWaybill.Visibility = Visibility.Hidden;
                    gdRobot.Visibility = Visibility.Hidden;
                    gdKillBig.Visibility = Visibility.Hidden;
                    break;
            }
            txtPwd.Password = "";
        }
        #region 跳页
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
        #endregion

        #endregion
        #region SuperSocket监听
        void Client_DataReceived(object sender, DataEventArgs e)
        {
            try
            {
                string msg_type = Encoding.UTF8.GetString(e.Data, 0, 2);
                if (!Enum.TryParse(msg_type, out RemoteCommand rest))
                {
                    return;
                }
                int length = BitConverter.ToInt32(e.Data, 2);
                byte[] data = new byte[e.Length - 6];
                Buffer.BlockCopy(e.Data, 6, data, 0, e.Length - 6);
                switch (rest)
                {
                    case RemoteCommand.Login:
                        CheckLogin(data, length);
                        break;
                    case RemoteCommand.SendFrontCurSession:
                        SetFrontCurSession(data, length);
                        break;
                    case RemoteCommand.ImportFrontLocalSessions:
                        OnImportFrontLocalSessions(data, length);
                        break;
                    case RemoteCommand.SendFrontPassword:
                        SetFrontPassword(data, length);
                        break;
                    case RemoteCommand.SendFrontSetting:
                        SetFrontSetting(length, data);
                        break;
                    case RemoteCommand.SendFrontSummationBetRecord:
                        SetFrontBetRecord(data, length);
                        break;
                    case RemoteCommand.SendFrontLiveData:
                        SetFrontLiveData(length, data);
                        break;
                    case RemoteCommand.ImportBackFail:
                        MessageBox.Show("传送路单失败");
                        break;
                    case RemoteCommand.ImportBackOK:
                        MessageBox.Show("传送路单成功");
                        break;
                    case RemoteCommand.SendFrontBetRecordIdList:
                        GetFrontBetRecordIds(length, data);
                        break;
                    case RemoteCommand.SendFrontBetRecord:
                        ShowFrontBetRecord(length, data);
                        break;
                    default:
                        //MessageBox.Show(msg_type);
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void CheckLogin(byte[] data, int length)
        {
            try
            {
                var data_str = Encoding.UTF8.GetString(data, 0, length);
                //var str = data_str;
                var str = JsonConvert.DeserializeObject<string>(data_str);
                if (str == "OK")
                {
                    Login.Instance.Dispatcher.Invoke(new Action(() =>
                    {
                        btnDisconnect.Visibility = Visibility.Visible;
                        btnConnect.Visibility = Visibility.Collapsed;
                        txtServerIP.IsEnabled = false;
                        Login.Instance.Hide();
                    }));
                }
                else
                {
                    if (SSClient.IsConnected)
                    {
                        SSClient.Close();
                        MessageBox.Show("连接失败,检查网络或密码");
                    }
                }
            }
            catch (Exception ex)
            {
                if (SSClient.IsConnected)
                {
                    SSClient.Close();
                    MessageBox.Show("连接失败,检查网络或密码");
                }
            }
            finally
            {
                Login.Instance.Dispatcher.Invoke(new Action(() => { Login.Instance.btnConnect.IsEnabled = true; }));
            }
        }
        public void SetFrontPassword(byte[] data, int len)
        {
            var data_str = Encoding.UTF8.GetString(data, 0, len);
            PasswordMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(data_str);
            //MainWindow.Instance.Dispatcher.Invoke(new Action(() => { lstButton.ItemsSource = setting; }));
        }
        public void SetFrontSetting(int len, byte[] data)
        {
            try
            {
                var data_str = Encoding.UTF8.GetString(data, 0, len);
                Setting.Instance.game_setting = JsonConvert.DeserializeObject<Dictionary<string, SettingItem>>(data_str);

                Instance.Dispatcher.Invoke(new Action(() => { lstButton.ItemsSource = Setting.Instance.game_setting; }));
            }
            catch (Exception ex)
            {
#if DEBUG
#endif
            }
        }
        private void SetFrontCurSession(byte[] data, int len)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    var data_str = Encoding.UTF8.GetString(data, 0, len);
                    var session = JsonConvert.DeserializeObject<Session>(data_str);

                    FeedLocalSessions(session);
                    SetWaybillFromSession(session);
                    LocalSessionIndex = session.SessionId;
                }));
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("setfrontcursession出错");
#endif
            }
        }
        private void FeedLocalSessions(Session lastone)
        {
            for (int i = LocalSessions.Count; i < lastone.SessionId; i++)
            {
                LocalSessions.Add(new Session(i));
            }
            if (LocalSessions.Count - 1 < lastone.SessionId)
            {
                LocalSessions.Add(lastone);
            }
            else
            {
                LocalSessions[lastone.SessionId] = lastone;
            }
        }
        public void SetFrontBetRecord(byte[] data, int len)
        {
            var data_str = Encoding.UTF8.GetString(data, 0, len);
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                BetRecordDataFromFront = JsonConvert.DeserializeObject<ObservableCollection<BackBetRecord>>(data_str);
                dgProfit.ItemsSource = BetRecordDataFromFront;
            }));
        }
        public void SetFrontLiveData(int len, byte[] data)
        {
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    string data_str = Encoding.UTF8.GetString(data, 0, data.Length);
                    FrontRecord = JsonConvert.DeserializeObject<BackLiveData>(data_str);
                    Players = JsonConvert.DeserializeObject<ObservableCollection<Player>>(FrontRecord.JsonPlayerScores);

                    RoundIndex = FrontRecord.RoundIndex;

                    SessionIndex = FrontRecord.SessionIndex;
                    txtSessionIndex.Text = SessionIndex.ToString();

                    CountDown = FrontRecord.Countdown;
                    StateText = FrontRecord.State == (int)GameState.Shuffling ? "洗牌中" : (FrontRecord.State == (int)GameState.Betting ? "押注中" : "开牌中");
                    if ((GameState)FrontRecord.State == GameState.Preparing)
                    {
                        bdPrepare.Visibility = Visibility.Visible;
                        txtFrontStateTitle.Text = "前台尚未开始";
                        txtCountdownDescription.Text = "此时可以传送路单";
                    }
                    else if ((GameState)FrontRecord.State == GameState.Shuffling)
                    {
                        bdPrepare.Visibility = Visibility.Visible;
                        txtCountdownDescription.Text = CountDown + "秒开始下一局";
                        txtFrontStateTitle.Text = "正在洗牌";
                    }
                    else
                    {
                        bdPrepare.Visibility = Visibility.Hidden;
                    }

                    DeskBanker = FrontRecord.DeskBanker;
                    DeskPlayer = FrontRecord.DeskPlayer;
                    DeskTie = FrontRecord.DeskTie;
                    DeskScore = Players.Sum(p => p.Balance + p.BetScore.Sum(d => d.Value));

                    Differ = Math.Abs(DeskBanker - DeskPlayer);
                    MostPlayer = Players.FirstOrDefault(player => player.BetScore.Values.Sum() == Players.Max(p => p.BetScore.Values.Sum())).Id;
                    Profit = Desk.GetProfit(FrontRecord.Winner, DeskBanker, DeskPlayer, DeskTie);
                }
                catch (Exception ex)
                {
#if DEBUG
#endif
                }
            }));
        }
        public void OnImportFrontLocalSessions(byte[] data, int len)
        {
            try
            {
                string data_str = Encoding.UTF8.GetString(data, 0, len);
                var sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(data_str);
                if (sessions.Count == 0)
                {
                    MessageBox.Show("前台没有后台传送的路单!");
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
        private void GetFrontBetRecordIds(int len, byte[] data)
        {
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                var data_str = Encoding.UTF8.GetString(data, 0, len);
                if (string.IsNullOrEmpty(data_str))
                {
                    MessageBox.Show("前台押分记录为空");
                    return;
                }
                var list = JsonConvert.DeserializeObject<List<int>>(data_str);
                if (list == null || list.Count == 0)
                {
                    MessageBox.Show("前台押分记录为空");
                    return;
                }
                FrontBetRecordIdList = list;
                var lastRecord = list.Max();
                SSClient.SendCommand(RemoteCommand.SendFrontBetRecord, lastRecord);
            }));
        }
        private void ShowFrontBetRecord(int length, byte[] data)
        {
            var data_str = Encoding.UTF8.GetString(data, 0, length);
            if (string.IsNullOrEmpty(data_str))
            {
                return;
            }
            var list = JsonConvert.DeserializeObject<List<BetScoreRecord>>(data_str);
            if (list == null || list.Count == 0)
            {
                return;
            }
            if (BetRecordWindow.IsInited)
            {
                BetRecordWindow.ResetSession(list);
            }
            else
            {
                BetRecordWindow.Init(FrontBetRecordIdList, list);
            }
        }
        #endregion

        private void btnBetRecord_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnSaveSettingToFront_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnClearFrontAccount_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnFrontShutdown_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnQuitSetting_Click(object sender, RoutedEventArgs e)
        {
            gdPwdAndSetting.Visibility = Visibility.Hidden;
        }
        private void btnPwdNum_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;
            var num = Convert.ToInt32(tag);
            txtPwd.Password += num;
        }
        private void btnDeletePwd(object sender, RoutedEventArgs e)
        {
            var pwd = txtPwd.Password;
            if (pwd.Length > 0)
            {
                txtPwd.Password = pwd.Substring(0, pwd.Length - 1);
            }
        }
    }
}