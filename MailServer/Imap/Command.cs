using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MailServer.Imap
{
    internal static class Command
    {
        // syntax error
        static public string ReturnInvaildCommand(string tag)
        {
            return tag + " BAD Invalid command";
        }

        static public string ReturnBadState(string tag, string command)
        {
            return tag + " BAD Bad state for " + command.ToUpper();
        }

        static public string ReturnParseError(string tag, string command)
        {
            return tag + " BAD " + command + " parse error";
        }

        static public string ReturnParseError(string tag)
        {
            return tag + " BAD parse error";
        }

        static public string ReturnMissingTag()
        {
            return "* BAD missing tag";
        }

        // any state
        static public string LogoutCommand(string tag)
        {
            return "* BYE IMAP4rev1 Server logging out\n\r" + tag + " OK LOGOUT completed";
        }

        static public string CapabilityCommand(string tag)
        {
            return "* CAPABILITY IMAP4rev1 AUTH = LOGIN AUTH=PLAIN\n\r" + tag + " OK CAPABILITY completed";
        }

        // not authenticate state
        static public string LoginCommand(string tag, string command, string argument1, string argument2, ref string state)
        {
            if (argument1 == "" || argument2 == "") return Command.ReturnParseError(tag, command);
            else
            {
                FileStream fs = new FileStream(Environment.CurrentDirectory + @"\Data\user_password", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                StreamReader sr = new StreamReader(fs);
                string lineContent = "";
                while ((lineContent = sr.ReadLine()) != null)
                {
                    string[] user_pass = lineContent.Split(' ');
                    if (user_pass[0] == argument1 && user_pass[1] == argument2)
                    {
                        sr.Close();
                        fs.Close();
                        state = "auth";
                        return tag + " OK LOGIN completed";
                    }
                }
                sr.Close();
                fs.Close();
                return tag + " NO LOGIN failed";
            }
        }

        //authenticate state
        //selected state
    }
}