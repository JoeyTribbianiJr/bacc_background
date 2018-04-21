using Bacc_background;
using SuperSocket.ClientEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using WsUtils;

namespace Bacc_front
{
    public class SuperClient : AsyncTcpSession
    {
        //public override int ReceiveBufferSize { get => base.ReceiveBufferSize; set => base.ReceiveBufferSize = value; }
        public SuperClient()
        {
            ReceiveBufferSize = 1024 * 1024 * 4;
            // 连接断开事件
            Closed += Client_Closed;
            // 连接到服务器事件
            Connected += Client_Connected;
            // 发生错误的处理
            Error += Client_Error;
        }
        public void Client_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
        void Client_Connected(object sender, EventArgs e)
        {
            var pass = Login.Instance.Password;
            SendCommand(RemoteCommand.Login, pass);
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.Instance.txtServerIP.Text = HostName;
            }));
        }

        void Client_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("连接关闭:");
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.Instance.btnTransmitBillway.IsEnabled = true;
                Login.Instance.btnConnect.IsEnabled = true;
                MainWindow.Instance.btnConnect.Visibility = Visibility.Visible;
                MainWindow.Instance.btnDisconnect.Visibility = Visibility.Hidden;
            }));
        }
        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect(IPAddress host_ip, int port)
        {
            var server_end = new IPEndPoint(host_ip, port);
            Connect(server_end);
        }
        /// <summary>
        /// 向服务器发命令行协议的数据
        /// </summary>
        /// <param name="key">命令名称</param>
        /// <param name="data">数据</param>
        public void SendStrKeyCommand(string key, string data)
        {
            if (IsConnected)
            {
                byte[] arr = Encoding.UTF8.GetBytes(string.Format("{0} {1}\r\n", key, data));
                try
                {
                    Send(arr, 0, arr.Length);
                }
                catch (Exception)
                {
                    MessageBox.Show("SendCommand函数出错");
                }
            }
            else
            {
                throw new InvalidOperationException("未建立连接");
            }

        }
        public void SendCommand(RemoteCommand cmd, object obj)
        {
            var type = ((int)cmd).ToString().PadLeft(2, '0');
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            if (IsConnected)
            {
                byte[] arr = Encoding.UTF8.GetBytes(string.Format("{0} {1}\r\n", type, data));
                try
                {
                    //Send(arr, 0, arr.Length);
                    Send(arr, 0, arr.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SendCommand函数出错");
                }
            }
            else
            {
                MessageBox.Show("未建立连接");
            }
        }

        public void SendCompressedCommand(RemoteCommand cmd, object obj)
        {
            var type = ((int)cmd).ToString().PadLeft(2, '0');
            var data0 = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            var data = CompressHelper.Compress(data0);
            if (IsConnected)
            {
                byte[] arr = Encoding.UTF8.GetBytes(string.Format("{0} {1}\r\n", type, data));
                try
                {
                    //Send(arr, 0, arr.Length);
                    Send(arr, 0, arr.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SendCommand函数出错");
                }
            }
            else
            {
                MessageBox.Show("未建立连接");
            }
        }

        //public void SendCompressedCommand(RemoteCommand cmd, string data_str)
        //{
        //    var type = ((int)cmd).ToString().PadLeft(2, '0');
        //    //var data0 = Newtonsoft.Json.JsonConvert.SerializeObject(data_str);
        //    var data = Compress(data_str);
        //    if (IsConnected)
        //    {
        //        byte[] arr = Encoding.UTF8.GetBytes(string.Format("{0} {1}\r\n", type, data));
        //        try
        //        {
        //            //Send(arr, 0, arr.Length);
        //            Send(arr, 0, arr.Length);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("SendCommand函数出错");
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("未建立连接");
        //    }
        //}

    }

}
