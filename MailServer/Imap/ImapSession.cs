using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MailServer.Imap
{
    internal class ImapSession
    {
        private string state = "noAuth";
        private string tag = "";
        private string command = "";
        private string argument1 = "";
        private string argument2 = "";
        private string respose = "* OK IMAP4rev1 Service Ready";
        private string userSession = "";
        private string userMailBox = "";

        // trả về response ứng với từng lệnh trong session
        public string GetResposed()
        {
            return this.respose;
        }

        public string GetResposed(string commandLine)
        {
            string[] commands = commandLine.Split();
            return GetResposed(commands);
        }

        public string GetResposed(string[] commands)
        {
            if (commands.Length == 1)
            {
                this.tag = commands[0];
                this.command = "";
                this.argument1 = "";
                this.argument2 = "";
                ProcessCommand();
                return this.respose;
            }
            if (commands.Length == 2) return GetResposed(commands[0], commands[1]);
            if (commands.Length == 3) return GetResposed(commands[0], commands[1], commands[2]);
            return GetResposed(commands[0], commands[1], commands[2], commands[3]);
        }

        public string GetResposed(string tag, string command)
        {
            this.tag = tag;
            this.command = command;
            this.argument1 = "";
            this.argument2 = "";
            ProcessCommand();
            return this.respose;
        }

        public string GetResposed(string tag, string command, string argument1)
        {
            this.tag = tag;
            this.command = command;
            this.argument1 = argument1;
            this.argument2 = "";
            ProcessCommand();
            return this.respose;
        }

        public string GetResposed(string tag, string command, string argument1, string argument2)
        {
            this.tag = tag;
            this.command = command;
            this.argument1 = argument1;
            this.argument2 = argument2;
            ProcessCommand();
            return this.respose;
        }

        public void ResetSession()
        {
            this.tag = "";
            this.command = "";
            this.argument1 = "";
            this.argument2 = "";
            this.state = "";
            this.respose = "";
        }

        //xử lý theo trạng thái
        private void ProcessCommand()
        {
            if (this.tag == "")
            {
                this.respose = Response.ReturnMissingTagResponse();
                return;
            }
            if (this.command == "")
            {
                this.respose = Response.ReturnParseErrorResponse(this.tag);
                return;
            }
            switch (this.state)
            {
                case "noAuth":
                    ProcessNotAuthenticatedState();
                    break;

                case "auth":
                    ProcessAuthenticatedState();
                    break;

                case "selected":
                    ProcessSelectedState();
                    break;

                default:
                    break;
            }
        }

        // trạng thái chưa xác thực
        // các lệnh được dùng: capability,noop,logout,starttls,authenticate,login
        private void ProcessNotAuthenticatedState()
        {
            switch (this.command.ToLower())
            {
                case "capability":
                    this.respose = Response.ReturnCapabilityResponse(this.tag);
                    break;

                case "noop":
                    NoopCommand();
                    break;

                case "logout":
                    this.respose = Response.ReturnLogoutResponse(this.tag);
                    break;

                case "starttls":
                    break;

                case "authenticate":
                    break;

                case "login":
                    this.respose = Response.ReturnLoginResponse(this.tag, this.command, this.argument1, this.argument2, ref this.state, ref this.userSession);
                    break;

                case "select":
                case "examine":
                case "create":
                case "delete":
                case "subscribe":
                case "unsubscribe":
                case "list":
                case "lsub":
                case "status":
                case "append":
                case "check":
                case "close":
                case "expunge":
                case "search":
                case "fetch":
                case "store":
                case "copy":
                case "uid":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                default:
                    this.respose = Response.ReturnInvaildCommandResponse(this.tag);
                    break;
            }
        }

        // trạng thái đã xác thực
        // các lệnh được dùng: capability,noop,logout,select,examine,create,delete,subscribe,unsubscribe,list,lsub,status,append
        private void ProcessAuthenticatedState()
        {
            switch (this.command.ToLower())
            {
                case "capability":
                    this.respose = Response.ReturnCapabilityResponse(this.tag);
                    break;

                case "noop":
                    NoopCommand();
                    break;

                case "logout":
                    this.respose = Response.ReturnLogoutResponse(this.tag);
                    break;

                case "starttls":
                case "authenticate":
                case "login":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                case "select":
                    this.respose = Response.ReturnSelectedResponse(this.tag, this.argument1, ref this.state, this.userSession, ref this.userMailBox);
                    break;

                case "examine":
                    break;

                case "create":
                    break;

                case "delete":
                    break;

                case "subscribe":
                    break;

                case "unsubscribe":
                    break;

                case "list":
                    break;

                case "lsub":
                    break;

                case "status":
                    break;

                case "append":
                    break;

                case "check":
                case "close":
                case "expunge":
                case "search":
                case "fetch":
                case "store":
                case "copy":
                case "uid":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                default:
                    this.respose = Response.ReturnInvaildCommandResponse(this.tag);
                    break;
            }
        }

        // trạng thái đã chọn hộp thư
        // các lệnh được dùng: capability,noop,logout,select,examine,create,delete,subscribe,unsubscribe,list,lsub,status,append,expunge,search,fetch,store,copy,uid
        private void ProcessSelectedState()
        {
            switch (this.command.ToLower())
            {
                case "capability":
                    this.respose = Response.ReturnCapabilityResponse(this.tag);
                    break;

                case "noop":
                    NoopCommand();
                    break;

                case "logout":
                    this.respose = Response.ReturnLogoutResponse(this.tag);
                    break;

                case "starttls":
                case "authenticate":
                case "login":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                case "select":
                    this.respose = Response.ReturnSelectedResponse(this.tag, this.argument1, ref this.state, this.userSession, ref this.userMailBox);
                    break;

                case "examine":
                    break;

                case "create":
                    break;

                case "delete":
                    break;

                case "subscribe":
                    break;

                case "unsubscribe":
                    break;

                case "list":
                    break;

                case "lsub":
                    break;

                case "status":
                    break;

                case "append":
                    break;

                case "check":
                    break;

                case "close":
                    break;

                case "expunge":
                    break;

                case "search":
                    break;

                case "fetch":
                    this.respose = Response.ReturnFetchResponse(this.tag, this.argument1, this.argument2, this.userSession, this.userMailBox);
                    break;

                case "store":
                    break;

                case "copy":
                    break;

                case "uid":
                    break;

                default:
                    this.respose = Response.ReturnInvaildCommandResponse(this.tag);
                    break;
            }
        }

        private void NoopCommand()
        {
            throw new NotImplementedException();
        }
    }
}