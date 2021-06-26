﻿using System;
using System.Linq;
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
        public static string ReturnInvaildCommandResponse(string tag)
        {
            return tag + " BAD Invalid command";
        }

        public static string ReturnBadStateResponse(string tag, string command)
        {
            return tag + " BAD Bad state for " + command.ToUpper();
        }

        public static string ReturnParseErrorResponse(string tag, string command)
        {
            return tag + " BAD " + command.ToUpper() + " parse error";
        }

        public static string ReturnParseErrorResponse(string tag)
        {
            return tag + " BAD parse error";
        }

        public static string ReturnMissingTagResponse()
        {
            return "* BAD missing tag";
        }

        // any state
        public static string ReturnLogoutResponse(string tag,ref string state)
        {
            state = "Logout";
            return "* BYE IMAP4rev1 Server logging out\r\n" + tag + " OK LOGOUT completed";
        }

        public static string ReturnCapabilityResponse(string tag)
        {
            return "* CAPABILITY IMAP4rev1 NAMESPACE AUTH=LOGIN AUTH=PLAIN STARTTLS ACL UNSELECT UIDPLUS QUOTA BINARY\r\n" + tag + " OK CAPABILITY completed";
        }

        public static string ReturnNoopCommand(string tag)
        {
            return tag + " OK NOOP completed";
        }

        // not authenticate state
        public static string ReturnLoginResponse(string tag, string argument, ref string state, ref string userSession)
        {
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+)) (\"(?:[^\"]*)\"|(?:[^\\s]+))");
            // kiểm tra đối số của lênh login
            if (!math.Success) return Response.ReturnParseErrorResponse(tag, "LOGIN");
            // lấy username từ group
            string username = math.Groups[1].Value.Replace("\"", "");
            // lấy password từ group
            string password = math.Groups[2].Value.Replace("\"", "");
            // kiểm tra tên tài khoảng và mật khẩu trong ImapDB
            List<User> userInfo = SqliteQuery.LoadUserInfo(username.Split('@')[0],password);
            //báo lỗi nếu user không tồn tại
            if (userInfo.Count == 0) return tag + " NO LOGIN failed";
            state = "auth";
            userSession = userInfo[0].name;
            return tag + " OK LOGIN completed";
        }

        //authenticate state
        public static string ReturnSelectedResponse(string tag, string argument, ref string state, string userSession, ref string userMailBox)
        {
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+))");
            // kiểm tra đối số lệnh select
            if(!math.Success) return Response.ReturnParseErrorResponse(tag, "SELECT");
            string mailbox = math.Groups[1].Value.Replace("\"","");
            //truy xuất thông tin về mailbox từ ImapDB
            List<MailBox> mailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailbox);
            //báo lỗi nếu mailbox không tồn tại
            if (mailBoxInfo.Count == 0) return tag + " NO Mailbox does not exist";
            // thay đổi trạng thái sang selected state
            state = "selected";
            // gán usermailbox cho session
            userMailBox = mailbox;
            // gán thông tin trả về vào response
            string respose = "";
            respose += $"* {mailBoxInfo[0].mailexists} EXISTS\r\n";
            respose += $"* {mailBoxInfo[0].recent} RECENT\r\n";
            if (mailBoxInfo[0].firstunseen >0) respose += $"* OK [UNSEEN {mailBoxInfo[0].firstunseen}] Message {mailBoxInfo[0].firstunseen} is first unseen\r\n";
            respose += $"* OK [UIDVALIDITY {mailBoxInfo[0].uidvalidity}] UIDs valid\r\n";
            respose += $"* OK [UIDNEXT {mailBoxInfo[0].uidnext}] Predicted next UID\r\n";
            respose += @"* FLAGS (\Answered \Flagged \Deleted \Seen \Draft)" + "\r\n";
            respose += @"* OK [PERMANENTFLAGS (\Answered \Flagged \Deleted \Seen \Draft)] " + "\r\n";
            respose += tag + " OK [READ-WRITE] SELECT completed";
            int success;
            if (mailBoxInfo[0].recent==0) success = SqliteQuery.UpdateRecentFlag(userSession, userMailBox); 
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
        public static string ReturnListResponse(string tag, string argument, string userSession)
        {
            string response = "";
            string root = Environment.CurrentDirectory + $"\\ImapMailBox\\{userSession}\\";
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+)) (\"(?:[^\"]*)\"|(?:[^\\s]+))");
            if(!math.Success) return Response.ReturnParseErrorResponse(tag, "LIST");
            // lấy reference từ group
            string reference = math.Groups[1].Value.Replace("\"","");
            string dir=root;
            if (Regex.IsMatch(reference, @"^(./)?.*")) dir +=reference.Replace("./", "");
            // lấy mailboxName từ group
            string mailboxName = math.Groups[2].Value.Replace("\"","");
            // reference = root và mailboxName =""
            if (dir == root && mailboxName=="") return "* LIST (\\Noselect ) \"/\" \"\"\r\n" + tag + " OK LIST completed";
            // kiểm tra đường dẫn
            // kiểm tra mailboxName bằng "*","%"
            if (!Directory.Exists(dir+mailboxName) && mailboxName != "*" && mailboxName != "%") return tag+" OK LIST completed";
            int temp = 0;
            if (mailboxName == "%")
            {
                temp = 1;
                mailboxName = "*";
            } 
                
            
            string[] dirList = Directory.GetDirectories(dir, mailboxName);
            foreach(string mailboxDir in dirList)
            {
                // nếu mailboxName bằng "%" kiểm tra folder không có subfolder
                if (temp == 1 && Directory.GetDirectories(mailboxDir).Length != 0) continue;
                string mailbox = mailboxDir.Replace(root, "");
                List<MailBox> ListmailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailbox);
                if(ListmailBoxInfo.Count==0) return tag + " OK LIST completed";
                MailBox mailBoxInfo = ListmailBoxInfo[0];
                string[] tempArr =
                {
                    (mailBoxInfo.all == 1?"\\All":""),
                    (mailBoxInfo.archive == 1?"\\Archive":""),
                    (mailBoxInfo.drafts == 1?"\\Drafts":""),
                    (mailBoxInfo.flagged == 1?"\\Flagged":""),
                    (mailBoxInfo.haschildren == 1?"\\HasChildren":""),
                    (mailBoxInfo.hasnochildren == 1?"\\HasNoChildren":""),
                    (mailBoxInfo.important == 1?"\\Important":""),
                    (mailBoxInfo.inbox == 1?"\\Inbox":""),
                    (mailBoxInfo.junk == 1?"\\Junk":""),
                    (mailBoxInfo.marked == 1?"\\Marked":""),
                    (mailBoxInfo.nointeriors == 1?"\\NoInferiors":""),
                    (mailBoxInfo.noselect == 1?"\\Noselect":""),
                    (mailBoxInfo.sent == 1?"\\Sent":""),
                    (mailBoxInfo.trash == 1?"\\Trash":""),
                };
                tempArr = tempArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                response += "* LIST (" + string.Join(' ', tempArr) + $") \"/\" \"{reference}{mailbox}\"\r\n";
            }
            return response + tag + " OK LIST completed";
        }
        public static string ReturnSubcribeResponse(string tag,string argument,string userSession)
        {
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+))");
            // kiểm tra đối số lệnh subcribe
            if (!math.Success) return Response.ReturnParseErrorResponse(tag, "SUBSCRIBE");
            string mailbox = math.Groups[1].Value.Replace("\"", "");
            int success = SqliteQuery.UpdateMailBoxSubcribed(userSession, mailbox,1);
            if (success == 1) return tag + " OK SUBSCRIBE completed";
            return tag + " NO Mailbox does not exist";
        }
        public static string ReturnUnsubcribeResponse(string tag, string argument, string userSession)
        {
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+))");
            // kiểm tra đối số lệnh unsubcribe
            if (!math.Success) return Response.ReturnParseErrorResponse(tag, "UNSUBSCRIBE");
            string mailbox = math.Groups[1].Value.Replace("\"", "");
            int success = SqliteQuery.UpdateMailBoxSubcribed(userSession, mailbox, 0);
            if (success == 1) return tag + " OK UNSUBSCRIBE completed";
            return tag + " NO Mailbox does not exist";
        }
        public static string ReturnLsubResponse(string tag, string argument, string userSession)
        {
            string response = "";
            string root = Environment.CurrentDirectory + $"\\ImapMailBox\\{userSession}\\";
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+)) (\"(?:[^\"]*)\"|(?:[^\\s]+))");
            if (!math.Success) return Response.ReturnParseErrorResponse(tag, "LSUB");
            // lấy reference từ group
            string reference = math.Groups[1].Value.Replace("\"", "");
            string dir = root;
            if (Regex.IsMatch(reference, @"^(./)?.*")) dir += reference.Replace("./", "");
            // lấy mailboxName từ group
            string mailboxName = math.Groups[2].Value.Replace("\"", "");
            // reference = root và mailboxName =""
            if (dir == root && mailboxName == "") return "* LSUB (\\Noselect ) \"/\" \"\"\r\n" + tag + " OK LSUB completed";
            // kiểm tra đường dẫn
            // kiểm tra mailboxName bằng "*","%"
            if (!Directory.Exists(dir + mailboxName) && mailboxName != "*" && mailboxName != "%") return tag + " OK LSUB completed";
            int temp = 0;
            if (mailboxName == "%")
            {
                temp = 1;
                mailboxName = "*";
            }


            string[] dirList = Directory.GetDirectories(dir, mailboxName);
            foreach (string mailboxDir in dirList)
            {
                // nếu mailboxName bằng "%" kiểm tra folder không có subfolder
                if (temp == 1 && Directory.GetDirectories(mailboxDir).Length != 0) continue;
                string mailbox = mailboxDir.Replace(root, "");
                List<MailBox> ListmailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailbox);
                if (ListmailBoxInfo.Count == 0) return tag + " OK LSUB completed";
                MailBox mailBoxInfo = ListmailBoxInfo[0];
                // kiểm tra mailbox đã được subcribe chưa
                if (mailBoxInfo.subscribed == 0) continue;
                // tạo response
                string[] tempArr =
                {
                    (mailBoxInfo.all == 1?"\\All":""),
                    (mailBoxInfo.archive == 1?"\\Archive":""),
                    (mailBoxInfo.drafts == 1?"\\Drafts":""),
                    (mailBoxInfo.flagged == 1?"\\Flagged":""),
                    (mailBoxInfo.haschildren == 1?"\\HasChildren":""),
                    (mailBoxInfo.hasnochildren == 1?"\\HasNoChildren":""),
                    (mailBoxInfo.important == 1?"\\Important":""),
                    (mailBoxInfo.inbox == 1?"\\Inbox":""),
                    (mailBoxInfo.junk == 1?"\\Junk":""),
                    (mailBoxInfo.marked == 1?"\\Marked":""),
                    (mailBoxInfo.nointeriors == 1?"\\NoInferiors":""),
                    (mailBoxInfo.noselect == 1?"\\Noselect":""),
                    (mailBoxInfo.sent == 1?"\\Sent":""),
                    (mailBoxInfo.trash == 1?"\\Trash":""),
                };
                tempArr = tempArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                response += "* LSUB (" + string.Join(' ', tempArr) + $") \"/\" \"{reference}{mailbox}\"\r\n";
            }
            return response + tag + " OK LSUB completed";
        }



        //selected state

        public static string ReturnFetchResponse(string tag, string argument, string userSession, string userMailBox,bool fromUIDCommand=false,bool slient=false) //mới được có 2 cái header và text body thôi nha mấy cha
        {
            string response = "";
            var math = Regex.Match(argument, @"^(\d+|(?:\d+|\*):(?:\d+|\*)) \(([^\(\)]*)\)");
            if (!math.Success) return ReturnParseErrorResponse(tag, "FETCH");
            string mailIndex = math.Groups[1].Value;
            string right;
            string left;
            List<MailInfo> mailInfoList;
            if (mailIndex.Contains(':'))
            {
                string[] temp = mailIndex.Split(':',StringSplitOptions.RemoveEmptyEntries);
                if (temp[0] == "*")
                {
                    if (fromUIDCommand) left = "uid";
                    else left = "rowid";
                }
                else left = temp[0];
                if (temp[1] == "*")
                {
                    if (fromUIDCommand) right = "uid";
                    else right = "rowid";
                }
                else right = temp[1];
            }
            else
            {
                right = mailIndex;
                left = right;
            }
            if (fromUIDCommand) mailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession, userMailBox, left, right);
            else mailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox, left, right);

            string[] items = math.Groups[2].Value.Split(' ');
            string[] arguments = argument.Split(' ');

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            string mailbBoxDir = Environment.CurrentDirectory + $"/ImapMailBox/{userSession}/{userMailBox}/";
            string emailPath;
            if(mailInfoList.Count==0) return tag + " OK FETCH completed";
            foreach (MailInfo mailInfo in mailInfoList)
            {
                emailPath = mailbBoxDir + $"email_{mailInfo.uid}";
                if(File.Exists(emailPath+".eml"))
                {
                    if (File.Exists(emailPath + ".msg")) emailPath += ".msg";
                    else emailPath += ".eml";
                }
                else
                {
                    if (File.Exists(emailPath + ".msg")) emailPath += ".msg";
                    else continue;
                }
                FileInfo email = new FileInfo(emailPath);
                response += "* " + (fromUIDCommand ? mailInfo.uid : mailInfo.numrow) + " FETCH (";
                bool first = true;
                foreach(string item in items)
                {
                    if (!first) response += " ";
                    first = false;
                    switch(item.ToLower())
                    {
                        case "uid":
                            response += $"UID {mailInfo.uid}";
                            break;
                        case "flags":
                            string[] tempArr = {(mailInfo.recent == 1 ? "\\Recent":""),
                                            (mailInfo.answered == 1 ? "\\Answered" : ""),
                                            (mailInfo.flagged == 1 ? "\\Flagged" : ""),
                                            (mailInfo.deleted == 1 ? "\\Deleted" : ""),
                                            (mailInfo.seen == 1 ? "\\Seen" : ""),
                                            (mailInfo.draft == 1 ? "\\Draft" : "")};
                            tempArr = tempArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            response += "FLAGS ("+string.Join(' ',tempArr)+")";
                            break;
                        case "rfc822.size":
                            response += $"RFC822.SIZE {email.Length}";
                            break;
                        case "body.peek[]":
                            response += "BODY[] {"+email.Length+"}\r\n";
                            using (StreamReader sr = new StreamReader(email.OpenRead()))
                            {
                                string temp = sr.ReadToEnd();
                                response += temp;
                            }
                            slient = true;
                            break;
                        case "internaldate":
                            dtDateTime = dtDateTime.AddSeconds(mailInfo.intertime).ToLocalTime();
                            response += "INTERNALDATE \"" + dtDateTime.ToString("dd-MMM-yyyy HH:mm:ss zzz") +"\"";
                            break;
                        default:
                            break;
                    }
                }
                response += $")\r\n";
            }
            int success;
            if(!slient)
            {
                if (fromUIDCommand) success = SqliteQuery.UpdateSeenFlagWithUID(userSession, userMailBox, left, right);
                else success = SqliteQuery.UpdateSeenFlagWithIndex(userSession, userMailBox, left, right);
            }    
            response += tag + " OK FETCH completed";
            return response;
        }
        public static string ReturnSearchResponse(string tag, string argument, string userSession,string userMailBox, bool fromUIDCommand=false)
        {
            var math = Regex.Match(argument, @"(?:[sS][iI][nN][cC][eE])( .*)?");
            if (!math.Success) return ReturnParseErrorResponse(tag, "SEARCH");
            DateTime dateTime = Convert.ToDateTime(math.Groups[1].Value.Trim());
            long unixTime = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
            List<MailInfo> mailInfoList = SqliteQuery.LoadMailInfoSinceInterTime(userSession, userMailBox, unixTime);
            if (mailInfoList.Count == 0) return tag + "OK SEARCH completed\r\n";
            string respone = "* SEARCH";
            if(fromUIDCommand) foreach (MailInfo mailInfo in mailInfoList) respone += $" {mailInfo.uid}";
            else foreach (MailInfo mailInfo in mailInfoList) respone += $" {mailInfo.numrow}";
            respone += "\r\n" + tag + " OK SEARCH completed";
            return respone;
        }
        public static string ReturnCreateResponse(string tag, string argument, string userSession)
        {
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+))");
            if(!math.Success) return Response.ReturnParseErrorResponse(tag, "SELECT");
            string mailboxName = math.Groups[1].Value.Replace("\"", "");
            int success = SqliteQuery.InsertMailBox(userSession, mailboxName);
            if (success == 1)
            {
                if (!Directory.Exists(Environment.CurrentDirectory + $"\\ImapMailBox\\{userSession}\\{mailboxName}"))
                    Directory.CreateDirectory(Environment.CurrentDirectory + $"\\ImapMailBox\\{userSession}\\{mailboxName}");
                return tag + " OK CREATE completed";
            }
            return tag + " NO Mailbox already exists";
        }
        public static string ReturnExpungeResponse(string tag)
        {
            List<int> deletedMail = SqliteQuery.LoadDeletedMail();
            if (deletedMail.Count == 0) return tag + " NO Deleted mail does not exist";
            string response = "";
            int count = 0;
            for(int i = 0; i<deletedMail.Count; ++i)
            {
                int index = deletedMail[i] - count;
                response += "* " + index + " EXPUNGE\r\n";
                count++;
            }
            SqliteQuery.DeleteMail();
            response += tag + " OK EXPUNGE completed\r\n";
            return response;
        }

        public static string ReturnUIDResponse(string tag, string agrument, string userSession, string userMailBox)
        {
            // cho search since
            var math = Regex.Match(agrument, @"^(\S+)(?: (.*))?");
            if (!math.Success) return ReturnParseErrorResponse(tag, "UID");
            string command = math.Groups[1].Value;
            string newArgument = math.Groups[2].Value;
            switch (command.ToLower())
            {
                case "search":
                    return Response.ReturnSearchResponse(tag, newArgument, userSession, userMailBox, true);
                case "fetch":
                    return Response.ReturnFetchResponse(tag, newArgument, userSession, userMailBox, true);
                case "store":
                    return Response.ReturnStoreResponse(tag, newArgument, userSession, userMailBox, true);
                default:
                    return ReturnInvaildCommandResponse(tag);
            }
        }

        private static string ReturnStoreResponse(string tag, string argument, string userSession, string userMailBox, bool fromUIDCommand)
        {
            var math = Regex.Match(argument, @"^(\d+|(?:\d+|\*):(?:\d+|\*)) ([^\s]+) \(([^\(\)]*)\)");
            if (!math.Success) return ReturnParseErrorResponse(tag, "STORE");
            string mailIndex = math.Groups[1].Value;
            string item = math.Groups[2].Value;
            string right;
            string left;
            if (mailIndex.Contains(':'))
            {
                string[] temp = mailIndex.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (temp[0] == "*")
                {
                    if (fromUIDCommand) left = "uid";
                    else left = "rowid";
                }
                else left = temp[0];
                if (temp[1] == "*")
                {
                    if (fromUIDCommand) right = "uid";
                    else right = "rowid";
                }
                else right = temp[1];
            }
            else
            {
                right = mailIndex;
                left = right;
            }
            string newArgument = mailIndex + " (FLAGS)";
            Flags flags = new Flags();
            int success;
            if (math.Groups[3].Value!="")
            {
                string[] flagsArr = math.Groups[3].Value.Split(' ');
                if(!flags.BuildFlagItem(flagsArr)) return ReturnParseErrorResponse(tag, "STORE");
            }

            switch (item.ToLower())
            {
                case "flags":
                    if (flags.seen != "1") flags.seen = "0";
                    if (flags.answered != "1") flags.answered = "0";
                    if (flags.flagged != "1") flags.flagged = "0";
                    if (flags.deleted != "1") flags.deleted = "0";
                    if (flags.draft != "1") flags.draft = "0";
                    if (fromUIDCommand) success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, left, right, flags);
                    else success = SqliteQuery.UpdateFlagsWithIndex(userSession, userMailBox, left, right, flags);
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand);
                case "flags.silent":
                    if (flags.seen != "1") flags.seen = "0";
                    if (flags.answered != "1") flags.answered = "0";
                    if (flags.flagged != "1") flags.flagged = "0";
                    if (flags.deleted != "1") flags.deleted = "0";
                    if (flags.draft != "1") flags.draft = "0";
                    if (fromUIDCommand) success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, left, right, flags);
                    else success = SqliteQuery.UpdateFlagsWithIndex(userSession, userMailBox, left, right, flags);
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand,true);
                case "+flags":
                    if (fromUIDCommand) success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, left, right, flags);
                    else success = SqliteQuery.UpdateFlagsWithIndex(userSession, userMailBox, left, right, flags);
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand);
                case "+flags.slient":
                    if (fromUIDCommand) success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, left, right, flags);
                    else success = SqliteQuery.UpdateFlagsWithIndex(userSession, userMailBox, left, right, flags);
                    return tag + " OK STORE completed";
                case "-flags":
                    if (flags.seen == "1") flags.seen = "0";
                    if (flags.answered == "1") flags.answered = "0";
                    if (flags.flagged == "1") flags.flagged = "0";
                    if (flags.deleted == "1") flags.deleted = "0";
                    if (flags.draft == "1") flags.draft = "0";
                    if (fromUIDCommand) success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, left, right, flags);
                    else success = SqliteQuery.UpdateFlagsWithIndex(userSession, userMailBox, left, right, flags);
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand);
                case "-flags.slient":
                    if (flags.seen == "1") flags.seen = "0";
                    if (flags.answered == "1") flags.answered = "0";
                    if (flags.flagged == "1") flags.flagged = "0";
                    if (flags.deleted == "1") flags.deleted = "0";
                    if (flags.draft == "1") flags.draft = "0";
                    if (fromUIDCommand) success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, left, right, flags);
                    else success = SqliteQuery.UpdateFlagsWithIndex(userSession, userMailBox, left, right, flags);
                    return tag + " OK STORE completed";
                default:
                    return ReturnParseErrorResponse(tag, "STORE");
            }
        }

        private static string StoreFormatReturn(string tag, string argument, string userSession, string userMailBox, bool fromUIDCommand=false, bool silent=false)
        {
            string response = Response.ReturnFetchResponse(tag, argument, userSession, userMailBox, fromUIDCommand, silent);
            var maths = Regex.Matches(response, @"\* [\d]+ FETCH \(FLAGS \(.*\)\)");
            response = "";
            for (int i = 0; i < maths.Count; i++) response += maths[i].Value+"\r\n";
            response += tag + " OK STORE completed";
            return response;
        }

    }
}