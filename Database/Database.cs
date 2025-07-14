using System;
using System.Data.SQLite;
using System.IO;

namespace UrbanZenith.Database
{
    public static class DatabaseContext
    {
        private const string FolderPath = "Database";
        private const string DbFileName = "UrbanZenith.db";
        private static readonly string DbPath = Path.Combine(FolderPath, DbFileName);
        public static SQLiteConnection GetConnection()
        {
            Directory.CreateDirectory(FolderPath); 
            return new SQLiteConnection($"Data Source={DbPath};Version=3;");
        }

        public static void Initialize()
        {
            Directory.CreateDirectory(FolderPath);
            if (!File.Exists(DbPath))
            {
                Console.WriteLine("📦 Creating new SQLite database...");
                SQLiteConnection.CreateFile(DbPath);
            }


            using var conn = GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS MenuItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        Price REAL NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Tables (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        Status TEXT DEFAULT 'Available',
                        StaffId INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TableId INTEGER NOT NULL,
                        OrderDate TEXT DEFAULT CURRENT_TIMESTAMP,
                        Status TEXT,
                        FOREIGN KEY (TableId) REFERENCES Tables(Id)
                    );

                    CREATE TABLE IF NOT EXISTS OrderItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL,
                        MenuItemId INTEGER NOT NULL,
                        Quantity INTEGER DEFAULT 1,
                        FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                        FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL,
                        PaymentMethod TEXT,
                        PaidAmount REAL,
                        PaidAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(OrderId) REFERENCES Orders(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Staff (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Role TEXT,
                        Username TEXT UNIQUE,
                        Password TEXT
                    );
                ";

            cmd.ExecuteNonQuery();

            Console.WriteLine("✅ Database initialized.");
        }

        public static void ExecuteQuery(string sql)
        {
            ExecuteQuery(sql, null, null);
        }

        public static void ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            cmd.ExecuteNonQuery();
        }

        public static void ExecuteQuery(string sql, Action<SQLiteDataReader> handleRow, params SQLiteParameter[] parameters)
        {
            using var conn = GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using var reader = cmd.ExecuteReader();
            handleRow?.Invoke(reader);
        }

        public static List<Dictionary<string, object>> SelectAsDictionaryList(
            string tableName,
            string whereClause = null,
            SQLiteParameter[] parameters = null)
        {
            var results = new List<Dictionary<string, object>>();
            string sql = $"SELECT * FROM {tableName}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += " WHERE " + whereClause;
            }

            ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    results.Add(row);
                }
            }, parameters ?? new SQLiteParameter[0]);

            return results;
        }

    }
}
