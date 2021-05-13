﻿using System;
using System.Net;

namespace EmeowIMAP.Client
{
    public class ImapSession
    {
        public string CurrentTag { get; set; }

        public int TagCount { get; set; }

        public ImapSession()
        {
            CurrentTag = string.Empty;
            TagCount = 0;
        }

        public string InitiallizeSession()
        {
            CurrentTag = string.Format("a{0:0000} ", ++TagCount);
            Command command = new Command(CurrentTag, TypeCommand.LOGIN, "19522256@thi123.com", "Thi123");
            return command.GetString();
        }

        public string SelectMailBox(string MailBox)
        {
            CurrentTag = string.Format("a{0:0000} ", ++TagCount);
            Command command = new Command(CurrentTag, TypeCommand.SELECT, MailBox);
            return command.GetString();
        }

        public string GetMailHeader(int Sequence)
        {
            CurrentTag = string.Format("a{0:0000} ", ++TagCount);
            Command command = new Command(CurrentTag, TypeCommand.FETCH, Sequence.ToString(), "body[header]");
            return command.GetString();
        }

        public string GetMailText(int Sequence)
        {
            CurrentTag = string.Format("a{0:0000} ", ++TagCount);
            Command command = new Command(CurrentTag, TypeCommand.FETCH, Sequence.ToString(), "body[text]");
            return command.GetString();
        }
    }
}