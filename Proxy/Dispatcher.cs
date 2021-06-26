using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Proxy
{
    public class Dispatcher
    {
        // set the TcpListener on port 1580
        int port = 1580;
        TcpListener listener;
        // List of processors that is called as C-P-S session
        static List<CoreComm> processors = new List<CoreComm>();
        // List of clients
        static List<TcpClient> clients = new List<TcpClient>();
        public Dispatcher()
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void ListenForRequests()
        {
            listener.Start();
            while (true)
            {
                try
                {
                    // Start listening for client requests
                    Console.WriteLine("Waiting for a connection...");
                    // Enter the listening loop
                    while (true)
                    {
                        // Perform a blocking call to accept requests.
                        TcpClient client = listener.AcceptTcpClient();
                        clients.Add(client);
                        // Get IP and port or client
                        string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                        string clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
                        Console.WriteLine("Connected from " + clientIP + " : " + clientPort);
                        Thread t = new Thread(ThreadProc);
                        t.Start(clients[clients.Count - 1]);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                }
            }
        }
        private static void ThreadProc(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var processor = new CoreComm(client);
            try
            {
                processor.ReSendRequest();
            }
            catch
            {
                processors.Remove(processor);
                clients.Remove(client);
            }
        }
    }
}
