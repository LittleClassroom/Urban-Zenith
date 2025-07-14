using ConsoleTableExt;
using System;
using System.Data.Entity.Core.Objects;
using System.Data.SQLite;
using UrbanZenith.Database;
using UrbanZenith.Models;

namespace UrbanZenith.Services
{
    public static class StaffService
    {
        public static void ListStaff()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Staff ORDER BY Name";

            using var reader = cmd.ExecuteReader();

            var rows = new List<object[]>();

            Console.WriteLine("\n\n===== Staff List =====");
            while (reader.Read())
            {
                rows.Add(new object[] {
                    reader.GetInt32(0),
                    reader.GetString(1)
                });
            }

            ConsoleTableBuilder
                .From(rows)
                .WithColumn("ID", "Name")
                .WithFormat(ConsoleTableBuilderFormat.MarkDown)
                .ExportAndWriteLine();
        }

        public static void GetStaffInfo(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Role, Username FROM Staff WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            var rows = new List<Object[]>();

            if (reader.Read())
            {
                Console.WriteLine("=================================");
                Console.WriteLine("         Staff Information");
                Console.WriteLine("=================================");
                Console.WriteLine($"ID      : S-{reader.GetInt32(0).ToString("D3")}");
                Console.WriteLine($"Name    : {reader.GetString(1)}");
                Console.WriteLine($"Role    : {reader.GetString(2)}");
                Console.WriteLine($"Username: {reader.GetString(3)}");
                Console.WriteLine("=================================");


                Console.WriteLine("\n\n\n");

            }
            else
            {
                Console.WriteLine($"No staff found with ID {id}");
            }
        }

        public static void AddStaff()
        {
            Console.Write("Enter Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Role: ");
            string role = Console.ReadLine();

            Console.Write("Enter Username: ");
            string username = Console.ReadLine();

            Console.Write("Enter Password: ");
            string password = Console.ReadLine();

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Staff (Name, Role, Username, Password) 
                VALUES (@name, @role, @username, @password)";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    Console.WriteLine("Staff member added successfully.");
                else
                    Console.WriteLine("Failed to add staff member.");
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                    Console.WriteLine("Error: Username already exists.");
                else
                    Console.WriteLine($"Database error: {ex.Message}");
            }
        }

        public static void RemoveStaff(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Staff WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
                Console.WriteLine($"Staff member with ID {id} removed.");
            else
                Console.WriteLine($"No staff found with ID {id}.");
        }

        public static void UpdateStaff(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name, Role, Username FROM Staff WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                Console.WriteLine($"❌ Staff with ID {id} not found.");
                return;
            }

            string currentName = reader.GetString(0);
            string currentRole = reader.GetString(1);
            string currentUsername = reader.GetString(2);

            Console.WriteLine($"--- Updating Staff ID {id} ---");
            Console.WriteLine($"Current Name: {currentName}");
            Console.Write("New Name (leave blank to keep): ");
            string newName = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(newName)) newName = currentName;

            Console.WriteLine($"Current Role: {currentRole}");
            Console.Write("New Role (Waiter, Cashier, Admin): ");
            string newRole = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(newRole)) newRole = currentRole;

            Console.WriteLine($"Current Username: {currentUsername}");
            Console.Write("New Username (leave blank to keep): ");
            string newUsername = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(newUsername)) newUsername = currentUsername;

            Console.Write("Update Password? (yes/no): ");
            string updatePwd = Console.ReadLine()?.Trim().ToLower();
            string newPassword = null;

            if (updatePwd == "yes")
            {
                Console.Write("New Password: ");
                newPassword = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    Console.WriteLine("❌ Password cannot be empty.");
                    return;
                }
            }

            reader.Close();

            cmd = conn.CreateCommand();
            cmd.CommandText = @"
        UPDATE Staff SET Name = @name, Role = @role, Username = @username" +
                (newPassword != null ? ", Password = @password" : "") +
                " WHERE Id = @id";

            cmd.Parameters.AddWithValue("@name", newName);
            cmd.Parameters.AddWithValue("@role", newRole);
            cmd.Parameters.AddWithValue("@username", newUsername);
            if (newPassword != null)
                cmd.Parameters.AddWithValue("@password", newPassword);
            cmd.Parameters.AddWithValue("@id", id);

            if (cmd.ExecuteNonQuery() > 0)
                Console.WriteLine("✅ Staff updated.");
            else
                Console.WriteLine("❌ Update failed.");
        }

    }
}
