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
        static public string ReturnLogoutResponse(string tag,ref string state)
        {
            state = "Logout";
            return "* BYE IMAP4rev1 Server logging out\n\r" + tag + " OK LOGOUT completed";
        }

        static public string ReturnCapabilityResponse(string tag)
        {
            return "* CAPABILITY IMAP4rev1 AUTH=LOGIN AUTH=PLAIN\n\r" + tag + " OK CAPABILITY completed";
        }

        // not authenticate state
        static public string ReturnLoginResponse(string tag, string[] arguments, ref string state, ref string userSession)
        {
            // kiểm tra đối số của lênh login
            if (arguments == null || arguments.Length < 2) return Response.ReturnParseErrorResponse(tag, "LOGIN");

            // kiểm tra tên tài khoảng và mật khẩu trong ImapDB
            List<User> userInfo = SqliteQuery.LoadUserInfo(arguments[0].Split('@')[0], arguments[1]);
            //báo lỗi nếu user không tồn tại
            if (userInfo.Count == 0) return tag + " NO LOGIN failed";
            state = "auth";
            userSession = userInfo[0].name;
            return tag + " OK LOGIN completed";
        }

        //authenticate state
        static public string ReturnSelectedResponse(string tag, string[] arguments, ref string state, string userSession, ref string userMailBox)
        {
            // kiểm tra đối số của lênh select
            if (arguments == null) return ReturnParseErrorResponse(tag, "SELECT");
            //truy xuất thông tin về mailbox từ ImapDB
            List<MailBox> mailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, arguments[0]);
            //báo lỗi nếu mailbox không tồn tại
            if (mailBoxInfo.Count == 0) return tag + " NO Mailbox does not exist";

            // thay đổi trạng thái sang selected state
            state = "selected";
            // gán usermailbox cho session
            userMailBox = arguments[0];
            // gán thông tin trả về vào response
            string respose = "";
            respose += $"* { mailBoxInfo[0].exists} EXISTS\n\r";
            respose += $"* {mailBoxInfo[0].recent} RECENT\n\r";
            respose += $"* OK [UNSEEN {mailBoxInfo[0].firstunseen}] Message {mailBoxInfo[0].firstunseen} is first unseen\n\r";
            respose += $"* OK [UIDVALIDITY {mailBoxInfo[0].uidvalidity}] UIDs valid\n\r";
            respose += $"* OK [UIDNEXT {mailBoxInfo[0].uidnext}] Predicted next UID\n\r";
            respose += @"* OK [PERMANENTFLAGS (\Seen \Answered \Flagged \Deleted \Draft] ." + "\n\r";
            respose += tag + " OK [READ-WRITE] SELECT completed";

            return respose;
        }

        private static MailMessage GetMail(string path)
        {
            //dịch mail bằng msgReader vào đưa vào MailMessage
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
            // kiểm tra số đối số của lệnh fetch
            if (arguments == null || arguments.Length < 2) ReturnParseErrorResponse(tag, "FETCH");
            //kiểm tra fetch number
            uint mailNum;
            if (!UInt32.TryParse(arguments[0], out mailNum)) return ReturnParseErrorResponse(tag, "FETCH");
            // truy vấn UID của fetch number
            int MailUID = SqliteQuery.LoadUIDMail(mailNum);
            // trả về lỗi nếu không tồn tại fetch number
            if (MailUID == -1) return ReturnParseErrorResponse(tag, "FETCH");

            string respose = "";
            // kiểm tra đối số của fetch
            if (arguments[1].ToLower() != "body[header]" && arguments[1].ToLower() != "body[text]") return ReturnParseErrorResponse(tag, "FETCH");

            // ...\MailServer\Imap\MailBoxImap\....
            string path = Response.GetProjectDir() + $"\\Imap\\ImapMailBox\\{userSession}\\{userMailBox.ToLower()}";

            // đọc mail được lưu trong mail box
            MailMessage message = GetMail(path + $"\\email_{MailUID}.eml");
            if (arguments[1].ToLower() == "body[header]")
            {
                respose += $"From: {message.From}\n\r";
                respose += $"To: {message.To}\n\r";
                respose += $"Subject: {message.Subject}\n\r";
            }
            if (arguments[1].ToLower() == "body[text]") respose += message.Body;
            respose = $"* {arguments[0]} FETCH({arguments[1]} " + "{" + respose.Length + "} \n\r" + respose + ")\n\r";
            respose += tag + " OK FETCH completed";

            return respose;
        }

        private static bool IsListPermanentFlags(string[] arguments)
        {
            //kiểm tra danh sách flag nhập vào
            if (!arguments[4].StartsWith('(')) return IsPermanentFlags(arguments[4]);
            else
            {
                arguments[4].Replace("(", string.Empty);
                arguments[arguments.Length - 1].Replace(")", string.Empty);
                for (int i = 5; i < arguments.Length; i++)
                    if (!IsPermanentFlags(arguments[i])) return false;
                return true;
            }
        }

        private static bool IsPermanentFlags(string flag)
        {
            // kiểm tra flag nhập vào
            switch (flag.ToLower())
            {
                case "\\seen":
                case "\\answered":
                case "\\flagged":
                case "\\deleted":
                case "\\draft":
                    return true;

                default:
                    return false;
            }
        }

        private static string GetProjectDir()
        {
            // lấy directory của project
            Regex regex = new Regex(@"\\MailServer\\bin\\(Debug|Release).*$");
            return regex.Replace(Environment.CurrentDirectory, "\\MailServer");
        }
    }
}