using System;
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
            return "* CAPABILITY IMAP4rev1 AUTH=LOGIN AUTH=PLAIN STARTTLS UNSELECT UIDPLUS XLIST\r\n" + tag + " OK CAPABILITY completed";
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
            List<UserInfo> userInfo = SqliteQuery.LoadUserInfo(username.Split('@')[0],password);
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
            List<MailBoxInfo> mailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailbox);
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
            if (mailBoxInfo[0].recent!=0) success = SqliteQuery.UpdateRecentFlag(userSession, userMailBox); 
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
        public static string ReturnListResponse(string tag,string command, string argument, string userSession)
        {
            string response = "";
            string root = Environment.CurrentDirectory + $"\\ImapMailBox\\{userSession}\\";
            var math = Regex.Match(argument, "^(\"(?:[^\"]*)\"|(?:[^\\s]+)) (\"(?:[^\"]*)\"|(?:[^\\s]+))");
            if(!math.Success) return Response.ReturnParseErrorResponse(tag, command);
            // lấy reference từ group
            string reference = math.Groups[1].Value.Replace("\"","");
            string dir=root;
            if (Regex.IsMatch(reference, @"^(./)?.*")) dir +=reference.Replace("./", "");
            // lấy mailboxName từ group
            string mailboxName = math.Groups[2].Value.Replace("\"","");
            // reference = root và mailboxName =""
            if (dir == root && mailboxName=="") return $"* {command} (\\Noselect ) \"/\" \"\"\r\n" + tag + $" OK {command} completed";
            // kiểm tra đường dẫn
            // kiểm tra mailboxName bằng "*","%"
            if (!Directory.Exists(dir+mailboxName) && mailboxName != "*" && mailboxName != "%") return tag+ $" OK {command} completed";
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
                List<MailBoxInfo> ListmailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailbox);
                if(ListmailBoxInfo.Count==0) return tag + $" OK {command} completed";
                MailBoxInfo mailBoxInfo = ListmailBoxInfo[0];
                string[] tempArr =
                {
                    (mailBoxInfo.all == 1?"\\All":""),
                    (mailBoxInfo.archive == 1?"\\Archive":""),
                    (mailBoxInfo.drafts == 1?"\\Drafts":""),
                    (mailBoxInfo.flagged == 1?"\\Flagged":""),
                    (mailBoxInfo.inbox == 1?"\\Inbox":""),
                    (mailBoxInfo.junk == 1?"\\Junk":""),
                    (mailBoxInfo.marked == 1 && command!="XLIST"?"\\Marked":""),
                    (mailBoxInfo.marked == 0 && command!="XLIST"?"\\UnMarked":""),
                    (mailBoxInfo.nointeriors == 1 && command!="XLIST"?"\\NoInferiors":""),
                    (mailBoxInfo.noselect == 1 && command!="XLIST"?"\\Noselect":""),
                    (mailBoxInfo.sent == 1?"\\Sent":""),
                    (mailBoxInfo.trash == 1?"\\Trash":"")
                };
                tempArr = tempArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                response += $"* {command} (" + string.Join(' ', tempArr) + $") \"/\" \"{reference}{mailbox}\"\r\n";
            }
            return response + tag + $" OK {command} completed";
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
                List<MailBoxInfo> ListmailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailbox);
                if (ListmailBoxInfo.Count == 0) return tag + " OK LSUB completed";
                MailBoxInfo mailBoxInfo = ListmailBoxInfo[0];
                // kiểm tra mailbox đã được subcribe chưa
                if (mailBoxInfo.subscribed == 0) continue;
                // tạo response
                string[] tempArr =
                {
                    (mailBoxInfo.all == 1?"\\All":""),
                    (mailBoxInfo.archive == 1?"\\Archive":""),
                    (mailBoxInfo.drafts == 1?"\\Drafts":""),
                    (mailBoxInfo.flagged == 1?"\\Flagged":""),
                    (mailBoxInfo.inbox == 1?"\\Inbox":""),
                    (mailBoxInfo.junk == 1?"\\Junk":""),
                    (mailBoxInfo.marked == 1?"\\Marked":"UnMarked"),
                    (mailBoxInfo.nointeriors == 1?"\\NoInferiors":""),
                    (mailBoxInfo.noselect == 1?"\\Noselect":""),
                    (mailBoxInfo.sent == 1?"\\Sent":""),
                    (mailBoxInfo.trash == 1?"\\Trash":"")
                };
                tempArr = tempArr.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                response += "* LSUB (" + string.Join(' ', tempArr) + $") \"/\" \"{reference}{mailbox}\"\r\n";
            }
            return response + tag + " OK LSUB completed";
        }
        //selected state

        public static string ReturnFetchResponse(string tag, string argument, string userSession, string userMailBox, bool fromUIDCommand = false, bool slient = false) //mới được có 2 cái header và text body thôi nha mấy cha
        {
            string response = "";
            var math = Regex.Match(argument, @"^((?:(?:(?:[1-9]+|\*):(?:[1-9]+|\*)|[1-9]+),)*(?:(?:[1-9]+|\*):(?:[1-9]+|\*)|[1-9]+)) \(([^\(\)]*)\)");
            if (!math.Success) return ReturnParseErrorResponse(tag, "FETCH");
            List<MailInfo> tempMailInfoList;
            List<MailInfo> mailInfoList = new List<MailInfo>();
            string right="";
            string left="";
            string[] mailIndexArr = math.Groups[1].Value.Split(',');
            foreach(string mailIndex in mailIndexArr)
            {
                if (mailIndex.Contains(':'))
                {
                    string[] temp = mailIndex.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (temp[0] == "*")
                    {
                        if (fromUIDCommand) left = "uid";
                        else left = "numrow";
                    }
                    else left = temp[0];
                    if (temp[1] == "*")
                    {
                        if (fromUIDCommand) right = "uid";
                        else right = "numrow";
                    }
                    else right = temp[1];
                    if (fromUIDCommand) tempMailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession, userMailBox, left, right);
                    else tempMailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox, left, right);
                }
                else
                {
                    if (fromUIDCommand) tempMailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession,userMailBox,mailIndex);
                    else tempMailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox,mailIndex);
                }
                foreach(MailInfo tempMail in tempMailInfoList)
                {
                    if (!mailInfoList.Contains(tempMail)) mailInfoList.Add(tempMail);
                }    
            }
            mailInfoList.Sort((x, y) => x.uid.CompareTo(y.uid));
            
            

            string[] items = math.Groups[2].Value.Split(' ');
            string[] arguments = argument.Split(' ');

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            string mailbBoxDir = Environment.CurrentDirectory + $"/ImapMailBox/{userSession}/{userMailBox}/";
            string emailPath;
            if (mailInfoList.Count == 0) return tag + " OK FETCH completed";
            foreach (MailInfo mailInfo in mailInfoList)
            {
                emailPath = mailbBoxDir + $"email_{mailInfo.uid}";
                if (File.Exists(emailPath + ".eml"))
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
                foreach (string item in items)
                {
                    if (!first) response += " ";
                    first = false;
                    switch (item.ToLower())
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
                            response += "FLAGS (" + string.Join(' ', tempArr) + ")";
                            break;
                        case "rfc822.size":
                            response += $"RFC822.SIZE {email.Length}";
                            break;
                        case "body.peek[]":
                            response += "BODY[] {" + email.Length + "}\r\n";
                            using (StreamReader sr = new StreamReader(email.OpenRead()))
                            {
                                string temp = sr.ReadToEnd();
                                response += temp;
                            }
                            slient = true;
                            break;
                        case "internaldate":
                            dtDateTime = dtDateTime.AddSeconds(mailInfo.intertime).ToLocalTime();
                            response += "INTERNALDATE \"" + dtDateTime.ToString("dd-MMM-yyyy HH:mm:ss zzz") + "\"";
                            break;
                        default:
                            break;
                    }
                }
                response += $")\r\n";
            }
            int success;
            if (!slient)
            {
                foreach(MailInfo  mailInfo in mailInfoList)
                {
                    if (fromUIDCommand) success = SqliteQuery.UpdateSeenFlagWithUID(userSession, userMailBox, mailInfo.uid.ToString());
                    else success = SqliteQuery.UpdateSeenFlagWithIndex(userSession, userMailBox, mailInfo.numrow.ToString());
                }    
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
        public static string ReturnExpungeResponse(string tag,string userSession,string userMailBox,bool fromUIDCommand=false,string argument = "")
        {
            List<MailInfo> mailInfoList = new List<MailInfo>();
            if (argument!="")
            {
                var math = Regex.Match(argument, @"^((?:(?:(?:[1-9]+|\*):(?:[1-9]+|\*)|[1-9]+),)*(?:(?:[1-9]+|\*):(?:[1-9]+|\*)|[1-9]+))");
                if (!math.Success) return ReturnParseErrorResponse(tag, "FETCH");
                List<MailInfo> tempMailInfoList; 
                string right = "";
                string left = "";
                string[] mailIndexArr = math.Groups[1].Value.Split(',');
                foreach (string mailIndex in mailIndexArr)
                {
                    if (mailIndex.Contains(':'))
                    {
                        string[] temp = mailIndex.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (temp[0] == "*")
                        {
                            if (fromUIDCommand) left = "uid";
                            else left = "numrow";
                        }
                        else left = temp[0];
                        if (temp[1] == "*")
                        {
                            if (fromUIDCommand) right = "uid";
                            else right = "numrow";
                        }
                        else right = temp[1];
                        if (fromUIDCommand) tempMailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession, userMailBox, left, right);
                        else tempMailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox, left, right);
                    }
                    else
                    {
                        if (fromUIDCommand) tempMailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession, userMailBox, mailIndex);
                        else tempMailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox, mailIndex);
                    }
                    foreach (MailInfo tempMail in tempMailInfoList)
                    {
                        if (!mailInfoList.Contains(tempMail) && tempMail.deleted==1) mailInfoList.Add(tempMail);
                    }
                }
                mailInfoList.Sort((x, y) => x.uid.CompareTo(y.uid));
            }
            else mailInfoList = SqliteQuery.LoadDeletedMail(userSession, userMailBox);
            if (mailInfoList.Count == 0) return tag + " OK EXPUNGE completed";
            List<string> TrashMailbox = SqliteQuery.LoadTrashMailBoxName(userSession);
            string response = "";
            int success;
            string soursePath;
            string desPath;
            long baseUID;
            string root = Environment.CurrentDirectory + $"/ImapMailBox/{userSession}";
            if(TrashMailbox.IndexOf(userMailBox)==-1 && TrashMailbox.Count != 0)
            {
                foreach(string mailBox in TrashMailbox)
                {
                    List<MailBoxInfo> mailBoxInfo = SqliteQuery.LoadMailBoxInfo(userSession, mailBox);
                    if (mailBoxInfo.Count == 0) return "OK EXPUNGE completed";
                    baseUID = mailBoxInfo[0].uidnext;
                    foreach (MailInfo mail in mailInfoList)
                    {
                        soursePath = root + $"/{userMailBox}/email_{baseUID}";
                        if (File.Exists(soursePath + ".msg"))
                        {
                            soursePath += ".msg";
                            desPath = root + $"/{mailBox}/email_{baseUID}.msg";
                        }
                        else
                        {
                            if (File.Exists(soursePath + ".eml"))
                            {
                                soursePath += ".eml";
                                desPath = root + $"/{mailBox}/email_{baseUID}.eml";
                            }
                            else return tag + "OK EXPUNGE completed";
                        }
                        File.Copy(soursePath, desPath);
                        File.Delete(soursePath);
                        success = SqliteQuery.DeleteMailWithUID(userSession, userMailBox, mail.uid);
                        mail.uid = baseUID++;
                        mail.mailboxname = mailBox;
                        success = SqliteQuery.InsertMailIntoMailBox(userSession, mailBox, mail);
                        response += $"* {mail.numrow} EXPUNGE\r\n";
                    }    
                }    
            }
            else
            {
                foreach (MailInfo mail in mailInfoList)
                {
                    soursePath = root + $"/{userMailBox}/email_{mail.uid}";
                    if (File.Exists(soursePath + ".msg")) soursePath += ".msg";
                    else
                    {
                        if (File.Exists(soursePath + ".eml")) soursePath += ".eml";
                        else return tag + "OK EXPUNGE completed";
                    }
                    File.Delete(soursePath);
                    success = SqliteQuery.DeleteMailWithUID(userSession, userMailBox, mail.uid);
                    response += $"* {mail.numrow} EXPUNGE\r\n";
                }
            }
            response += tag + " OK EXPUNGE completed";
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
                case "expunge":
                    return Response.ReturnExpungeResponse(tag, userSession, userMailBox, true, newArgument);
                default:
                    return ReturnInvaildCommandResponse(tag);
            }
        }

        public static string ReturnStoreResponse(string tag, string argument, string userSession, string userMailBox, bool fromUIDCommand=false)
        {
            var math = Regex.Match(argument, @"^((?:(?:(?:[1-9]+|\*):(?:[1-9]+|\*)|[1-9]+),)*(?:(?:[1-9]+|\*):(?:[1-9]+|\*)|[1-9]+)) ([^\s]+) \(([^\(\)]*)\)");
            if (!math.Success) return ReturnParseErrorResponse(tag, "STORE");
            string item = math.Groups[2].Value;
            if (!math.Success) return ReturnParseErrorResponse(tag, "FETCH");
            List<MailInfo> tempMailInfoList;
            List<MailInfo> mailInfoList = new List<MailInfo>();
            string right = "";
            string left = "";
            string[] mailIndexArr = math.Groups[1].Value.Split(',');
            foreach (string mailIndex in mailIndexArr)
            {
                if (mailIndex.Contains(':'))
                {
                    string[] temp = mailIndex.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (temp[0] == "*")
                    {
                        if (fromUIDCommand) left = "uid";
                        else left = "numrow";
                    }
                    else left = temp[0];
                    if (temp[1] == "*")
                    {
                        if (fromUIDCommand) right = "uid";
                        else right = "numrow";
                    }
                    else right = temp[1];
                    if (fromUIDCommand) tempMailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession, userMailBox, left, right);
                    else tempMailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox, left, right);
                }
                else
                {
                    if (fromUIDCommand) tempMailInfoList = SqliteQuery.LoadMailInfoWithUID(userSession, userMailBox, mailIndex);
                    else tempMailInfoList = SqliteQuery.LoadMailInfoWithIndex(userSession, userMailBox, mailIndex);
                }
                foreach (MailInfo tempMail in tempMailInfoList)
                {
                    if (!mailInfoList.Contains(tempMail)) mailInfoList.Add(tempMail);
                }
            }
            mailInfoList.Sort((x, y) => x.uid.CompareTo(y.uid));
            string newArgument = math.Groups[1].Value + " (FLAGS)";
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
                    foreach(MailInfo mailInfo in mailInfoList)
                    {
                        success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, mailInfo.uid.ToString(), flags);
                    }
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand);
                case "flags.silent":
                    if (flags.seen != "1") flags.seen = "0";
                    if (flags.answered != "1") flags.answered = "0";
                    if (flags.flagged != "1") flags.flagged = "0";
                    if (flags.deleted != "1") flags.deleted = "0";
                    if (flags.draft != "1") flags.draft = "0";
                    foreach (MailInfo mailInfo in mailInfoList)
                    {
                        success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, mailInfo.uid.ToString(), flags);
                    }
                    return tag + " OK STORE completed";
                case "+flags":
                    foreach (MailInfo mailInfo in mailInfoList)
                    {
                        success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, mailInfo.uid.ToString(), flags);
                    }
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand);
                case "+flags.silent":
                    foreach (MailInfo mailInfo in mailInfoList)
                    {
                        success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, mailInfo.uid.ToString(), flags);
                    }
                    return tag + " OK STORE completed";
                case "-flags":
                    if (flags.seen == "1") flags.seen = "0";
                    if (flags.answered == "1") flags.answered = "0";
                    if (flags.flagged == "1") flags.flagged = "0";
                    if (flags.deleted == "1") flags.deleted = "0";
                    if (flags.draft == "1") flags.draft = "0";
                    foreach (MailInfo mailInfo in mailInfoList)
                    {
                        success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, mailInfo.uid.ToString(), flags);
                    }
                    return Response.StoreFormatReturn(tag, newArgument, userSession, userMailBox, fromUIDCommand);
                case "-flags.silent":
                    if (flags.seen == "1") flags.seen = "0";
                    if (flags.answered == "1") flags.answered = "0";
                    if (flags.flagged == "1") flags.flagged = "0";
                    if (flags.deleted == "1") flags.deleted = "0";
                    if (flags.draft == "1") flags.draft = "0";
                    foreach (MailInfo mailInfo in mailInfoList)
                    {
                        success = SqliteQuery.UpdateFlagsWithUID(userSession, userMailBox, mailInfo.uid.ToString(), flags);
                    }
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

        public static string ReturnAppendResponse(string tag, string argument, string userSession,AppendCall appendCall)
        {
            //var maths = Regex.Match(argument, "");
            var math = Regex.Match(argument, "^(?:((?:\"[\\w\\s]+\"|\\w+) \\((?:\\\\\\w+)*\\) \"[^\"]*\" \\{\\d*\\})|((?:\"[\\w\\s]+\"|\\w+) \\((?:\\\\\\w+)*\\) \\{\\d*\\})|((?:\"[\\w\\s]+\"|\\w+) \\{\\d*\\}))");
            if (!math.Success) return Response.ReturnParseErrorResponse(tag, "APPEND");
            MailInfo mailAppend = new MailInfo();
            Flags flags = new Flags();
            string mailBoxName;
            List<string> strFlag = new List<string>();
            DateTime Date = DateTime.Now;
            int messageSize = 0;
            if(math.Groups[3].Success)
            {
                math = Regex.Match(math.Groups[3].Value, "(?:(\"[\\w\\s]+\"|\\w+) \\{(\\d*)\\})");
                mailBoxName = math.Groups[1].Value.Replace("\"", "");
                if (math.Groups[2].Value != "" && !Int32.TryParse(math.Groups[3].Value, out messageSize)) return Response.ReturnParseErrorResponse(tag, "APPEND");
            }
            else
            {
                if(math.Groups[2].Success)
                {
                    math = Regex.Match(math.Groups[2].Value, "(?:(\"[\\w\\s]+\"|\\w+) \\(((?:\\\\\\w+)*)\\) \\{(\\d*)\\})");
                    mailBoxName = math.Groups[1].Value.Replace("\"", "");
                    if (math.Groups[2].Value != "") strFlag = math.Groups[2].Value.Split(' ').ToList();
                    if (math.Groups[3].Value != "" && !Int32.TryParse(math.Groups[3].Value, out messageSize)) return Response.ReturnParseErrorResponse(tag, "APPEND");
                }
                else
                {
                    math = Regex.Match(math.Groups[1].Value, "(?:(\"[\\w\\s]+\"|\\w+) \\(((?:\\\\\\w+)*)\\) (\"[^\"]*\") \\{(\\d*)\\})");
                    mailBoxName = math.Groups[1].Value.Replace("\"","");
                    if (math.Groups[2].Value != "") strFlag = math.Groups[2].Value.Split(' ').ToList();
                    if (math.Groups[3].Value != "" && DateTime.TryParse(math.Groups[3].Value, out Date)) return Response.ReturnParseErrorResponse(tag, "APPEND");
                    if (math.Groups[4].Value != "" && !Int32.TryParse(math.Groups[4].Value, out messageSize)) return Response.ReturnParseErrorResponse(tag, "APPEND");
                }    
            }
            List<MailBoxInfo> mailBoxInfoList = SqliteQuery.LoadMailBoxInfo(userSession, mailBoxName);
            if (mailBoxInfoList.Count == 0) return tag + " NO Mailbox does not exist";
            if(!flags.BuildFlagItem(strFlag.ToArray())) return Response.ReturnParseErrorResponse(tag, "APPEND");
            mailAppend.user = userSession;
            mailAppend.mailboxname = mailBoxName;
            mailAppend.uid = mailBoxInfoList[0].uidnext;
            mailAppend.recent = 1;
            mailAppend.seen = (flags.seen == "1" ? 1 : 0);
            mailAppend.answered = (flags.answered == "1" ? 1 : 0);
            mailAppend.deleted = (flags.deleted == "1" ? 1 : 0);
            mailAppend.draft = (flags.draft == "1" ? 1 : 0);
            mailAppend.flagged = (flags.flagged == "1" ? 1 : 0);
            mailAppend.intertime = ((DateTimeOffset)Date).ToUnixTimeSeconds();
            appendCall.isCall = true;
            appendCall.mailInfo = mailAppend;
            appendCall.size = messageSize;
            appendCall.tag = tag;
            return "+ Ready for literal data";
        }

        public static string ReturnMessagesAppendResponse(string commandLine, AppendCall appendCall)
        {
            appendCall.message += commandLine + "\r\n";
            if (appendCall.message.Length < appendCall.size) return "";
            if(appendCall.message.Length> appendCall.size)
            {
                appendCall.reset();
                return "";
            }
            File.WriteAllText(Environment.CurrentDirectory + $"/ImapMailBox/{appendCall.mailInfo.user}/{appendCall.mailInfo.mailboxname}/email_{appendCall.mailInfo.uid}.msg",appendCall.message);
            int success = SqliteQuery.InsertMailIntoMailBox(appendCall.mailInfo.user, appendCall.mailInfo.mailboxname, appendCall.mailInfo);
            appendCall.reset();
            return appendCall.tag +" OK APPEND completed";
        }
    }
}