using System;
//using System.Threading.Tasks;
using System.Windows;

using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;
namespace WIoTa_Serial_Tool
{
    public partial class MainWindow : Window
    {
        public static void CreateSQLiteTable(string databasePath, string tableName, int tab_index)
        {
            string connectionString = $"Data Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = "";
                switch (tab_index)
                {
                    case 0:
                        createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} (NumCol INTEGER, Hex INTEGER, AT指令 TEXT, 发送 TEXT)";
                        break;
                    case 1:
                        createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} (NumCol INTEGER, Hex INTEGER, 延时 TEXT, AT指令 TEXT, 发送 TEXT)";
                        break;
                    case 2:
                        createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} (NumCol INTEGER, Hex INTEGER, 延时 TEXT, 应答 TEXT, AT指令 TEXT, 发送 TEXT)";
                        break;
                }
                using (SQLiteCommand createTableCommand = new SQLiteCommand(createTableQuery, connection))
                {
                    createTableCommand.ExecuteNonQuery();
                }
            }
        }

        public static void InsertDataToSQLite(string databasePath, string tableName, List<GridDataTemp> data, int tab_index)
        {
            string connectionString = $"Data Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 清空表格
                string deleteQuery = $"DELETE FROM {tableName}";
                using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
                {
                    deleteCommand.ExecuteNonQuery();
                }

                string insertQuery = "";
                switch (tab_index)
                {
                    case 0:
                        insertQuery = $"INSERT INTO {tableName} (NumCol, Hex, AT指令, 发送) VALUES (@NumCol, @Hex, @AT指令, @发送)";
                        break;
                    case 1:
                        insertQuery = $"INSERT INTO {tableName} (NumCol, Hex, 延时, AT指令, 发送) VALUES (@NumCol, @Hex, @延时, @AT指令, @发送)";
                        break;
                    case 2:
                        insertQuery = $"INSERT INTO {tableName} (NumCol, Hex, 延时, 应答, AT指令, 发送) VALUES (@NumCol, @Hex, @延时, @应答, @AT指令, @发送)";
                        break;
                }

                using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                {
                    foreach (GridDataTemp item in data)
                    {
                        insertCommand.Parameters.AddWithValue("@NumCol", item.NumCol);
                        insertCommand.Parameters.AddWithValue("@Hex", item.Hex ? 1 : 0);

                        if (tab_index == 0)
                        {
                            insertCommand.Parameters.AddWithValue("@AT指令", item.AT指令);
                            insertCommand.Parameters.AddWithValue("@发送", item.发送);
                        }
                        else if (tab_index == 1)
                        {
                            insertCommand.Parameters.AddWithValue("@延时", item.延时);
                            insertCommand.Parameters.AddWithValue("@AT指令", item.AT指令);
                            insertCommand.Parameters.AddWithValue("@发送", item.发送);
                        }
                        else if (tab_index == 2)
                        {
                            insertCommand.Parameters.AddWithValue("@延时", item.延时);
                            insertCommand.Parameters.AddWithValue("@应答", item.应答);
                            insertCommand.Parameters.AddWithValue("@AT指令", item.AT指令);
                            insertCommand.Parameters.AddWithValue("@发送", item.发送);
                        }

                        insertCommand.ExecuteNonQuery();

                        insertCommand.Parameters.Clear();
                    }
                }
            }
        }

        public static List<GridDataTemp> ReadDataFromSQLite(string databasePath, string tableName, int tab_index)
        {
            List<GridDataTemp> data = new List<GridDataTemp>();

            string connectionString = $"Data Source={databasePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string selectQuery = $"SELECT * FROM {tableName}";
      
                    using (SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection))
                    {
                        using (SQLiteDataReader reader = selectCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GridDataTemp item = new GridDataTemp();
                                item.NumCol = reader.GetInt32(0);
                                item.Hex = reader.GetInt32(1) == 1;

                                if (tab_index == 0)
                                {
                                    item.AT指令 = reader.GetString(2);
                                    item.发送 = reader.GetString(3);
                                }
                                else if (tab_index == 1)
                                {
                                    item.延时 = reader.GetString(2);
                                    item.AT指令 = reader.GetString(3);
                                    item.发送 = reader.GetString(4);
                                }
                                else if (tab_index == 2)
                                {
                                    item.延时 = reader.GetString(2);
                                    item.应答 = reader.GetString(3);
                                    item.AT指令 = reader.GetString(4);
                                    item.发送 = reader.GetString(5);
                                }
                                data.Add(item);
                                   
                             }
                        }
                    }
                }

                return data;
            }
            catch(SQLiteException)
            {
                return data;
            }
          
        }

    }
}
