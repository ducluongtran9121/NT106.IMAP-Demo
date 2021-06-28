using MailClient.DataModels.Imap;
using System.Collections.ObjectModel;

namespace MailClient.Helpers
{
    public static class AccountHelper
    {
        public static Account CurrentAccount;

        public static ObservableCollection<Account> Accounts = new();
    }
}