using System;
using System.Collections.Generic;

namespace MailClient.IMAP.Commands
{
    public class BaseCommand
    {
        private string Tag { get; set; }

        private CommandType Type { get; set; }

        private string[] Parameters { get; set; }

        public BaseCommand()
        {
            Tag = string.Empty;
            Type = CommandType.NONE;
        }

        public BaseCommand(string Tag, CommandType Type, params string[] Parameters)
        {
            this.Tag = Tag;
            this.Type = Type;
            this.Parameters = Parameters;
        }

        public override string ToString()
        {
            string command = Tag + " " + Type.ToString();

            command += " " + string.Join(' ', Parameters);

            return command;
        }
    }

    public enum CommandType
    {
        NONE,
        LOGIN,
        FETCH,
        STARTTLS,
        SELECT,
        EXAMINE,
        CREATE,
        DELETE,
        RENAME,
        SUBSCRIBE,
        UNSUBSCRIBE,
        LIST,
        LSUB,
        STATUS,
        APPEND,
        CHECK,
        CLOSE,
        EXPURE,
        SEARCH,
        STORE,
        COPY,
        UID,
        LOGOUT
    }
}