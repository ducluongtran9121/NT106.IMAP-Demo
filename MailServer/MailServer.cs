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
    public class MailServer
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
            // try-catch sai port hoặc post đã được sử dụng
            try
            {
                // bắt đầu lắng nghe
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            //try-catch lỗi server và khỏi động lại server
            try
            {
                // disable for test
                // Directory.CreateDirectory(Environment.CurrentDirectory + "ImapMailBox/");
                while (true)
                {
                    // đợi client connect
                    Console.WriteLine("Waiting for new connection...");
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " : " + ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString() + " connected");
                    // tạo thread và lưu thread của client đang connecting
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
                // ngừng lắng nghe
                if (listener != null)
                    listener.Stop();
                //khởi động lại
                Run();
            }
        }

        private void HandleClientMessage(object agrument)
        {
            //một phiên làm việc với một client
            TcpClient client = (TcpClient)agrument;
            string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            string clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
            // try-cacth lỗi client
            try
            {
                // khởi tạo luồng đọc ghi
                StreamReader sr = new StreamReader(client.GetStream());
                StreamWriter sw = new StreamWriter(client.GetStream());
                // cài đặt timeout = 30 phút
                sr.BaseStream.ReadTimeout = 1800000;
                // tạo sessionImap
                ImapSession session = new ImapSession();
                string msg = "";
                string resposed = session.GetResposed("");
                
                sw.WriteLine(resposed);
                sw.Flush();
                // kiểm tra client đang kết nối
                while (client.Connected)
                {
                    // try-cacth timeout
                    try
                    {
                        msg = sr.ReadLine(); // có thể sinh ra exception trong winform xảy ra khi client đột ngột ngắt kết nối
                        if (msg == null) break; //msg = null khi client đột ngột ngắt kết nối chỉ trên console
                        //if (msg == "") continue; // bỏ qua nếu chuỗi trống
                        // trả lời lại các lệnh của client trong session hiện tại
                        resposed = session.GetResposed(msg);
                        if (resposed == "") continue;
                        if(session.GetStartTLS())
                        {
                            byte[] encResponse = session.GetEncrytionResponse(msg);
                            byte[] numSendBytes = session.GetEncrytionResponse(encResponse.Length.ToString());
                            // gửi đi trước thông tin mã hóa chứa độ dài của thông điệp mã hóa cần gửi
                            string sendNumBytes = Encoding.UTF8.GetString(numSendBytes);
                            sw.WriteLine(sendNumBytes);
                            sw.Flush();
                            // gửi thông điệp dưới dạng mã hóa
                            string sendResponse = Encoding.UTF8.GetString(encResponse);
                            sw.WriteLine(sendResponse);
                        }    
                        else sw.WriteLine(resposed);
                        sw.Flush();
                        if (session.GetState() == "Logout") break;
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(ex.InnerException.GetType());
                        sr.Dispose();
                        sr.Close();
                        sw.Dispose();
                        sw.Close();
                        // thông báo timeout cho client
                        sw.WriteLine("* BYE connection timed out");
                        sw.Flush();
                        break;
                    }
                    // in console server
                    Console.WriteLine(msg);
                    Console.WriteLine(resposed);
                }
                Console.WriteLine(clientIP + " : " + clientPort + " disconnected");
                // giải phóng stream và TCPclient connection
                client.Dispose();
                client.Close();
                sr.Dispose();
                sr.Close();
                sw.Dispose();
                sw.Close(); 
                clientconnectionList.Remove(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                client.Dispose();
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