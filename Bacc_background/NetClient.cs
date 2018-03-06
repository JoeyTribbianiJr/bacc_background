using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Bacc_background
{
    //[PropertyChanged.ImplementPropertyChanged]
    public class NetClient
    {
        private const int port = 54322;
        private Socket client;
        private Thread thread;
        private static NetClient instance;

        public Image GameImage { get; set; }
        public IPAddress host_ip;

        public static NetClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NetClient();
                }
                return instance;
            }
        }

        private NetClient()
        {
        }

        public void StartClient()
        {
            try
            {
                
                thread = new Thread(new ThreadStart(ReceiveImage));
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
                return;
            }
        }

        private void ReceiveImage()
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipendpiont = new IPEndPoint(host_ip, port);
                client.Connect(ipendpiont);
                //建立终结点
                byte[] b = new byte[1024 * 1000];//图片很大
                MemoryStream ms = new MemoryStream();
                while (client.Connected)
                {
                    client.Receive(b);

                    ms.Write(b, 0, b.Length); 
                    Image img = Image.FromStream(ms);
                    GameImage = img;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
                //thread.Abort();
            }
        }
    }
}
