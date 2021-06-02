using System;

namespace MailClientIMAP
{
    public static class Command
    {
        public static string Login(int tagCount, string username, string password)
        {
            string Tag = string.Format("a{0:0000} ", tagCount);
            return new CommandBase(Tag, CommandType.LOGIN, username, password).ToString();
        }

        public static string SelectMailBox(int tagCount, string MailBox)
        {
            string Tag = string.Format("a{0:0000} ", tagCount);
            return new CommandBase(Tag, CommandType.SELECT, MailBox).ToString();
        }

        public static string FetchMailHeader(int tagCount, int Sequence)
        {
            string Tag = string.Format("a{0:0000} ", tagCount);
            return new CommandBase(Tag, CommandType.FETCH, Sequence.ToString(), "body[header]").ToString();
        }

        public static string FetchMailText(int tagCount, int Sequence)
        {
            string Tag = string.Format("a{0:0000} ", tagCount);
            return new CommandBase(Tag, CommandType.FETCH, Sequence.ToString(), "body[text]").ToString();
        }
    }
}
