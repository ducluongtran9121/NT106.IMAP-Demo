using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MailServer;
using System.Text.RegularExpressions;

namespace MailServer.Imap
{
    internal class ImapSession
    {
        private string state = "noAuth";
        private string tag = "";
        private string command = "";
        private string agrument;
        private string[] agruments;
        private string respose = "* OK IMAP4rev1 Service Ready";
        private string userSession = "";
        private string userMailBox = "";
        private bool startTLS = false;
        // trả về TLS
        public bool GetStartTLS()
        {
            return this.startTLS;
        }
        // trả về trạng thái của session
        public string GetState()
        {
            return this.state;
        }
        // trả về response ứng với từng lệnh trong session
        public byte[] GetEncrytionResponse(string commandLine)
        {
            string keyAES = "12345678123456781234567812345678";
            string ivAES = "1122334455667788";
            this.respose = GetResposed(commandLine);
            return AESCryptography.EncryptWithAES(this.respose, keyAES, ivAES);
        }

        public string GetDecrytionResponse(byte[] ciphertext)
        {
            string keyAES = "12345678123456781234567812345678";
            string ivAES = "1122334455667788";
            return AESCryptography.DecryptWithAES(ciphertext, keyAES, ivAES);
        }

        public string GetResposed(string commandLine)
        {
            Match math;
            // kiểm tra lệnh rỗng
            if (commandLine == "") return this.respose;
            // tách tag với phần còn lại("tag" "remain")
            math = Regex.Match(commandLine, @"^(\S+) ?(.*)");
            // tag bắt đầu với khoảng trắng (invald tag)
            if (!math.Success) return this.respose = Response.ReturnMissingTagResponse();
            this.tag = math.Groups[1].Value;
            // kiểm tra lệnh null
            if(math.Groups[2].Value=="") return this.respose = Response.ReturnParseErrorResponse(this.tag);
            // tách lệnh với phần còn lại ("command" "remain")
            math = Regex.Match(math.Groups[2].Value, @"^(\S+) ?(.*)");
            // command bắt đầu với khoảng trắng
            if(!math.Success) return this.respose = Response.ReturnParseErrorResponse(this.tag);
            this.command = math.Groups[1].Value;
            this.agrument = math.Groups[2].Value;
            this.agruments = math.Groups[2].Value.Split();
            // xử lý lệnh
            ProcessCommand();
            return this.respose;
        }


        public void ResetSession()
        {
            this.tag = "";
            this.command = "";
            this.agruments = null;
            this.state = "";
            this.respose = "";
        }

        //xử lý theo trạng thái
        private void ProcessCommand()
        {
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
                    this.respose = Response.ReturnNoopCommand(this.tag);
                    break;

                case "logout":
                    this.respose = Response.ReturnLogoutResponse(this.tag,ref this.state);
                    break;

                case "starttls":
                    break;

                case "authenticate":
                    break;

                case "login":
                    this.respose = Response.ReturnLoginResponse(this.tag, this.agrument, ref this.state, ref this.userSession);
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
                    this.respose = Response.ReturnNoopCommand(this.tag);
                    break;

                case "logout":
                    this.respose = Response.ReturnLogoutResponse(this.tag,ref this.state);
                    break;

                case "starttls":
                case "authenticate":
                case "login":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                case "select":
                    this.respose = Response.ReturnSelectedResponse(this.tag, this.agrument, ref this.state, this.userSession, ref this.userMailBox);
                    break;

                case "examine":
                    break;

                case "create":
                    this.respose = Response.ReturnCreateResponse(this.tag, this.agrument, this.userSession);
                    break;

                case "delete":
                    break;

                case "subscribe":
                    this.respose = Response.ReturnSubcribeResponse(this.tag, this.agrument, this.userSession);
                    break;

                case "unsubscribe":
                    this.respose = Response.ReturnUnsubcribeResponse(this.tag, this.agrument, this.userSession);
                    break;

                case "list":
                    this.respose = Response.ReturnListResponse(this.tag, this.agrument, this.userSession);
                    break;

                case "lsub":
                    this.respose = Response.ReturnLsubResponse(this.tag, this.agrument, this.userSession);
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
                    this.respose = Response.ReturnNoopCommand(this.tag);
                    break;

                case "logout":
                    this.respose = Response.ReturnLogoutResponse(this.tag, ref this.state);
                    break;

                case "starttls":
                case "authenticate":
                case "login":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                case "select":
                    this.respose = Response.ReturnSelectedResponse(this.tag, this.agrument, ref this.state, this.userSession, ref this.userMailBox);
                    break;

                case "examine":
                    break;

                case "create":
                    break;

                case "delete":
                    break;

                case "subscribe":
                    this.respose = Response.ReturnSubcribeResponse(this.tag, this.agrument, this.userSession);
                    break;

                case "unsubscribe":
                    this.respose = Response.ReturnUnsubcribeResponse(this.tag, this.agrument, this.userSession);
                    break;

                case "list":
                    break;

                case "lsub":
                    this.respose = Response.ReturnLsubResponse(this.tag, this.agrument, this.userSession);
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
                    this.respose = Response.ReturnExpungeResponse(this.tag);
                    break;
                case "search":
                    
                    break;

                case "fetch":
                    this.respose = Response.ReturnFetchResponse(this.tag, this.agrument, this.userSession, this.userMailBox);
                    break;

                case "store":
                    break;

                case "copy":
                    break;

                case "uid":
                    this.respose = Response.ReturnUIDCommand(this.tag, this.agrument, this.userSession, this.userMailBox);
                    break;

                default:
                    this.respose = Response.ReturnInvaildCommandResponse(this.tag);
                    break;
            }
        }

    }
}