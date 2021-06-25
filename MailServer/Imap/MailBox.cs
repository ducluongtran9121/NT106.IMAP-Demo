using System;
using System.Collections.Generic;
using System.Text;

namespace MailServer.Imap
{
    internal class MailBox
    {
        public string user { get; set; }
        public string name { get; set; }
        public string uidvalidity { get; set; }
        public string exists { get; set; }
        public string recent { get; set; }
        public string firstunseen { get; set; }
        public string uidnext { get; set; }
        public int all { get; set; }
        public int archive { get; set; }
        public int drafts { get; set; }
        public int flagged { get; set; }
        public int haschildren { get; set; }
        public int hasnochildren { get; set; }
        public int important { get; set; }
        public int inbox { get; set; }
        public int junk { get; set; }
        public int marked { get; set; }
        public int nointeriors { get; set; }
        public int noselect { get; set; }
        public int sent { get; set; }
        public int subscribed { get; set; }
        public int trash { get; set; }

    }
}