using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MailServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // disable for test
            // Directory.CreateDirectory(Environment.CurrentDirectory + "ImapMailBox/");
            // contruct for test
            if (Directory.Exists(Environment.CurrentDirectory + "/ImapMailBox/")) Directory.Delete(Environment.CurrentDirectory + "/ImapMailBox/", true);
            Directory.CreateDirectory(Environment.CurrentDirectory + "/ImapMailBox/thihuynh/INBOX/");
            Directory.CreateDirectory(Environment.CurrentDirectory + "/ImapMailBox/thi2256/INBOX/");
            Directory.CreateDirectory(Environment.CurrentDirectory + "/ImapMailBox/son2137/INBOX/");
            //Directory.CreateDirectory(Environment.CurrentDirectory + "/ImapMailBox/luong1815/INBOX/");
            File.Copy(GetProjectDir() + "/Imap/ImapMailBox/thihuynh/INBOX/email_1.msg", Environment.CurrentDirectory + "/ImapMailBox/thihuynh/INBOX/email_1.msg");
            File.Copy(GetProjectDir() + "/Imap/ImapMailBox/thihuynh/INBOX/email_2.eml", Environment.CurrentDirectory + "/ImapMailBox/thihuynh/INBOX/email_2.eml");
            File.Copy(GetProjectDir() + "/Imap/ImapMailBox/thihuynh/INBOX/email_3.eml", Environment.CurrentDirectory + "/ImapMailBox/thihuynh/INBOX/email_3.eml");
            Console.WriteLine("Staring server on port 1578");
            MailServer server1 = new MailServer(1578);
            server1.Start();
            //Console.WriteLine("Staring server on port 1579");
            //MailServer server2 = new MailServer(1579);
            //server2.Start();
        }
        private static string GetProjectDir()
        {
            // lấy directory của project
            Regex regex = new Regex(@"\\MailServer\\bin\\(Debug|Release).*$");
            return regex.Replace(Environment.CurrentDirectory, "\\MailServer");
        }
    }
}