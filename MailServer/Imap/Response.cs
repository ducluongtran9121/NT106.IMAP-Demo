using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MailServer.Imap
{
    internal static class Response
    {
        // syntax error
        static public string ReturnInvaildCommandResponse(string tag)
        {
            return tag + " BAD Invalid command";
        }

        static public string ReturnBadStateResponse(string tag, string command)
        {
            return tag + " BAD Bad state for " + command.ToUpper();
        }

        static public string ReturnParseErrorResponse(string tag, string command)
        {
            return tag + " BAD " + command.ToUpper() + " parse error";
        }

        static public string ReturnParseErrorResponse(string tag)
        {
            return tag + " BAD parse error";
        }

        static public string ReturnMissingTagResponse()
        {
            return "* BAD missing tag";
        }

        // any state
        static public string ReturnLogoutResponse(string tag)
        {
            return "* BYE IMAP4rev1 Server logging out\n\r" + tag + " OK LOGOUT completed";
        }

        static public string ReturnCapabilityResponse(string tag)
        {
            return "* CAPABILITY IMAP4rev1 AUTH=LOGIN AUTH=PLAIN\n\r" + tag + " OK CAPABILITY completed";
        }

        // not authenticate state
        static public string ReturnLoginResponse(string tag, string[] arguments, ref string state, ref string userSession)
        {
            if (arguments == null || arguments.Length < 2) return Response.ReturnParseErrorResponse(tag, "LOGIN");
            else
            {
                FileStream fs = new FileStream(Response.GetProjectDir() + @"\Data\user_password", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                StreamReader sr = new StreamReader(fs);
                string lineContent = "";
                while ((lineContent = sr.ReadLine()) != null)
                {
                    string[] user_pass = lineContent.Split(' ');
                    if (user_pass[0] == arguments[0] && user_pass[1] == arguments[1])
                    {
                        sr.Close();
                        fs.Close();
                        userSession = user_pass[0].Split('@')[0];
                        state = "auth";
                        return tag + " OK LOGIN completed";
                    }
                }
                sr.Close();
                fs.Close();
                return tag + " NO LOGIN failed";
            }
        }

        //authenticate state
        static public string ReturnSelectedResponse(string tag, string[] arguments, ref string state, string userSession, ref string userMailBox)
        {
            if (arguments == null || arguments.Length < 1) return ReturnParseErrorResponse(tag, "SELECT");
            string path = Response.GetProjectDir() + $"\\Data\\{userSession}\\{arguments[0].ToLower()}";
            if (!Directory.Exists(path)) return tag + " NO Mailbox does not exist";
            string[] mailBoxInfo = Response.ReadMailBoxInfo(path);
            int firstUnseen = Response.GetFirstUnseen(path);
            string respose = "";
            respose += $"* { mailBoxInfo[0]} EXISTS\n\r";
            respose += $"* {mailBoxInfo[1]} RECENT\n\r";
            respose += $"* OK [UNSEEN {firstUnseen.ToString()}] Message {firstUnseen.ToString()} is first unseen\n\r";
            respose += $"* OK [UIDVALIDITY {mailBoxInfo[2]}] UIDs valid\n\r";
            respose += $"* OK [UIDNEXT {mailBoxInfo[3]}] Predicted next UID\n\r";
            respose += @"* OK [PERMANENTFLAGS (\Seen \Answered \Flagged \Deleted \Draft] ." + "\n\r";
            respose += tag + " OK [READ-WRITE] SELECT completed";
            state = "selected";
            userMailBox = arguments[0];
            return respose;
        }

        private static string[] ReadMailBoxInfo(string path)
        {
            string[] mailBoxInfo = new string[4];
            FileStream fs = new FileStream(path + @"\MailBoxInfo.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs);
            for (int i = 0; i < 4; i++)
                mailBoxInfo[i] = sr.ReadLine().Split(' ')[1];
            sr.Close();
            fs.Close();
            return mailBoxInfo;
        }

        private static int GetFirstUnseen(string path)
        {
            int firstUnseen = 0;
            FileStream fs = new FileStream(path + @"\MailInfo.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs);
            string lineContent = "";
            while ((lineContent = sr.ReadLine()) != null)
            {
                firstUnseen++;
                if (lineContent.Split(' ').Length < 2) break;
            }
            sr.Close();
            fs.Close();
            return firstUnseen;
        }

        private static MailMessage GetMail(string path)
        {
            MailMessage mailMessage = new MailMessage();
            FileInfo fileInfo = new FileInfo(path);
            var emlFileMessage = MsgReader.Mime.Message.Load(fileInfo);
            mailMessage.From = new MailAddress(emlFileMessage.Headers.From.Address);
            foreach (var recipient in emlFileMessage.Headers.To)
                mailMessage.To.Add(recipient.Address);
            mailMessage.Subject = emlFileMessage.Headers.Subject;
            mailMessage.Body = System.Text.Encoding.UTF8.GetString(emlFileMessage.TextBody.Body);
            return mailMessage;
        }

        //selected state
        public static string ReturnFetchResponse(string tag, string[] arguments, string userSession, string userMailBox) //mới được có 2 cái header và text body thôi nha mấy cha
        {
            if (arguments == null || arguments.Length < 2) ReturnParseErrorResponse(tag, "FETCH");
            int mailNum;
            string respose = "";
            if (!Int32.TryParse(arguments[0], out mailNum)) return ReturnParseErrorResponse(tag, "FETCH");
            if (mailNum < 1 || (arguments[1].ToLower() != "body[header]" && arguments[1].ToLower() != "body[text]")) return ReturnParseErrorResponse(tag, "FETCH");
            string path = Response.GetProjectDir() + $"\\Data\\{userSession}\\{userMailBox.ToLower()}";
            FileStream fs = new FileStream(path + @"\MailInfo.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader sr = new StreamReader(fs);
            string lineContent = "";
            string email = "";
            int i = 0;
            while ((lineContent = sr.ReadLine()) != null)
            {
                i++;
                if (i == mailNum)
                {
                    email = lineContent.Split(' ')[0];
                    break;
                }
            }
            if (i < mailNum) return ReturnParseErrorResponse(tag, "FETCH");
            MailMessage message = GetMail(path + $"\\{email}.eml");
            if (arguments[1].ToLower() == "body[header]")
            {
                respose += $"From: {message.From}\n\r";
                respose += $"To: {message.To}\n\r";
                respose += $"Subject: {message.Subject}\n\r";
            }
            if (arguments[1].ToLower() == "body[text]") respose += message.Body;
            respose = $"* {arguments[0]} FETCH({arguments[1]} " + "{" + respose.Length + "} \n\r" + respose + ")\n\r";
            respose += tag + " OK FETCH completed";
            sr.Close();
            fs.Close();
            return respose;
        }

        private static string GetProjectDir()
        {
            Regex regex = new Regex(@"\\MailServer\\bin.*$");
            return regex.Replace(Environment.CurrentDirectory, "\\MailServer");
        }
    }
}