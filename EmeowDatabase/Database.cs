using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage; 

namespace EmeowDatabase
{
    public class Database
    {
        public static string DatabaseName { get; set; } = "EmeowDatabase.db";

        public enum Table
        {
            Accounts,
        }

        public async static void InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(DatabaseName, CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
            Debug.Print(dbpath);
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                await db.OpenAsync();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS Accounts (" +
                    "LocalPart NVARCHAR(100) NOT NULL, " +
                    "Domain NVARCHAR(50) NOT NULL, " +
                    "Name NVARCHAR(100), " +
                    "Password NVARCHAR(100) NOT NULL, " +
                    "Glyph NVARCHAR(10), " + 
                    "PRIMARY KEY(LocalPart,Domain));";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                await createTable.ExecuteReaderAsync();

                db.Close();
            }
        }
        public async static void AddData(Table table, params string[] ps)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
            try
            {
                using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
                {
                    db.Open();

                    string commandText = "INSERT INTO " + table.ToString() + " VALUES (";

                    for (int i = 1; i <= ps.Length; i++)
                    {
                        commandText += "@Entry" + i.ToString();

                        if (i != ps.Length)
                            commandText += ", ";
                    }

                    commandText += ");";

                    SqliteCommand insertCommand = new SqliteCommand();
                    insertCommand.Connection = db;

                    // Use parameterized query to prevent SQL injection attacks
                    insertCommand.CommandText = commandText;

                    for (int i = 1; i <= ps.Length; i++)
                    {
                        insertCommand.Parameters.AddWithValue("@Entry" + i.ToString(), ps[i - 1]);
                    }

                    await insertCommand.ExecuteReaderAsync();

                    db.Close();
                }
            }
            catch (Exception)
            {

            }

        }

        public async static Task<List<string>> GetColumnData(Table table, string column)
        {
            List<string> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                await db.OpenAsync();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT " + column + " FROM " + table.ToString(), db);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();
            }

            return entries;
        }

        public async static Task<List<List<string>>> GetTableData(Table table)
        {
            List<List<string>> data = new List<List<string>>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                await db.OpenAsync();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT *"  + " FROM " + table.ToString(), db);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    List<string> entries = new List<string>();

                    for (int i = 0; i < query.FieldCount; i++)
                    { 
                        entries.Add(query.GetString(i));
                    }
                    data.Add(entries);
                }

                db.Close();
            }

            return data;
        }

        public async static Task<List<string>> SearchAccountData(string localPart, string domain, string pwd)
        {
            List<string> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                await db.OpenAsync();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT *" +
                    " FROM Accounts" +
                    " WHERE LocalPart = " + localPart +
                    " AND Domain = " + domain + 
                    " AND Password = " + pwd, db);

                Debug.Print("SELECT *" +
                    " FROM Accounts" +
                    " WHERE LocalPart = " + localPart +
                    " AND Domain = " + domain +
                    " AND Password = " + pwd);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (query.Read())
                {
                    for (int i = 0; i < query.FieldCount; i++)
                    {
                        entries.Add(query.GetString(i));
                    }
                }

                db.Close();
            }

            return entries;
        }
    }
}
