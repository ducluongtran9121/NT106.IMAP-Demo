using MailClient.Imap.Commands;
using MailClient.Imap.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailClient.Imap
{
    public class Folder
    {
        public string Name { get; private set; }

        public int Exists { get; private set; }

        public int Recent { get; private set; }

        public long UidValidity { get; private set; }

        public long UidNext { get; private set; }

        public FolderAccess Access { get; private set; }

        public MessageFlag[] Flags { get; private set; }

        public MessageFlag[] PermanentFlags { get; set; }

        private CoreClient CoreClient { get; set; }

        public List<int> Uids { get; private set; }

        internal Folder(string name, int exists, int recent, long uidValidity, int uidNext, FolderAccess access, MessageFlag[] flags, MessageFlag[] permanentflags, CoreClient client)
        {
            Name = name;
            Exists = exists;
            Recent = recent;
            UidValidity = uidValidity;
            UidNext = uidNext;
            Access = access;
            Flags = (MessageFlag[])flags.Clone();
            PermanentFlags = (MessageFlag[])permanentflags.Clone();
            CoreClient = client;
        }

        internal Folder(int exists, int recent, long uidValidity, int uidNext, FolderAccess access, MessageFlag[] flags, MessageFlag[] permanentflags)
        {
            Exists = exists;
            Recent = recent;
            UidValidity = uidValidity;
            UidNext = uidNext;
            Access = access;
            Flags = (MessageFlag[])flags.Clone();
            PermanentFlags = (MessageFlag[])permanentflags.Clone();
        }

        public Folder(string name)
        {
            Name = name;
        }

        internal Folder(string name, CoreClient client)
        {
            Name = name;
            CoreClient = client;
        }

        public async Task OpenAsync()
        {
            if (CoreClient == null)
                throw new NullReferenceException("CoreClient not set to an instance of an object");

            if (!CoreClient.IsConnected)
                throw new ConnectionException("CoreClient is not connected");

            if (!await CoreClient.WriteDataAsync(Command.SelectFolder(++CoreClient.Tag, Name)))
                throw new WriteDataException();

            byte[] data = await CoreClient.ReadDataAsync();

            if (data == null) throw new ReadDataException();

            Folder folder = FragmentCommand.SelectFolder(data);

            Exists = folder.Exists;
            Recent = folder.Recent;
            UidValidity = folder.UidValidity;
            UidNext = folder.UidNext;
            Flags = folder.Flags;
            PermanentFlags = folder.PermanentFlags;

            Uids = await GetMessagesUid();
        }

        public async Task<List<int>> GetMessagesUid()
        {
            if (CoreClient == null)
                throw new NullReferenceException("CoreClient not set to an instance of an object");

            if (!CoreClient.IsConnected)
                throw new ConnectionException("CoreClient is not connected");

            if (!await CoreClient.WriteDataAsync(Command.Search(++CoreClient.Tag, "SINCE 01-Jan-1970", useUid: true)))
                throw new WriteDataException();

            byte[] data = await CoreClient.ReadDataAsync();

            if (data == null) throw new ReadDataException();

            return FragmentCommand.Search(data);
        }

        public async Task<Message> GetMessageAsync(int index)
        {
            if (CoreClient == null)
                throw new NullReferenceException("CoreClient not set to an instance of an object");

            if (!CoreClient.IsConnected)
                throw new ConnectionException("CoreClient is not connected");

            if (!await CoreClient.WriteDataAsync(Command.Fetch(++CoreClient.Tag, Uids[index].ToString(), useUid: true, "UID", "FLAGS", "RFC822.SIZE", "INTERNALDATE", "BODY.PEEK[]")))
                throw new WriteDataException();

            byte[] data = await CoreClient.ReadDataAsync();

            if (data == null) throw new ReadDataException();

            return FragmentCommand.FetchMessage(data);
        }

        public async Task<Message[]> GetMessagesAsync()
        {
            List<Message> messages = new();

            for (int i = 0; i < Exists; i++)
                messages.Add(await GetMessageAsync(i));

            return messages.ToArray();
        }
    }
}