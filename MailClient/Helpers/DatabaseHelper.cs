using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MailClient.Helpers
{
    public static class DatabaseHelper
    {
        public const string AccountsDatabaseName = "MailClientAccounts.db";

        public const string AccountTableName = "Accounts";

        public static string CurrentDatabaseName;

        public static async Task InitalizeAsync()
        {
            try
            {
                // Create database if not exists
                _ = await ApplicationData.Current.LocalFolder.CreateFileAsync(AccountsDatabaseName, CreationCollisionOption.OpenIfExists);

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AccountsDatabaseName);

                // Print path of database in debug output
                Debug.WriteLine(dbpath);

                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS Accounts (" +
                    "Address NVARCHAR NOT NULL, " +
                    "Name NVARCHAR, " +
                    "Password NVARCHAR NOT NULL, " +
                    "Glyph NVARCHAR NOT NULL, " +
                    "PRIMARY KEY(Address));";

                SqliteCommand createTable = new(tableCommand, db);

                _ = await createTable.ExecuteReaderAsync();

                db.Close();
            }
            catch (Exception) { }
        }

        public static async Task InitializeAccountDatabaseAsync(string databaseName)
        {
            _ = await ApplicationData.Current.LocalFolder.CreateFileAsync(databaseName, CreationCollisionOption.OpenIfExists);

            await CreateAccountMailboxAsync(databaseName, "Inbox");
        }

        public static async Task CreateAccountMailboxAsync(string databaseName, string mailboxName)
        {
            try
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);

                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string tableCommand = "CREATE TABLE IF NOT " +
                    $"EXISTS {mailboxName} (" +
                    "Uid INTEGER NOT NULL, " +
                    "FromAddr VARCHAR, " +
                    "ToAddrs VARCHAR, " +
                    "Subject NVARCHAR, " +
                    "Date VARCHAR, " +
                    "Body NVARCHAR, " +
                    "Attachments VARCHAR, " +
                    "Seen INTEGER NOT NULL, " +
                    "Answered INTEGER NOT NULL, " +
                    "Flagged INTEGER NOT NULL, " +
                    "Deleted INTEGER NOT NULL, " +
                    "Draft INTEGER NOT NULL, " +
                    "Recent INTEGER NOT NULL, " +
                    "PRIMARY KEY(UID));";

                SqliteCommand createTable = new(tableCommand, db);

                _ = await createTable.ExecuteReaderAsync();

                db.Close();
            }
            catch (Exception) { }
        }

        public static async Task CreateAccountMailboxesAsync(string databaseName, string[] mailboxNames)
        {
            try
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);

                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string tableCommand;

                SqliteCommand createTable;

                foreach (string mailboxName in mailboxNames)
                {
                    tableCommand = "CREATE TABLE IF NOT " +
                    $"EXISTS {mailboxName} (" +
                    "UID INTEGER NOT NULL, " +
                    "From VARCHAR, " +
                    "To VARCHAR, " +
                    "Subject NVARCHAR, " +
                    "Date VARCHAR, " +
                    "Body NVARCHAR, " +
                    "Attachments VARCHAR " +
                    "Seen INTEGER NOT NULL, " +
                    "Answered INTEGER NOT NULL, " +
                    "Flagged INTEGER NOT NULL, " +
                    "Deleted INTEGER NOT NULL, " +
                    "Draft INTEGER NOT NULL, " +
                    "Recent INTEGER NOT NULL, " +
                    "PRIMARY KEY(UID));";

                    createTable = new SqliteCommand(tableCommand, db);

                    _ = await createTable.ExecuteReaderAsync();
                }

                db.Close();
            }
            catch (Exception) { }
        }

        public static async Task InsertDataAsync(string databaseName, string tableName, string[] data)
        {
            try
            {
                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);

                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string commandText = "INSERT INTO " + tableName + " VALUES (";

                for (int i = 0; i < data.Length; i++)
                {
                    commandText += "@Entry" + i.ToString();

                    if (i != data.Length - 1)
                        commandText += ", ";
                }

                commandText += ");";

                // Use parameterized query to prevent SQL injection attacks
                SqliteCommand command = new(commandText, db);

                for (int i = 0; i < data.Length; i++)
                {
                    _ = command.Parameters.AddWithValue($"@Entry{i}", data[i]);
                }

                _ = await command.ExecuteReaderAsync();

                db.Close();
            }
            catch (Exception) { }
        }

        public async static Task<List<string>> SelectColumnDataAsync(string databaseName, string tableName, string column)
        {
            try
            {
                List<string> entries = new();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);

                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                SqliteCommand selectCommand = new("SELECT " + column + " FROM " + tableName.ToString(), db);

                SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                while (await query.ReadAsync())
                {
                    entries.Add(query.GetString(0));
                }

                db.Close();

                return entries;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<List<List<string>>> GetColumnsDataAsync(string databaseName, string tableName, string[] columnNames)
        {
            try
            {
                if (columnNames.Length == 0) return null;

                List<List<string>> data = new();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string commandText = "SELECT " + string.Join(", ", columnNames) + $" FROM {tableName};";

                SqliteCommand command = new(commandText, db);

                SqliteDataReader query = await command.ExecuteReaderAsync();

                while (await query.ReadAsync())
                {
                    List<string> entries = new();
                    for (int i = 0; i < query.FieldCount; i++)
                    {
                        entries.Add(query.GetString(i));
                    }
                    data.Add(entries);
                }

                db.Close();

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<List<List<string>>> GetTableDataAsync(string databaseName, string tableName)
        {
            try
            {
                List<List<string>> data = new();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                SqliteCommand command = new("SELECT *" + " FROM " + tableName, db);

                SqliteDataReader query = await command.ExecuteReaderAsync();

                while (await query.ReadAsync())
                {
                    List<string> entries = new();

                    for (int i = 0; i < query.FieldCount; i++)
                    {
                        entries.Add(query.GetString(i));
                    }
                    data.Add(entries);
                }

                db.Close();

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<bool> CheckDataAsync(string databaseName, string tableName, Tuple<string, string>[] data)
        {
            try
            {
                if (data.Length == 0) return false;

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                // Create command
                string commandText = "SELECT *" + $" FROM {tableName}";
                commandText += $" WHERE {data[0].Item1} = @Entry0";

                for (int i = 1; i < data.Length; i++)
                {
                    commandText += $" AND {data[i].Item1} = @Entry{i}";
                }

                commandText += ";";

                // Use parameterized query to prevent SQL injection attacks
                SqliteCommand command = new(commandText, db);

                for (int i = 0; i < data.Length; i++)
                {
                    command.Parameters.AddWithValue($"@Entry{i}", data[i].Item2);
                }

                SqliteDataReader query = await command.ExecuteReaderAsync();

                db.Close();

                return query.HasRows;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> CheckDataAsync(string databaseName, string tableName, string[] columns, Tuple<string, string>[] data)
        {
            try
            {
                if (data.Length == 0 || columns.Length == 0) return false;

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string commandText = "SELECT " + string.Join(", ", columns);
                commandText = commandText.Remove(commandText.Length - 2) + $" FROM {tableName}";
                commandText += $" WHERE {data[0].Item1} = $Entry0";

                for (int i = 1; i < data.Length; i++)
                {
                    commandText += $" AND {data[i].Item1} = $@Entry{i}";
                }

                commandText += ";";

                // Use parameterized query to prevent SQL injection attacks
                SqliteCommand command = new(commandText, db);

                for (int i = 0; i < data.Length; i++)
                {
                    command.Parameters.AddWithValue($"@Entry{i}", data[i].Item2);
                }

                SqliteDataReader query = await command.ExecuteReaderAsync();

                db.Close();

                return query.HasRows;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<List<string>> GetDataAsync(string databaseName, string tableName, Tuple<string, string>[] data)
        {
            try
            {
                if (data.Length == 0) return null;

                List<string> entries = new();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string commandText = "SELECT *" +
                        $" FROM {tableName}";
                commandText += $" WHERE {data[0].Item1} = @Entry0";

                for (int i = 1; i < data.Length; i++)
                {
                    commandText += $" AND {data[i].Item1} = @Entry{i}";
                }

                commandText += ";";

                // Use parameterized query to prevent SQL injection attacks
                SqliteCommand command = new(commandText, db);

                for (int i = 0; i < data.Length; i++)
                {
                    command.Parameters.AddWithValue($"@Entry{i}", data[i].Item2);
                }
                SqliteDataReader query = await command.ExecuteReaderAsync();

                while (query.Read())
                {
                    for (int i = 0; i < query.FieldCount; i++)
                    {
                        entries.Add(query.GetString(i));
                    }
                }

                db.Close();

                return entries;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string[]> SelectDataAsync(string databaseName, string tableName, string[] columns, Tuple<string, string>[] conditions)
        {
            try
            {
                if (conditions.Length == 0 || columns.Length == 0) return null;

                List<string> entries = new();

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                string commandText = "SELECT " + string.Join(", ", columns) + $" FROM {tableName}";
                commandText += $" WHERE {conditions[0].Item1} = @Entry0";

                for (int i = 1; i < conditions.Length; i++)
                {
                    commandText += $" AND {conditions[i].Item1} = @Entry{i}";
                }

                commandText += ";";

                // Use parameterized query to prevent SQL injection attacks
                SqliteCommand command = new(commandText, db);

                for (int i = 0; i < conditions.Length; i++)
                {
                    command.Parameters.AddWithValue($"@Entry{i}", conditions[i].Item2);
                }

                SqliteDataReader query = await command.ExecuteReaderAsync();

                while (await query.ReadAsync())
                {
                    for (int i = 0; i < query.FieldCount; i++)
                    {
                        entries.Add(query.GetString(i));
                    }
                }

                db.Close();

                return entries.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task DeleteRowsAsync(string databaseName, string tableName, Tuple<string, string>[] conditions)
        {
            try
            {
                if (conditions.Length == 0) return;

                string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseName);
                using SqliteConnection db = new($"Filename={dbpath}");

                await db.OpenAsync();

                // Create command
                string commandText = $"DELETE FROM {tableName}";
                commandText += $" WHERE {conditions[0].Item1} = @Entry0";

                for (int i = 1; i < conditions.Length; i++)
                {
                    commandText += $" AND {conditions[i].Item1} = @Entry{i}";
                }

                commandText += ";";

                // Use parameterized query to prevent SQL injection attacks
                SqliteCommand command = new(commandText, db);

                for (int i = 0; i < conditions.Length; i++)
                {
                    command.Parameters.AddWithValue($"@Entry{i}", conditions[i].Item2);
                }

                SqliteDataReader query = await command.ExecuteReaderAsync();

                db.Close();
            }
            catch (Exception) { }
        }
    }
}