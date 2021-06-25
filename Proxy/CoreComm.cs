using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;
using System.Text;
using System.IO;

namespace Proxy
{
    public class IamServer
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public IamServer(string Ip, int port)
        {
            this.IP = Ip;
            this.Port = port;
        }
    }

    public class CoreComm
    {
        // Buffer for reading data
        int bufSize = 1024;
        // List of servers
        static List<IamServer> servers = new List<IamServer>();  
        // Client <-> Proxy socket
        protected TcpClient C2PSocket;
        NetworkStream C2PStream;
        // Proxy <-> Server socket
        protected TcpClient P2SSocket;


        public CoreComm()
        {
            // reading config for servers' parameters
            IamServer server1 = new IamServer("127.0.0.1", 1578);
            servers.Add(server1);
            IamServer server2 = new IamServer("127.0.0.1", 1579);
            servers.Add(server2);
        }

        public CoreComm(TcpClient socket)
        {
            C2PSocket = socket;
            // Get a stream object for reading and writing
            C2PStream = C2PSocket.GetStream();
        }


        public void ReSendRequest()
        {
            // Select server to redirect client
            var servers = GetDestinationServers();

            // Notify if no server exists
            string errMessage = "No Server ready!!\n\r";
            byte[] ErrMessage = Encoding.ASCII.GetBytes(errMessage);
            if (servers.Count == 0)
                C2PStream.Write(ErrMessage, 0, ErrMessage.Length);

            else
                // for debug only send the first in the list
                Console.WriteLine("Redirect to server " + servers[0].IP + " : " + servers[0].Port);
                SendRequestToServer(servers[0]);

        }

        public void SendRequestToServer(IamServer server)
        {
            // Initialize socket between Proxy and Server
            P2SSocket = new TcpClient();
            // Create connection between Proxy and Server
            P2SSocket.Connect(IPAddress.Parse(server.IP), server.Port);
            NetworkStream P2SStream = P2SSocket.GetStream();

            StreamReader srP2S = new StreamReader(P2SStream);
            StreamWriter swP2S = new StreamWriter(P2SStream);
            StreamReader srC2P = new StreamReader(C2PStream);
            StreamWriter swC2P = new StreamWriter(C2PStream);
            
            // Get IP and port or client
            string clientIP = ((IPEndPoint)C2PSocket.Client.RemoteEndPoint).Address.ToString();
            string clientPort = ((IPEndPoint)C2PSocket.Client.RemoteEndPoint).Port.ToString();
            // Get IP and port or server
            string serverIP = ((IPEndPoint)P2SSocket.Client.RemoteEndPoint).Address.ToString();
            string serverPort = ((IPEndPoint)P2SSocket.Client.RemoteEndPoint).Port.ToString();
            // Response: * OK IMAP4rev1 Service Ready
            string response = srP2S.ReadLine();
            swC2P.WriteLine(response);
            swC2P.Flush();
            // Commands from client
            string msg = "";
            while (P2SSocket.Connected)
            { 
                try
                {
                    // Read command from client
                    msg = srC2P.ReadLine();
                    // Sudden disconnection from client 
                    if (msg == null) break;
                    // Send Command to server
                    swP2S.WriteLine(msg);
                    swP2S.Flush();

                    var receiveBuffer = new byte[bufSize];
                    // Read response from server in bytes instead of sr.Readline
                    var count = P2SStream.Read(receiveBuffer, 0, bufSize);
                    response = Encoding.UTF8.GetString(receiveBuffer, 0, count);
                    // Return client the server response 
                    swC2P.WriteLine(response.Trim());
                    swC2P.Flush();

                    // Commands with only tag
                    if (msg.Split(' ').Length == 1) continue;
                    // Client call logout command 
                    if ((msg.Split(' ')[1]).ToLower() == "logout") break;
                     
                }
                catch
                {
                    // Close streams
                    swP2S.Dispose();
                    swP2S.Close();
                    srC2P.Dispose();
                    srC2P.Close();
                    swC2P.Dispose();
                    swC2P.Close();
                    break;
                }
            }
            // Shutdown and end connection
            Console.WriteLine(clientIP + " : " + clientPort + " disconnected");
            C2PSocket.Close();
            Console.WriteLine("Disconnect with server " + serverIP + " : " + serverPort);
            P2SSocket.Close();
            // Close streams
            swP2S.Dispose();
            swP2S.Close();
            srC2P.Dispose();
            srC2P.Close();
            swC2P.Dispose();
            swC2P.Close();
        }

        // Round Robin algorithm
        static int lastServerAnswered = 0;

        public List<IamServer> GetDestinationServers()
        {
            // processing to determine the query destinations
            lock (servers)
            {
                int currentServerNum = lastServerAnswered;
                lastServerAnswered++;
                if (lastServerAnswered > servers.Count - 1)
                    lastServerAnswered = 0;

                return new List<IamServer> { servers[currentServerNum] };
            }
        }

    }
}
