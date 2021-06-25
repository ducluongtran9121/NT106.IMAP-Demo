using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace MailServer.Imap
{
    internal static class SqliteQuery
    {
        public static List<MailBox> LoadMailBoxInfo(string userSession, string userMailBox)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailBox>($"select * from MailBox where user = '{userSession}' and name = '{userMailBox}'", new DynamicParameters());
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

        public static int LoadUIDMail(uint index)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<int>($"select uid from MailInfo where uid ='{index}'", new DynamicParameters());
                List<int> list = query.ToList();
                if (list.Count() == 0) return -1;
                return list[0];
            }
        }
        //\"_rowid_\"
        public static List<MailInfo> LoadMailInfo()
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailInfo>($"select * from MailInfo", new DynamicParameters());
                return query.ToList();
            }
        }

        public static List<int> LoadDeletedMail()
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<int>($"select uid from MailInfo where deleted = 1", new DynamicParameters());
                List<int> list = query.ToList();
                return list;
            }
        }    
         public static int InsertMailBox(string userSession, string userMailBox)
         {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"insert into MailBox(user,name,uidvalidity) values ('{userSession}','{userMailBox}',{DateTimeOffset.Now.ToUnixTimeSeconds()})", new DynamicParameters());
                return 1;
            }
            catch(SQLiteException)
            {
                return 0;
            }
            finally
            {
                cnn.Close();
            }
                

         }
        public static int UpdateMailBoxSubcribed(string userSession,string userMailBox,int subscribed)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"update MailBox set subscribed={subscribed} where user = '{userSession}' and name = '{userMailBox}' )", new DynamicParameters());
                return 1;
            }
            catch (SQLiteException)
            {
                return 0;
            }
            finally
            {
                cnn.Close();
            }
        }
        public static void DeleteMail()
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                cnn.Execute($"delete from MailInfo where deleted = 1", new DynamicParameters());
            }
        }

        internal static List<long> LoadUIDSince(string userSession, string userMailBox, long unixTime)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query=cnn.Query<long>($"select uid from MailInfo where user='{userSession}' and mailboxname = '{userMailBox}' and intertime >= {unixTime} order by uid", new DynamicParameters());
                return query.ToList();
            }
            
        }
    }
}