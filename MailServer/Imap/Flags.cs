using System;
using System.Collections.Generic;
using System.Text;

namespace MailServer.Imap
{
    class Flags
    {
        public string recent = "recent";
        public string seen = "seen";
        public string answered = "answered";
        public string flagged = "flagged";
        public string draft = "draft";
        public string deleted = "deleted";

        public bool BuildFlagItem(string[] flags)
        {
            foreach(string item in flags)
            {
                switch(item.ToLower())
                {
                    case "\\seen":
                        this.seen = "1";
                        break;
                    case "\\answered":
                        this.answered = "1";
                        break;
                    case "\\flagged":
                        this.flagged = "1";
                        break;
                    case "\\deleted":
                        this.deleted = "1";
                        break;
                    case "\\draft":
                        this.draft = "1";
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }
    }
}
