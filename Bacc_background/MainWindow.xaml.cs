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
using System.Data;
using System.Windows.Media;
using System.Threading;

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
        public BackLiveData FrontLiveRecord { get; set; }

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

        public int PreRoundIndex { get; set; }
        public string _roundStrIndex { get { return (RoundIndex + 1).ToString(); } }
        public Button btnGrdBigWaybillMask = new Button();
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
        private List<Button> big_waybill_btns = new List<Button>();
        private List<Button> front_sm_waybill_btns = new List<Button>();
        private List<Button> front_big_waybill_btns = new List<Button>();
        private int LocalRoundNum;

        public Dictionary<string, SettingItem> app_setting = new Dictionary<string, SettingItem>();
        public Dictionary<string, SettingItem> game_setting = new Dictionary<string, SettingItem>();
        public Dictionary<string, string> PasswordMap { get; set; }
        public List<int> FrontBetRecordIdList = new List<int>();
        public BetRecord BetRecordWindow = new BetRecord();

        public ObservableCollection<Player> Players { get; set; }

        #endregion

        public static MainWindow Instance { get; set; }
        public ObservableCollection<BackBetRecord> BetRecordDataFromFront { get; internal set; }
        public bool IsFrontCurSessionShowwing = false;

        public int AccountPageIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Closed += MainWindow_Closed;
            InitPassward();

            Players = Desk.Instance.Players;
            LocalSessions = new ObservableCollection<Bacc_front.Session>();
            Waybill = new ObservableCollection<WhoWin>();
            FrontWaybill = new ObservableCollection<WhoWin>();

            var items = Enumerable.Range(1, 12).Concat(Enumerable.Range(14, 2).ToList());
            cmbGuibinji.ItemsSource = items;

            DataHandler.InitBetRecordDataToBack();
            txtRoundNum.Text = 1000.ToString();
            Casino.DataContext = this;
            dgProfit.DataContext = BetRecordDataFromFront;
            dgProfit.ItemsSource = BetRecordDataFromFront;

            spPageButtons.DataContext = this;
            grdFrontBet.DataContext = this;
            spFrontPwds.DataContext = this;
            lstButton.DataContext = Setting.Instance;

            LocalRoundNum = Setting.Instance.GetIntSetting("round_num_per_session");
            InitWaybillBindingForLocal();

            BindWaybills();

            SSClient = new SuperClient();
            SSClient.DataReceived += Client_DataReceived;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //SSClient.Close();
            //Login.Instance.Visibility = Visibility.Visible;
            App.Current.Shutdown();
        }

        #region 路单绑定等
        /// ####################前台当前局的绑定#########################
        private void BindWaybills()
        {
            front_sm_waybill_btns.Clear();
            front_big_waybill_btns.Clear();

            FrontWaybill.Clear();

            grdBigWaybill.Children.Clear();
            grdSmallWaybill.Children.Clear();
            var round_num = 66;
            try
            {
                for (int i = 0; i < round_num; i++)
                {
                    FrontWaybill.Add(new WhoWin() { Winner = (int)WinnerEnum.none });
                    //小路单按钮绑定
                    var block = new Button();
                    block.Style = (Style)Resources["SmallWaybillButton"];
                    block.Tag = i;
                    block.Width = 10;
                    block.Height = 10;
                    block.Opacity = 0.2;
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
                    blockBig.Opacity = 0.2;
                    grdBigWaybill.Children.Add(blockBig);
                    front_big_waybill_btns.Add(blockBig);

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
            var roundindex = RoundIndex == -1 ? 0 : RoundIndex;
            if (Waybill.Count == session.RoundNumber)
            {
                for (int i = 0; i < session.RoundNumber; i++)
                {
                    Waybill[i].Winner = (int)session.RoundsOfSession[i].Winner.Item1;
                    if (big_waybill_btns != null && sm_waybill_btns != null)
                    {
                        if (i <= roundindex)
                        {
                            front_sm_waybill_btns[i].Opacity = 1;
                            front_big_waybill_btns[i].Opacity = 1;
                        }
                        else
                        {
                            front_sm_waybill_btns[i].Opacity = 0.2;
                            front_big_waybill_btns[i].Opacity = 0.2;
                        }
                    }
                }
            }
            else
            {
                Waybill.Clear();
                for (int i = 0; i < session.RoundNumber; i++)
                {
                    Waybill.Add(new WhoWin() { Winner = (int)session.RoundsOfSession[i].Winner.Item1 });
                    if (big_waybill_btns != null && sm_waybill_btns != null)
                    {
                        if (i <= roundindex)
                        {
                            front_sm_waybill_btns[i].Opacity = 1;
                            front_big_waybill_btns[i].Opacity = 1;
                        }
                        else
                        {
                            front_sm_waybill_btns[i].Opacity = 0.2;
                            front_big_waybill_btns[i].Opacity = 0.2;
                        }
                    }
                }
            }
            ResetSmWaybill();
        }
        /// ##########################################################

        /// ###################后台本地路单的绑定########################
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
            big_waybill_btns.Clear();
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
                big_waybill_btns.Add(blockBig);

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
            //Instance.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetSmWaybillForLocal));
            ResetSmWaybillForLocal();
            IsFrontCurSessionShowwing = false;
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
        /// ##########################################################
        #endregion
        #region 按钮事件
        private void btnFrontCurSessionClick(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.SendFrontCurSession, "");
        }
        private void btnPrintBillway_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnTransmitLocalSessions_Click(object sender, RoutedEventArgs e)
        {
            btnTransmitBillway.IsEnabled = false;
            var local_sessions = LocalSessions.Select(s => new LocalSession()
            {
                SessionIndex = s.SessionId,
                JsonSession = JsonConvert.SerializeObject(s)
            }).ToList();
            SSClient.SendCompressedCommand(RemoteCommand.ImportBack, local_sessions);
        }
        private void btnTransmitNextSession_Click(object sender, RoutedEventArgs e)
        {
            if((FrontLiveRecord.State != (int)GameState.Preparing 
                && FrontLiveRecord.State != (int)GameState.Shuffling)
                && FrontLiveRecord.SessionIndex == LocalSessionIndex)
            {
                MessageBox.Show("前台本局已经开始，不能修改路单");
            }
            else
            {
                SSClient.SendCommand(RemoteCommand.ImportBackNextSession, CurrentLocalSession);
            }
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
                    int temp = LocalSessionIndex;
                    LocalSessionIndex = LocalSessionIndex == 0 ? 1 : 0;
                    LocalSessionIndex = temp > num - 1 ? 0 : temp;
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
                    var decompress = CompressHelper.Decompress(content);
                    var sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(decompress);
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
            var op = new SaveFileDialog();
            if (op.ShowDialog() == true)
            {
                try
                {
                    var file = op.FileName;
                    var sessions = JsonConvert.SerializeObject(LocalSessions);
                    var compress = CompressHelper.Compress(sessions);
                    new FileUtils().WriteFileFromAbsolute(file, compress, true);
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
                    gbBigWaybill.Visibility = Visibility.Visible;
                    gbSmWaybill.Visibility = Visibility.Visible;

                    gdBigWaybill.Visibility = Visibility.Visible;
                    gdSmallWaybill.Visibility = Visibility.Visible;
                    break;
                case Setting.front_setting_pwd:
                    gdPwdAndSetting.Visibility = Visibility.Visible;
                    //dgProfit.Visibility = Visibility.Hidden;
                    //gpAccount.Visibility = Visibility.Hidden;
                    lstButton.Visibility = Visibility.Visible;
                    //lstButton.ItemsSource = Setting.Instance.game_setting.Where(kv => Setting.Instance.manager_menu_items.Contains(kv.Key));
                    break;
                case Setting.bet_record_pwd:
                    SSClient.SendCommand(RemoteCommand.SendFrontBetRecordIdList, "");
                    break;
                case Setting.front_account_pwd:
                    SSClient.SendCommand(RemoteCommand.SendFrontAccount, AccountPageIndex);
                    spAccount.Visibility = Visibility.Visible;
                    dgProfit.Visibility = Visibility.Hidden;
                    break;
                case Setting.robot_pwd:
                    gbSmWaybill.Visibility = Visibility.Visible;
                    gdRobot.Visibility = Visibility.Visible;
                    break;
                case Setting.kill_big_pwd:
                    gbSmWaybill.Visibility = Visibility.Visible;
                    gdKillBig.Visibility = Visibility.Visible;
                    break;
                case Setting.hide_pwd:
                    gbSmWaybill.Visibility = Visibility.Hidden;
                    gbBigWaybill.Visibility = Visibility.Hidden;
                    gbSmWaybill.Visibility = Visibility.Hidden;
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
        #region 设置界面按钮
        private void btnBetRecord_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnSaveSettingToFront_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ModifyFrontSetting, Setting.Instance.game_setting);
        }
        private void btnPreAccount_Click(object sender, RoutedEventArgs e)
        {
            AccountPageIndex++;
            SSClient.SendCommand(RemoteCommand.SendFrontAccount, AccountPageIndex);
        }
        private void btnNextAccount_Click(object sender, RoutedEventArgs e)
        {
            if (--AccountPageIndex < 0)
            {
                AccountPageIndex = 0;
            }
            SSClient.SendCommand(RemoteCommand.SendFrontAccount, AccountPageIndex);
        }
        private void btnClearFrontAccount_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ClearFrontAccount, "");
            Thread.Sleep(200);
            SSClient.SendCommand(RemoteCommand.SendFrontAccount, 0);
            MessageBox.Show("清除成功");
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
        #endregion
        #endregion
        #region SuperSocket监听
        void Client_DataReceived(object sender, DataEventArgs e)
        {
            try
            {
                var str = Encoding.UTF8.GetString(e.Data, 0, 10024);
                string msg_type = Encoding.UTF8.GetString(e.Data, 0, 2);
                if (!Enum.TryParse(msg_type, out RemoteCommand rest))
                {
                    //var data_str = Encoding.UTF8.GetString(e.Data, 0, 10024);
                    //int lgth = BitConverter.ToInt32(e.Data, 2);
                    //MessageBox.Show(data_str);
                    return;
                }
                int length = BitConverter.ToInt32(e.Data, 2);
                byte[] data = new byte[length];

                Buffer.BlockCopy(e.Data, 6, data, 0, length);
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
                    case RemoteCommand.SendFrontAccount:
                        SetFrontAccount(data, length);
                        break;
                    case RemoteCommand.SendFrontLiveData:
                        SetFrontLiveData(length, data);
                        break;
                    case RemoteCommand.ImportBackFail:
                        Dispatcher.Invoke(new Action(() =>
                        {
                            btnTransmitBillway.IsEnabled = true;
                        }));
                        MessageBox.Show("传送路单失败");
                        break;
                    case RemoteCommand.ImportBackOK:
                        Dispatcher.Invoke(new Action(() =>
                        {
                            btnTransmitBillway.IsEnabled = true;
                        }));
                        MessageBox.Show("传送路单成功");
                        break;
                    case RemoteCommand.ImportBackNextSessionOnCurrentSession:
                        MessageBox.Show("传送路单成功,本局生效");
                        break;
                    case RemoteCommand.ImportBackNextSessionOnNextSession:
                        MessageBox.Show("传送路单成功,下一局生效");
                        break;
                    case RemoteCommand.SendFrontBetRecordIdList:
                        GetFrontBetRecordIds(length, data);
                        break;
                    case RemoteCommand.SendFrontBetRecord:
                        ShowFrontBetRecord(length, data);
                        break;
                    case RemoteCommand.LockFrontOK:
                        Dispatcher.Invoke(new Action(() => { btnLockFront.IsEnabled = false; btnUnlockFront.IsEnabled = true; }));
                        break;
                    case RemoteCommand.UnlockFrontOK:
                        Dispatcher.Invoke(new Action(() => { btnLockFront.IsEnabled = true; btnUnlockFront.IsEnabled = false; }));
                        break;
                    case RemoteCommand.ModifyFrontPasswordOK:
                        Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("修改成功");
                        }));
                        break;
                    case RemoteCommand.ModifyFrontSettingOK:
                        Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("保存成功");
                        }));
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
                        txtServerIP.Text = Login.Instance.Host;
                        txtServerIP.IsEnabled = false;
                        Login.Instance.Title = Login.Instance.Host;
                        Login.Instance.SaveCorrectPwd();
                        Login.Instance.Hide();
                    }));
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Title = Login.Instance.Host;
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
        private void InitPassward()
        {
            PasswordMap = new Dictionary<string, string>();
            PasswordMap.Add("waiter_pwd", "00000");
            PasswordMap.Add("manager_pwd", "00000");
            PasswordMap.Add("boss_pwd", "00000");
            PasswordMap.Add("account_pwd", "00000");
            PasswordMap.Add("quit_front_pwd", "00000");
            PasswordMap.Add("shutdown_pwd", "00000");
            PasswordMap.Add("clear_account_pwd", "00000");
            PasswordMap.Add("conn_account_pwd", "00000");
        }
        public void SetFrontPassword(byte[] data, int len)
        {
            var data_str = Encoding.UTF8.GetString(data, 0, len);
            PasswordMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(data_str);
        }
        public void SetFrontSetting(int len, byte[] data)
        {
            try
            {
                var data_str = Encoding.UTF8.GetString(data, 0, len);
                Setting.Instance.game_setting = JsonConvert.DeserializeObject<Dictionary<string, SettingItem>>(data_str);
                Dispatcher.Invoke(new Action(() =>
                {
                    lstButton.ItemsSource = null;
                    lstButton.ItemsSource = Setting.Instance.game_setting;
                    AllMostLimit = Setting.Instance.GetIntSetting("total_limit_red");
                    LeastBet = Setting.Instance.GetIntSetting("min_limit_bet");
                    TieMostBet = Setting.Instance.GetIntSetting("tie_limit_red");
                }));
            }
            catch (Exception ex)
            {
#if DEBUG
#endif
            }
        }
        private void SetFrontCurSession(byte[] data, int len)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    var data_str = Encoding.UTF8.GetString(data, 0, len);
                    var session = JsonConvert.DeserializeObject<Session>(data_str);

                    FeedLocalSessions(session);

                    SetWaybillFromSession(session);
                    LocalSessionIndex = session.SessionId;

                    FrontCurrentSession = session;
                    SetFrontWaybillFromFrontSession();
                    IsFrontCurSessionShowwing = true;
                }
                catch (Exception ex)
                {
#if DEBUG
                    //MessageBox.Show("setfrontcursession,datalength: "+data.Length +"len:"+ len  + ex.Message + ex.StackTrace +ex.InnerException);
#endif
                }
            }));
        }
        private void FeedLocalSessions(Session lastone)
        {
            try
            {

                for (int i = LocalSessions.Count; i <= lastone.SessionId; i++)
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
            catch (Exception ex)
            {
                throw;
            }
        }
        //private ObservableCollection<BackBetRecord> BetRecordSummationDataToBack;
        public void InitBetRecordDataToBack()
        {
            BetRecordDataFromFront = new ObservableCollection<BackBetRecord>();
            foreach (var p in Desk.Instance.Players)
            {
                BetRecordDataFromFront.Add(new BackBetRecord()
                {
                    PlayerId = p.Id.ToString(),
                    BetScore = 0,
                    DingFen = 0,
                    Profit = 0,
                    ZhongFen = 0
                });
            }
        }
        public void SetFrontBetRecord(byte[] data, int len)
        {
            InitBetRecordDataToBack();
            var data_str = Encoding.UTF8.GetString(data, 0, len);
            //var s0 = DateTime.Now;

            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                //var records = JsonConvert.DeserializeObject<ObservableCollection<BackBetRecord>>(data_str);
                var records = JsonConvert.DeserializeObject<List<BetScoreRecord>>(data_str);
 
                if (records != null)
                {
                    var players = records.Where(rc => !string.IsNullOrEmpty(rc.JsonPlayerScores)).Select(r => JsonConvert.DeserializeObject<ObservableCollection<Player>>(r.JsonPlayerScores));
                    foreach (var bet_data in BetRecordDataFromFront)
                    {
                        foreach (var record in records)
                        {
                            if (string.IsNullOrEmpty(record.JsonPlayerScores))
                            {
                                continue;
                            }
                            var ps = JsonConvert.DeserializeObject<ObservableCollection<Player>>(record.JsonPlayerScores);
                            var player = ps.FirstOrDefault(p => p.Id == Convert.ToInt32(bet_data.PlayerId));

                            var profit = Desk.GetProfit(record.Winner, player.BetScoreOnBank, player.BetScoreOnPlayer, player.BetScoreOnTie);
                            bet_data.Profit += profit;
                        }
                        bet_data.BetScore = players.Sum(lst => lst.Where(ps => ps.Id == Convert.ToInt32( bet_data.PlayerId)).Sum(p => p.BetScore.Sum(s => s.Value)));
                        //顶分中分 todo
                    }
                }
                //var span = (DateTime.Now - s0).TotalMilliseconds;
                //MessageBox.Show(span.ToString());

                //var s1 = DateTime.Now;
                DataTable table = WsUtils.DataTableExtensions.ToDataTable(BetRecordDataFromFront);
                var dr = table.NewRow();
                dr["PlayerId"] = "总 计";
                dr["BetScore"] = table.Compute("SUM(BetScore)", null);
                dr["Profit"] = table.Compute("SUM(Profit)", null);
                dr["DingFen"] = table.Compute("SUM(DingFen)", null);
                dr["ZhongFen"] = table.Compute("SUM(ZhongFen)", null);
                table.Rows.Add(dr);
                dgProfit.ItemsSource = table.DefaultView;
                //span = (DateTime.Now - s1).TotalMilliseconds;
                //MessageBox.Show(span.ToString());
            }));
        }
        private void SetFrontAccount(byte[] data, int length)
        {
            var data_str = Encoding.UTF8.GetString(data, 0, length);
            if (string.IsNullOrEmpty(data_str) || data_str == "null")
            {
                AccountPageIndex--;
            }
            var account = JsonConvert.DeserializeObject<FrontAccount>(data_str);
            if (account == null)
            {
                return;
            }
            var cur_acc = JsonConvert.DeserializeObject<ObservableCollection<AddSubScoreRecord>>(account.JsonScoreRecord);
            DataTable table = WsUtils.DataTableExtensions.ToDataTable(cur_acc);
            var dr = table.NewRow();
            dr["PlayerId"] = "总 计";
            dr["TotalAddScore"] = table.Compute("SUM(TotalAddScore)", null);
            dr["TotalSubScore"] = table.Compute("SUM(TotalSubScore)", null);
            dr["TotalAccount"] = table.Compute("SUM(TotalAccount)", null);
            table.Rows.Add(dr);
            Dispatcher.Invoke(new Action(() =>
            {
                if (account.IsClear)
                {
                    txtAccountState.Text = "清零时间:" + account.ClearTime.ToString("yyyy年MM月dd日 hh:mm:ss");
                }
                else
                {
                    txtAccountState.Text = "账目未清零";
                }
                dgAccount.ItemsSource = table.DefaultView;
            }));
        }
        public void SetFrontLiveData(int len, byte[] data)
        {
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    string data_str = Encoding.UTF8.GetString(data, 0, data.Length);
                    FrontLiveRecord = JsonConvert.DeserializeObject<BackLiveData>(data_str);
                    Players = JsonConvert.DeserializeObject<ObservableCollection<Player>>(FrontLiveRecord.JsonPlayerScores);

                    RoundIndex = FrontLiveRecord.RoundIndex;
                    SessionIndex = FrontLiveRecord.SessionIndex;

                    StateText = FrontLiveRecord.State == (int)GameState.Shuffling ? "洗牌中" : (FrontLiveRecord.State == (int)GameState.Betting ? "押注中" : "开牌中");
                    if (FrontLiveRecord.State == (int)GameState.Dealing)
                    {
                        CountDown = 0;
                    }
                    else
                    {
                        CountDown = FrontLiveRecord.Countdown;
                    }

                    if ((GameState)FrontLiveRecord.State == GameState.Preparing)
                    {
                        bdPrepare.Visibility = Visibility.Visible;
                        txtFrontStateTitle.Text = "前台尚未开始";
                        txtCountdownDescription.Text = "此时可以传送路单";
                    }
                    else if ((GameState)FrontLiveRecord.State == GameState.Shuffling)
                    {
                        bdPrepare.Visibility = Visibility.Visible;
                        txtCountdownDescription.Text = CountDown + "秒开始下一局";
                        txtFrontStateTitle.Text = "正在洗牌";
                    }
                    else
                    {
                        bdPrepare.Visibility = Visibility.Hidden;
                    }

                    DeskBanker = FrontLiveRecord.DeskBanker;
                    DeskPlayer = FrontLiveRecord.DeskPlayer;
                    DeskTie = FrontLiveRecord.DeskTie;
                    DeskScore = Players.Sum(p => p.Balance + p.BetScore.Sum(d => d.Value));

                    Differ = Math.Abs(DeskBanker - DeskPlayer);
                    MostPlayer = Players.FirstOrDefault(player => player.BetScore.Values.Sum() == Players.Max(p => p.BetScore.Values.Sum())).Id;
                    Profit = Desk.GetProfit(FrontLiveRecord.Winner, DeskBanker, DeskPlayer, DeskTie);
                    txtLiveProfit.Foreground = Profit >=0? new SolidColorBrush(Colors.Green): new SolidColorBrush(Colors.Red) ;

                    if (front_sm_waybill_btns.Count != 0 && PreRoundIndex != RoundIndex)
                    {
                        SetFrontWaybillFromFrontSession();
                        PreRoundIndex = RoundIndex;
                    }
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
                Dispatcher.Invoke(new Action(() =>
                {
                    string data_str = Encoding.UTF8.GetString(data, 0, len);
                    var decompress_data = CompressHelper.Decompress(data_str);
                    var sessions = JsonConvert.DeserializeObject<ObservableCollection<Session>>(decompress_data);
                    if (sessions.Count == 0)
                    {
                        MessageBox.Show("前台没有后台传送的路单!");
                        return;
                    }
                    LocalSessions = new ObservableCollection<Session>(sessions.OrderBy(s => s.SessionId));
                    var idx = sessions.Count - 1;
                    SetWaybillFromSession(LocalSessions[idx]);
                    LocalSessionIndex = idx;
                    MessageBox.Show("导入成功!");
                }));
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
        private void btnClearFront_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ClearFrontLocalSessions, "");
            MessageBox.Show("清除成功");
        }
        private void btnAnding_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnCancleAnding_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var cmd = ((Button)sender).Content.ToString();
            switch (cmd)
            {
                case "庄赢":
                case "闲赢":
                case "和赢":
                    BetSide winner = cmd == "庄赢" ? BetSide.banker : cmd == "闲赢" ? BetSide.tie : BetSide.player;
                    SSClient.SendCommand(RemoteCommand.SetWinner, winner);
                    break;
                case "杀大赔小":
                    SSClient.SendCommand(RemoteCommand.KillBig, "");
                    break;
                case "补单":
                    SSClient.SendCommand(RemoteCommand.ExtraWaybill, "");
                    break;
                case "锁定前台":
                    SSClient.SendCommand(RemoteCommand.LockFront, "");
                    break;
                case "解锁前台":
                    SSClient.SendCommand(RemoteCommand.UnlockFront, "");
                    break;
                case "关机":
                    SSClient.SendCommand(RemoteCommand.ShutdownFront, "");
                    break;
                case "当机":
                    SSClient.SendCommand(RemoteCommand.BreakdownFront, "");
                    break;
            }
        }
        private void btnModiPwd_Click(object sender, RoutedEventArgs e)
        {
            SSClient.SendCommand(RemoteCommand.ModifyFrontPassword, PasswordMap);
        }

        private void btnQuitFrontAccount_Click(object sender, RoutedEventArgs e)
        {
            spAccount.Visibility = Visibility.Collapsed;
            dgProfit.Visibility = Visibility.Visible;
        }
    }
}