using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace MailServer.Imap
{
    internal static class SqliteData
    {
        public static List<MailBox> LoadMailBoxInfo(string userSession, string userMailBox)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailBox>($"select * from MailBox where user = '{userSession}' and name = '{userMailBox.ToLower()}'", new DynamicParameters());
                return query.ToList();
            }
        }

        public static List<User> LoadUserInfo(string userSession, string password)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<User>($"select * from User where name = '{userSession}' and password = '{password}'", new DynamicParameters());
                return query.ToList();
            }
        }

        public static List<string> LoadUIDMail(uint index)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<string>($"select uid from MailInfo where \"_rowid_\"='{index}'", new DynamicParameters());
                return query.ToList();
            }
        }

        public static List<MailInfo> LoadMailInfo()
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailInfo>($"select * from MailInfo", new DynamicParameters());
                return query.ToList();
            }
        }
    }
}