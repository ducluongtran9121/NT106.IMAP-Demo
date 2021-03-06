using System;
using System.Collections.Generic;
using System.Text;

namespace MailServer.Imap
{
    internal class MailInfo
    {
        public long numrow { get; set; }
        public string user { get; set; }
        public string mailboxname { get; set; }
        public long uid { get; set; }
        public int recent { get; set; }
        public int seen { get; set; }
        public int answered { get; set; }
        public int flagged { get; set; }
        public int draft { get; set; }
        public int deleted { get; set; }
        public long intertime { get; set; }
    }
}