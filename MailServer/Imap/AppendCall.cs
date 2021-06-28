using System;
using System.Collections.Generic;
using System.Text;

namespace MailServer.Imap
{
    class AppendCall
    {
        public bool isCall = false;
        public MailInfo mailInfo { get; set; }
        public string message = "";
        public int size { get; set; }
        public string tag { get; set; }
        public void reset()
        {
            this.isCall = false;
            this.message = "";
            this.mailInfo = null;
            this.size = 0;
            this.tag = "";
        }
    }
}
