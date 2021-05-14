using System;
using System.Collections.Generic;
using System.Text;

namespace MailServer.Imap
{
    internal class MailInfo
    {
        public string user { get; set; }
        public string mailbox_uid { get; set; }
        public string uid { get; set; }
        public string recent { get; set; }
        public string seen { get; set; }
        public string answered { get; set; }
        public string flagged { get; set; }
        public string draft { get; set; }
    }
}