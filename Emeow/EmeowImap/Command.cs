using System;
using System.Collections.Generic;

namespace EmeowIMAP
{
    public class Command
    {
        private string Tag { get; set; }

        private TypeCommand Type { get; set; }

        private List<string> Arguments { get; set; }

        public Command()
        {

        }

        public Command(string Tag, TypeCommand Type, params string[] Arguments)
        {
            this.Tag = Tag;
            this.Type = Type;
            this.Arguments = new List<string>(Arguments);
        }

        public string GetString()
        {
            string command = Tag + Type.ToString();

            command += " " + string.Join(' ', Arguments);

            return command;
        }
    }

    public enum TypeCommand
    {
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
    }
}