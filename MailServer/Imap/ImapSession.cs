﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MailServer;

namespace MailServer.Imap
{
    internal class ImapSession
    {
        private string state = "noAuth";
        private string tag = "";
        private string command = "";
        private string[] agruments;
        private string respose = "* OK IMAP4rev1 Service Ready";
        private string userSession = "";
        private string userMailBox = "";

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
            if (commandLine == "") return this.respose;
            string[] commands = commandLine.Split();
            if (commands.Length == 1)
            {
                this.tag = commands[0];
                this.command = "";
                this.agruments = null;
                ProcessCommand();
                return this.respose;
            }
            if (commands.Length == 2)
            {
                this.tag = commands[0];
                this.command = commands[1];
                this.agruments = null;
                ProcessCommand();
                return this.respose;
            }
            return GetResposed(commands);
        }

        public string GetResposed(string[] commands)
        {
            this.tag = commands[0];
            this.command = commands[1];
            agruments = new string[commands.Length - 2];
            Array.Copy(commands, 2, agruments, 0, commands.Length - 2);
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
                    this.respose = Response.ReturnLogoutResponse(this.tag,ref this.state);
                    break;

                case "starttls":
                    break;

                case "authenticate":
                    break;

                case "login":
                    this.respose = Response.ReturnLoginResponse(this.tag, this.agruments, ref this.state, ref this.userSession);
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
                    this.respose = Response.ReturnLogoutResponse(this.tag,ref this.state);
                    break;

                case "starttls":
                case "authenticate":
                case "login":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                case "select":
                    this.respose = Response.ReturnSelectedResponse(this.tag, this.agruments, ref this.state, this.userSession, ref this.userMailBox);
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
                    this.respose = Response.ReturnLogoutResponse(this.tag, ref this.state);
                    break;

                case "starttls":
                case "authenticate":
                case "login":
                    this.respose = Response.ReturnBadStateResponse(this.tag, this.command);
                    break;

                case "select":
                    this.respose = Response.ReturnSelectedResponse(this.tag, this.agruments, ref this.state, this.userSession, ref this.userMailBox);
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
                    this.respose = Response.ReturnExpungeResponse(this.tag);
                    break;
                case "search":
                    break;

                case "fetch":
                    this.respose = Response.ReturnFetchResponse(this.tag, this.agruments, this.userSession, this.userMailBox);
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