using MailClient.DataModels.Mail;
using System.Collections.ObjectModel;

namespace MailClient.Helpers
{
    public static class AccountHelper
    {
        public static Account CurrentAccount = new();

        public static ObservableCollection<Account> Accounts = new();

        public static ObservableCollection<string> Names
        {
            get
            {
                ObservableCollection<string> mailboxNames = new();
                foreach (MailBox mailBox in CurrentAccount.MailBoxes)
                {
                    mailboxNames.Add(mailBox.Name);
                }
                return mailboxNames;
            }
        }

        public static ObservableCollection<MailMessage> CurretMailBoxMessages
        {
            get
            {
                if (CurrentAccount.MailBoxes.Count == 0)
                    return null;

                return CurrentAccount.MailBoxes[0].Messages;
            }
        }
    }
}