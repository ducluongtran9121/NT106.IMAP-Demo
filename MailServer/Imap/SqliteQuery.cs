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
        public static List<MailBoxInfo> LoadMailBoxInfo(string userSession, string userMailBox)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailBoxInfo>($"select * from MailBoxInfo where user = '{userSession}' and name = '{userMailBox}'", new DynamicParameters());
                return query.ToList();
            }
        }

        public static List<UserInfo> LoadUserInfo(string userSession, string password)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<UserInfo>($"select * from UserInfo where name = '{userSession}' and password = '{password}'", new DynamicParameters());
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

        public static List<string> LoadTrashMailBoxName(string userSession)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<string>($"select name from MailBoxInfo where trash = 1 and user = '{userSession}'", new DynamicParameters());
                List<string> list = query.ToList();
                return list;
            }
        }
        public static int InsertMailIntoMailBox(string userSession,string userMalBox, MailInfo mail)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"insert into MailInfo(user,mailboxname,uid,recent,seen,answered,flagged,draft,deleted) values('{mail.user}','{mail.mailboxname}',{mail.uid},1,{mail.seen},{mail.answered},{mail.flagged},{mail.draft},{mail.deleted})", new DynamicParameters());
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
        public static long LoadFirstUnSeen(string userSession, string userMailBox)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                try
                {
                    var query = cnn.Query<long>($"select min(rowid) from MailInfo where user = '{userSession}' and mailboxname = '{userMailBox}' and seen = 0", new DynamicParameters());
                    return query.ToList()[0];
                }
                catch (DataException)
                {
                    return 0;
                }
            }
        }
      
        public static List<MailInfo> LoadMailInfoWithUID(string userSession, string userMailBox, string left, string right)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailInfo>($"select * from MailInfo where user='{userSession}' and mailboxname = '{userMailBox}' and uid>={left} and uid<={right}", new DynamicParameters());
                return query.ToList();
            }
        }
        public static List<MailInfo> LoadMailInfoWithUID(string userSession, string userMailBox, string uid)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailInfo>($"select * from MailInfo where user='{userSession}' and mailboxname = '{userMailBox}' and uid = {uid}", new DynamicParameters());
                return query.ToList();
            }
        }
        public static List<MailInfo> LoadMailInfoWithIndex(string userSession, string userMailBox, string left, string right)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                string tempVar = $"WITH var AS (SELECT ROW_NUMBER () OVER (ORDER BY MailInfo.uid ) numrow,* FROM MailInfo WHERE MailInfo.user = '{userSession}' and MailInfo.mailboxname = '{userMailBox}')";
                var query = cnn.Query<MailInfo>($"{tempVar} select * from var where numrow>={left} and numrow<={right}", new DynamicParameters());
                return query.ToList();
            }
        }
        public static List<MailInfo> LoadMailInfoWithIndex(string userSession, string userMailBox, string index)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                string tempVar = $"WITH var AS (SELECT ROW_NUMBER () OVER (ORDER BY MailInfo.uid ) numrow,* FROM MailInfo WHERE MailInfo.user = '{userSession}' and MailInfo.mailboxname = '{userMailBox}')";
                var query = cnn.Query<MailInfo>($"{tempVar} select * from var where numrow={index}", new DynamicParameters());
                return query.ToList();
            }
        }
        public static List<MailInfo> LoadMailInfoSinceInterTime(string userSession, string userMailBox, long unixTime)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailInfo>($"select * from MailInfo where user='{userSession}' and mailboxname = '{userMailBox}' and intertime >= {unixTime}", new DynamicParameters());
                return query.ToList();
            }

        }
        public static List<MailInfo> LoadDeletedMail(string userSession, string userMailBox)
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db"))
            {
                var query = cnn.Query<MailInfo>($"SELECT ROW_NUMBER() OVER(ORDER BY uid) numrow,* FROM MailInfo WHERE user = '{userSession}' and mailboxname = '{userMailBox}' and deleted = 1", new DynamicParameters());
                return query.ToList();
            }
            
            
        }
        public static int InsertMailBox(string userSession, string userMailBox)
         {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"insert into MailBoxInfo(user,name) values ('{userSession}','{userMailBox}')", new DynamicParameters());
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
                cnn.Execute($"update MailBoxInfo set subscribed={subscribed} where user = '{userSession}' and name = '{userMailBox}'", new DynamicParameters());
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
        public static int UpdateRecentFlag(string userSession, string userMailBox)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"update MailInfo set recent = 0 where user = '{userSession}' and mailboxname = '{userMailBox}'", new DynamicParameters());
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
        public static int UpdateSeenFlagWithUID(string userSession, string userMailBox, string left, string right)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"update MailInfo set seen = 1 where user = '{userSession}' and mailboxname = '{userMailBox}' and uid>={left} and uid<={right}", new DynamicParameters());
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
        public static int UpdateSeenFlagWithUID(string userSession, string userMailBox,string uid)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"update MailInfo set seen = 1 where user = '{userSession}' and mailboxname = '{userMailBox}' and uid={uid}", new DynamicParameters());
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
        public static int UpdateFlagsWithUID(string userSession, string userMailBox, string left, string right, Flags flags)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"update MailInfo set seen = {flags.seen},answered = {flags.answered},flagged={flags.flagged},draft = {flags.draft},deleted = {flags.deleted} where user = '{userSession}' and mailboxname = '{userMailBox}' and uid>={left} and uid<={right}", new DynamicParameters());
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
        public static int UpdateFlagsWithUID(string userSession, string userMailBox, string uid, Flags flags)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"update MailInfo set seen = {flags.seen},answered = {flags.answered},flagged={flags.flagged},draft = {flags.draft},deleted = {flags.deleted} where user = '{userSession}' and mailboxname = '{userMailBox}' and uid={uid}", new DynamicParameters());
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

        public static int UpdateSeenFlagWithIndex(string userSession, string userMailBox, string left, string right)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                string tempVar1 = $"WITH var AS (SELECT ROW_NUMBER () OVER (ORDER BY MailInfo.uid ) numrow,uid FROM MailInfo WHERE MailInfo.user = '{userSession}' and MailInfo.mailboxname = '{userMailBox}')";
                string tempVar2 = $"({tempVar1} select uid from var where numrow>={left} and numrow<={right})";
                cnn.Execute($"update MailInfo set seen = 1 where user = '{userSession}' and mailboxname = '{userMailBox}' and uid in {tempVar2}", new DynamicParameters());
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
        public static int UpdateSeenFlagWithIndex(string userSession, string userMailBox, string index)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                string tempVar1 = $"WITH var AS (SELECT ROW_NUMBER () OVER (ORDER BY MailInfo.uid ) numrow,uid FROM MailInfo WHERE MailInfo.user = '{userSession}' and MailInfo.mailboxname = '{userMailBox}')";
                string tempVar2 = $"({tempVar1} select uid from var where numrow = {index})";
                cnn.Execute($"update MailInfo set seen = 1 where user = '{userSession}' and mailboxname = '{userMailBox}' and uid in {tempVar2}", new DynamicParameters());
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
        public static int UpdateFlagsWithIndex(string userSession, string userMailBox, string left, string right, Flags flags)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                string tempVar1 = $"WITH var AS (SELECT ROW_NUMBER () OVER (ORDER BY MailInfo.uid ) numrow,uid FROM MailInfo WHERE MailInfo.user = '{userSession}' and MailInfo.mailboxname = '{userMailBox}')";
                string tempVar2 = $"({tempVar1} select uid from var where numrow>={left} and numrow<={right})";
                cnn.Execute($"update MailInfo set seen = {flags.seen},answered = {flags.answered},flagged={flags.flagged},draft = {flags.draft},deleted = {flags.deleted} where user = '{userSession}' and mailboxname = '{userMailBox}' and uid in {tempVar2}", new DynamicParameters());
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
        public static int UpdateFlagsWithIndex(string userSession, string userMailBox, string index, Flags flags)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                string tempVar1 = $"WITH var AS (SELECT ROW_NUMBER () OVER (ORDER BY MailInfo.uid ) numrow,uid FROM MailInfo WHERE MailInfo.user = '{userSession}' and MailInfo.mailboxname = '{userMailBox}')";
                string tempVar2 = $"({tempVar1} select uid from var where numrow>={index})";
                cnn.Execute($"update MailInfo set seen = {flags.seen},answered = {flags.answered},flagged={flags.flagged},draft = {flags.draft},deleted = {flags.deleted} where user = '{userSession}' and mailboxname = '{userMailBox}' and uid in {tempVar2}", new DynamicParameters());
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

        internal static int DeleteMailWithUID(string userSession, string userMailBox, long uid)
        {
            IDbConnection cnn = new SQLiteConnection("Data Source = .\\Imap\\ImapDB.db");
            cnn.Open();
            try
            {
                cnn.Execute($"delete from MailInfo Where user = '{userSession}' and mailboxname = '{userMailBox}' and uid = {uid}", new DynamicParameters());
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
    }
}