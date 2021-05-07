using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace MailClient
{
    public class MailClient
    {
        private TcpClient client;
        private IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1578);
        private StreamReader sr;
        private StreamWriter sw;
        private Thread listenThread;

        public void Connect()
        {
            client = new TcpClient();
            client.Connect(ipep);
            sr = new StreamReader(client.GetStream());
            sw = new StreamWriter(client.GetStream());
            listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
        }

        public void Send(string msg)
        {
            sw.WriteLine(msg);
            sw.Flush();
        }

        private void Listen()
        {
            try
            {
                while (client.Connected)
                {
                    string text = "";
                    text = sr.ReadLine();
                    Console.WriteLine(text);
                }
            }
            catch { }
        }

        public void Quit()
        {
            sw.WriteLine("QUIT.");
            sw.Flush();
            //listenThread.Abort();
            sr.Close();
            sw.Close();
            client.Close();
        }
    }
}