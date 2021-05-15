using IMAP.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IMAP
{
    public class MailClient
    {
        private readonly string ServerRespose = "* OK IMAP4rev1 Service Ready";

        private IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1578);

        private static readonly object Lock = new object();

        private TcpClient Client { get; set; }

        private StreamWriter SWriter { get; set; }

        private StreamReader SReader { get; set; }

        private Thread ListenThread { get; set; }

        private ImapSession Session { get; set; }

        private bool IsDataReady { get; set; } = false;

        private string CurrentData { get; set; }

        public MailClient()
        {
            
        }

        public async Task<bool> InitiallizeConnection()
        {
            try
            {
                Client = new TcpClient();

                Session = new ImapSession();

                await Client.ConnectAsync(EndPoint.Address, EndPoint.Port);

                SWriter = new StreamWriter(Client.GetStream());

                SReader = new StreamReader(Client.GetStream());

                ListenThread = new Thread(new ThreadStart(Listen));

                ListenThread.Start();

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Login()
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Session.Login());
                    await SWriter.FlushAsync();
                    IsDataReady = false;
                    return true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch(Exception) 
            {
                return false;
            }
        }

        public async Task<bool> SelectMailBox(string MailBox)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Session.SelectMailBox(MailBox));
                    await SWriter.FlushAsync();

                    while (!IsDataReady) { }

                    IsDataReady = false;

                    return true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<List<string>> GetMailHeader(int Sequence)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Session.FetchMailHeader(Sequence));

                    await SWriter.FlushAsync();

                    while (!IsDataReady) { }

                    IsDataReady = false;

                    await Task.Delay(500);

                    List<string> data = new List<string>(CurrentData.Split('\n'));

                    data.RemoveAt(0);
                    data.RemoveAt(data.Count - 1);
                    data.RemoveAt(data.Count - 1);

                    return data;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception) 
            {
                return null;
            }
        }

        public async Task<string> GetMailText(int Sequence)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Session.FetchMailText(Sequence));
                    await SWriter.FlushAsync();

                    while (!IsDataReady) { }

                    IsDataReady = false;

                    await Task.Delay(100);

                    List<string> data = new List<string>(CurrentData.Split('\n'));

                    data.RemoveAt(0);
                    data.RemoveAll(i => i == "");

                    string temp = string.Join('\n', data);

                    return temp.Remove(temp.Length - 1);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void Listen()
        {
            try
            {
                Regex regex = new Regex(@"(a\d{4})\sOK\s.+\scompleted");

                string dataTemp = string.Empty;

                while (Client.Connected)
                {
                    string text = SReader.ReadLine();

                    if (text != ServerRespose)
                    {
                        if (regex.IsMatch(text))
                        {
                            lock(Lock)
                            {
                                CurrentData = dataTemp;
                                dataTemp = string.Empty;
                                IsDataReady = true;
                            }
                        }
                        else
                        {
                            dataTemp += text == "" ? "\n" : text;
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}