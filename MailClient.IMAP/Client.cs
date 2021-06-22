using MailClient.IMAP.Commands;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MailClient.IMAP
{
    public class Client : DisposableBase
    {
        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private readonly string _serverRespose = "* OK IMAP4rev1 Service Ready";

        private readonly string _okPatern = @"(a\d{4})\sOK\s.+\scompleted";

        private readonly string _noPatern = @"(a\d{4})\sNO\s.+";

        private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        // Temp ip
        private IPEndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1578);

        private TcpClient TcpClient { get; set; }

        private StreamWriter SWriter { get; set; }

        private StreamReader SReader { get; set; }

        private Thread ListenThread { get; set; }

        private SemaphoreSlim Pool { get; set; }

        private CancellationTokenSource Cancellation { get; set; }

        private int TagCount { get; set; }

        private bool IsCommandSuccess { get; set; } = false;

        public bool IsConnected => TcpClient != null && TcpClient.Connected;

        public bool IsLoggedIn { get; set; }

        private string CurrentData { get; set; }

        public Client()
        {
            TagCount = 0;
            CurrentData = string.Empty;
            Pool = new SemaphoreSlim(0, 1);
            Cancellation = new CancellationTokenSource();
        }

        public async Task<bool> InitiallizeConnectionAsync()
        {
            try
            {
                TcpClient = new TcpClient();

                await TcpClient.ConnectAsync(EndPoint.Address, EndPoint.Port);

                SWriter = new StreamWriter(TcpClient.GetStream());

                SReader = new StreamReader(TcpClient.GetStream());

                ListenThread = new Thread(new ParameterizedThreadStart(Listen));

                ListenThread.Start(Cancellation.Token);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Command.Login(++TagCount, username, password));
                    await SWriter.FlushAsync();

                    await Pool.WaitAsync();

                    IsLoggedIn = IsCommandSuccess;

                    return IsCommandSuccess;
                }

                throw new Exception();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                IsCommandSuccess = false;
            }
        }

        public async Task<bool> SelectMailBoxAsync(string MailBox)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Command.SelectMailBox(++TagCount, MailBox));
                    await SWriter.FlushAsync();

                    await Pool.WaitAsync();

                    return IsCommandSuccess;
                }

                throw new Exception();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                IsCommandSuccess = false;
            }
        }

        public async Task<string[]> GetMailHeaderAsync(int Sequence)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Command.FetchMailHeader(++TagCount, Sequence));
                    await SWriter.FlushAsync();

                    await Pool.WaitAsync();

                    if (IsCommandSuccess)
                    {
                        string[] data = CurrentData.Split('\n');

                        return data[1..(data.Length - 2)];
                    }
                }

                throw new Exception();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                IsCommandSuccess = false;
            }
        }

        public async Task<string> GetMailBodyAsync(int Sequence)
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Command.FetchMailText(++TagCount, Sequence));
                    await SWriter.FlushAsync();

                    await Pool.WaitAsync();

                    if (IsCommandSuccess)
                    {
                        List<string> data = new(CurrentData.Split('\n'));

                        data.RemoveAt(0);
                        _ = data.RemoveAll(i => i == "");

                        string temp = string.Join('\n', data);

                        return temp.Remove(temp.Length - 1);
                    }
                }
                throw new Exception();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                IsCommandSuccess = false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                if (SWriter != null)
                {
                    await SWriter.WriteLineAsync(Command.Logout(++TagCount));
                    await SWriter.FlushAsync();

                    await Pool.WaitAsync();

                    return IsCommandSuccess;
                }

                throw new Exception();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                IsCommandSuccess = false;
            }
        }

        private void Listen(object cancellation)
        {
            try
            {
                CancellationToken token = (CancellationToken)cancellation;
                if (token == null)
                    throw new Exception();

                Regex Successed = new(_okPatern);
                Regex Failed = new(_noPatern);

                string dataTemp = string.Empty;

                while (TcpClient.Connected && !token.IsCancellationRequested)
                {
                    string text = SReader.ReadLine();

                    if (text != _serverRespose)
                    {
                        if (Failed.IsMatch(text))
                        {
                            IsCommandSuccess = false;
                            _ = Pool.Release();
                            continue;
                        }

                        if (Successed.IsMatch(text))
                        {
                            CurrentData = string.Copy(dataTemp);
                            dataTemp = string.Empty;
                            IsCommandSuccess = true;
                            _ = Pool.Release();
                        }
                        else
                        {
                            dataTemp += text == "" ? "\n" : text;
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            TcpClient?.Dispose();
            SReader?.Dispose();
            SWriter?.Dispose();
            Pool?.Dispose();

            if (ListenThread != null && ListenThread.IsAlive)
                Cancellation?.Cancel();

            Cancellation?.Dispose();

            CurrentData = null;

            _disposed = true;

            // Call base class implementation.
            base.Dispose(disposing);
        }
    }
}