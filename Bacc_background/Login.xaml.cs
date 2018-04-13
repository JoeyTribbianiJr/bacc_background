using Bacc_background.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Windows;

namespace Bacc_background
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public string Password { get; set; }
        public static Login Instance { get; set; }
        public bool IsSuccessConnect = false;
        public Login()
        {
            InitializeComponent();
            Instance = this;
            cbIps.SelectionChanged += CbIps_SelectionChanged;

            var hosts = JsonConvert.DeserializeObject<ObservableCollection<IpModel>>(Settings.Default.Hosts) ?? new ObservableCollection<IpModel>();
            cbIps.ItemsSource = hosts;
        }

        private void CbIps_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbIps.SelectedItem == null)
            {
                return;
            }
            var host = cbIps.SelectedItem as IpModel;
            txtRemark.Text = host.Remark;
            btnConnect.IsEnabled = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //MainWindow.Instance.Close();
            App.Current.Shutdown();
        }
        private void btnDeleteIp_Click(object sender, RoutedEventArgs e)
        {
            if (cbIps.SelectedItem == null)
            {
                return;
            }
            var host = cbIps.SelectedItem as IpModel;
            var hosts = cbIps.ItemsSource as ObservableCollection<IpModel>;
            hosts.Remove(host);

            Settings.Default.Hosts = JsonConvert.SerializeObject(hosts);
            Settings.Default.Save();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (cbIps.SelectedItem == null)
            {
                return;
            }
            var host = cbIps.SelectedItem as IpModel;
            try
            {
                Password = txtLoginPwd.Text;

                var host_ip = IPAddress.Parse(host.Host);
                MainWindow.Instance.SSClient.Connect(host_ip, MainWindow.port);
                btnConnect.IsEnabled = false;
            }
            catch (Exception ex)
            {
                if (MainWindow.Instance. SSClient.IsConnected)
                {
                    MainWindow.Instance. SSClient.Close();
                }
                MessageBox.Show("连接出错，检查网络或密码");
                btnConnect.IsEnabled = true;
            }
        }

        private void btnAddIp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewIp.Text))
            {
                MessageBox.Show("主机名不能为空");
                return;
            }
            var hosts = JsonConvert.DeserializeObject<ObservableCollection<IpModel>>(Settings.Default.Hosts) ?? new ObservableCollection<IpModel>();
            var new_host = new IpModel
            {
                Host = txtNewIp.Text,
                Remark = txtNewRemark.Text,
            };
            hosts.Add(new_host);
            Settings.Default.Hosts = JsonConvert.SerializeObject(hosts);
            Settings.Default.Save();
            cbIps.ItemsSource = hosts;
            cbIps.SelectedIndex = 0;
        }
    }
    public class IpModel
    {
        public string Host { get; set; }
        public string Remark { get; set; }
        public string Password { get; set; }
    }
}
