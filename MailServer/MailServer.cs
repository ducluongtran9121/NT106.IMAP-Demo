using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MailServer.Imap;

namespace MailServer
{
    internal class MailServer
    {
        private TcpListener listener;
        private Thread serverThread;
        private List<TcpClient> clientconnectionList = new List<TcpClient>();

        public MailServer(int port)
        {
            this.listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            try
            {
                serverThread = new Thread(new ThreadStart(Run));
                serverThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Run()
        {
            try
            {
                listener.Start();
                while (true)
                {
                    MakeServerDirctory();
                    Console.WriteLine("Waiting for new  the connection...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " : " + ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString() + " connected");
                    clientconnectionList.Add(client);
                    Thread t = new Thread(HandleClientMessage);
                    t.Start(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        private void MakeServerDirctory()
        {
            string path = Environment.CurrentDirectory + @"\Data";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private void HandleClientMessage(object agrument)
        {
            TcpClient client = (TcpClient)agrument;
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            string clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
            try
            {
                StreamReader sr = new StreamReader(client.GetStream());
                StreamWriter sw = new StreamWriter(client.GetStream());

                sr.BaseStream.ReadTimeout = 1800000;
                sw.BaseStream.ReadTimeout = 1800000;

                ImapSession session = new ImapSession();
                string msg = "";
                string resposed = session.GetResposed();
                sw.WriteLine(resposed);
                sw.Flush();

                while (client.Connected)
                {
                    try
                    {
                        msg = sr.ReadLine(); // có thể sinh ra exception trong winform xảy ra khi client đột ngột ngắt kết nối
                        if (msg == null) break; //msg = null khi client đột ngột ngắt kết nối chỉ trên console
                        resposed = session.GetResposed(msg);
                        sw.WriteLine(resposed);
                        sw.Flush();
                    }
                    catch (IOException)
                    {
                        sw.WriteLine("* BYE connection timed out");
                        sw.Flush();
                        break;
                    }

                    Console.WriteLine(msg);
                }
                Console.WriteLine(clientIP + " : " + clientPort + " disconnected");
                clientconnectionList.Remove(client);
                sr.Close();
                sw.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client.Close();
                clientconnectionList.Remove(client);
                Console.WriteLine(clientIP + " : " + clientPort + " disconnected");
            }
        }

        private void DisconnectWithClient(TcpClient client)
        {
            if (clientconnectionList.Remove(client))
            {
                Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " : " + ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString() + "is Removed");
            }
            else
            {
                Console.WriteLine("client not found");
            }
        }

        public void Stop()
        {
            for (int i = 0; i < clientconnectionList.Count; i++) clientconnectionList[i].Close();
            listener.Stop();
        }
    }
}