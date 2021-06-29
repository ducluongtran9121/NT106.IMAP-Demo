using MailClient.Imap.Enums;
using MailClient.Imap.Crypto;
using System;
using System.Text;

namespace MailClient.Imap.Commands
{
    public static class Command
    {
        private static string GetString(int tagCount, CommandType type, params string[] parameters)
        {
            return string.Format("a{0:0000}", tagCount) + " " + type.ToString() + " " + string.Join(" ", parameters) + "\r\n";
        }

        public static string Authenticate(int tagCount, string username, string password) =>
            GetString(tagCount, CommandType.LOGIN, username, password);

        public static string SelectFolder(int tagCount, string folderName) =>
            GetString(tagCount, CommandType.SELECT, $"\"{folderName}\"");

        public static string Search(int tagCount, string condition, bool useUid) =>
            $"{string.Format("a{0:0000}", tagCount)}{(useUid ? " UID " : " ")}{CommandType.SEARCH} {condition}\r\n";

        public static string XList(int tagCount, string reference, string mailboxName) =>
            GetString(tagCount, CommandType.XLIST, $"\"{reference}\"", $"\"{mailboxName}\"");

        public static string Noop(int tagCount) =>
            GetString(tagCount, CommandType.NOOP);

        public static string Fetch(int tagCount, string uidRange, bool useUid, params string[] items) =>
            $"{string.Format("a{0:0000}", tagCount)}{(useUid ? " UID " : " ")}{CommandType.FETCH} {uidRange} ({string.Join(" ", items)})\r\n";

        public static string Delete(int tagCount, string folderName) =>
            GetString(tagCount, CommandType.DELETE, folderName);

        public static string Store(int tagCount, string range, bool useUid, bool isAdd, params MessageFlag[] flags)
        {
            string command = $"{string.Format("a{0:0000}", tagCount)}{(useUid ? " UID " : " ")}{CommandType.STORE} {range}{(isAdd ? " +" : " -")}FLAGS (";
            foreach (MessageFlag flag in flags)
                command += $"\\{flag} ";

            return command.Remove(command.Length - 1) + ")\r\n";
        }

        public static string StartTLS(int tagCount)
        {
            return $"{string.Format("a{0:0000}", tagCount)} STARTTLS\r\n";
        }

        public static string Logout(int tagCount) =>
            GetString(tagCount, CommandType.LOGOUT);
    }
}