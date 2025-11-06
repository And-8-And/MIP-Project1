using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Lab3_1
{
    internal class SQLiteHandler
    {
        private SQLiteConnection connection;
        private string dbFile = "kewywords.db";

        public void ConnectToDb()
        {
            try
            {
                if (!File.Exists(dbFile))
                {
                    SQLiteConnection.CreateFile(dbFile);
                }

                connection = new SQLiteConnection("Data Source = " + dbFile + ";Version=3;");
                connection.Open();

                string sql = "CREATE TABLE IF NOT EXISTS Keywords (id INTEGER PRIMARY KEY AUTOINCREMENT, keyword TEXT)";

                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database connection error: " + ex.Message);
            }
        }

        public void DisconnectFromDb()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }

        public void InsertKeyword(string keyword)
        {
            if (connection == null)
            {
                System.Windows.Forms.MessageBox.Show("Database not connected");

                return;
            }

            string sql = "INSERT INTO Keywords (keyword) VALUES (@keyword)";

            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.Parameters.AddWithValue("@keyword", keyword);
            command.ExecuteNonQuery();
        }

        public List<string> GetAllKeywords()
        {
            List<string> keywords = new List<string>();

            if (connection == null)
            {
                System.Windows.Forms.MessageBox.Show("Database not connected");

                return keywords;
            }

            string sql = "SELECT keyword FROM Keywords";

            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                keywords.Add(reader["keyword"].ToString());
            }

            reader.Close();
            return keywords;
        }
    }
}
