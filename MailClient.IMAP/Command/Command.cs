using System;

namespace MailClient.IMAP.Commands
{
    public static class Command
    {
        public static string Login(int tagCount, string username, string password) =>
            new BaseCommand(string.Format("a{0:0000}", tagCount), CommandType.LOGIN, username, password).ToString();

        public static string SelectMailBox(int tagCount, string MailBox) =>
            new BaseCommand(string.Format("a{0:0000}", tagCount), CommandType.SELECT, MailBox).ToString();

        public static string FetchMailHeader(int tagCount, int Sequence) =>
            new BaseCommand(string.Format("a{0:0000}", tagCount), CommandType.FETCH, Sequence.ToString(), "body[header]").ToString();

        public static string FetchMailText(int tagCount, int Sequence) =>
            new BaseCommand(string.Format("a{0:0000}", tagCount), CommandType.FETCH, Sequence.ToString(), "body[text]").ToString();

        public static string Logout(int tagCount) =>
            new BaseCommand(string.Format("a{0:00000}", tagCount), CommandType.LOGOUT).ToString();
    }
}