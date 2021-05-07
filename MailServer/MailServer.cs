using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MailServer
{
    internal class MailServer
    {
        private TcpListener listener;
        private List<TcpClient> clientconnectionList = new List<TcpClient>();

        public MailServer(int port)
        {
            this.listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            try
            {
                Thread serverThread = new Thread(new ThreadStart(Run));
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
                    Console.WriteLine("Waiting for new  the connection...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("New client connected");
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

        private void HandleClientMessage(object agrument)
        {
            TcpClient client = (TcpClient)agrument;
            try
            {
                StreamReader sr = new StreamReader(client.GetStream());
                //sr.BaseStream.ReadTimeout = 10000;
                StreamWriter sw = new StreamWriter(client.GetStream());
                sw.WriteLine("* OK IMAP4rev1 Service Ready");
                sw.Flush();
                string lineContent = "";
                while (client.Connected)
                {
                    try
                    {
                        lineContent = sr.ReadLine();
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    if (lineContent == null) break;
                    Console.WriteLine(lineContent);
                }
                Console.WriteLine("One client disconnected");
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
                Console.WriteLine("One client disconnected");
            }
        }

        private void DisconnectWithClient(TcpClient client)
        {
            if (clientconnectionList.Remove(client))
            {
                Console.WriteLine("One client is Removed");
            }
            else
            {
                Console.WriteLine("client not found");
            }
        }

        public void Stop()
        {
            for (int i = 0; i < clientconnectionList.Count; i++) clientconnectionList[i].Close();
            clientconnectionList.Clear();
            listener.Stop();
        }
    }
}