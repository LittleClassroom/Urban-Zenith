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
                    Status TEXT
                );

                CREATE TABLE IF NOT EXISTS OrderItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    MenuItemId INTEGER NOT NULL,
                    Quantity INTEGER DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Payments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    PaymentMethod TEXT,
                    PaidAmount REAL,
                    PaidAt TEXT DEFAULT CURRENT_TIMESTAMP
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
    }
}
