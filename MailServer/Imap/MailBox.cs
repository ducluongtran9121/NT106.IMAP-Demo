using System;
using System.Collections.Generic;
using System.Text;

namespace MailServer.Imap
{
    internal class MailBox
    {
        public string user { get; set; }
        public string uidvalidity { get; set; }
        public string name { get; set; }
        public string exists { get; set; }
        public string recent { get; set; }
        public string firstunseen { get; set; }
        public string uidnext { get; set; }
    }
}