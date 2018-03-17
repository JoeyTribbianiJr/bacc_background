using SuperSocket.ClientEngine;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Bacc_front
{
    public class SuperClient : AsyncTcpSession
    {
        //public override int ReceiveBufferSize { get => base.ReceiveBufferSize; set => base.ReceiveBufferSize = value; }
        public SuperClient()
        {
            ReceiveBufferSize = 1024 * 1024;
            // 连接断开事件
            Closed += Client_Closed;
            // 收到服务器数据事件
            //DataReceived += Client_DataReceived;
            // 连接到服务器事件
            Connected += Client_Connected;
            // 发生错误的处理
            Error += Client_Error;

        }
        public void Client_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            //Console.WriteLine(e.Exception.Message);
        }
        void Client_Connected(object sender, EventArgs e)
        {
            MessageBox.Show(Client.RemoteEndPoint + "：连接成功");
        }
        

        
        void Client_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("连接断开");
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
        public void SendCommand(string key, string data)
        {
            if (IsConnected)
            {
                byte[] arr = Encoding.UTF8.GetBytes(string.Format("{0} {1}\r\n", key, data));
                Send(arr, 0, arr.Length);
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
                Send(arr, 0, arr.Length);
            }
            else
            {
                MessageBox.Show("未建立连接");
            }
        }

    }
    
}
