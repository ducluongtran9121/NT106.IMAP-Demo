using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClientIMAP
{
    public class CommandBase
    {
        private string Tag { get; set; }

        private CommandType Type { get; set; }

        private List<string> Arguments { get; set; }

        public CommandBase()
        {

        }

        public CommandBase(string Tag, CommandType Type, params string[] Arguments)
        {
            this.Tag = Tag;
            this.Type = Type;
            this.Arguments = new List<string>(Arguments);
        }

        public override string ToString()
        {
            string command = Tag + Type.ToString();

            command += " " + string.Join(' ', Arguments);

            return command;
        }
    }
    public enum CommandType
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
