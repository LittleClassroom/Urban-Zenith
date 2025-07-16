using ConsoleTableExt;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using UrbanZenith.Database;
using UrbanZenith.Models;

namespace UrbanZenith.Services
{
    public static class MenuService
    {
        
        public static void ListMenuItems(int page = 1)
        {
            Console.Clear();
            const int pageSize = 10;

            if (page < 1) page = 1;

            int totalItems = GetMenuItemCount();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            int offset = (page - 1) * pageSize;

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT Id, Name, Price
        FROM MenuItems
        ORDER BY Id
        LIMIT @limit OFFSET @offset;
    ";
            cmd.Parameters.AddWithValue("@limit", pageSize);
            cmd.Parameters.AddWithValue("@offset", offset);

            var rows = new List<List<object>>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new List<object>
        {
            reader["Id"],
            reader["Name"],
            $"{Convert.ToDecimal(reader["Price"]):C}"
        });
            }

            if (rows.Count == 0)
            {
                Console.WriteLine($"No menu items found on page {page}.");
                return;
            }

            ConsoleTableExt.ConsoleTableBuilder
                .From(rows)
                .WithTitle($"Menu Items - Page {page}/{totalPages}", ConsoleColor.Yellow, ConsoleColor.Black)
                .WithColumn("ID", "Name", "Price")
                .ExportAndWriteLine();
        }

        private static int GetMenuItemCount()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM MenuItems";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void InfoMenuItem(int itemId)
        {
            Console.Clear();
            try
            {
                using var conn = DatabaseContext.GetConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Name, Description, Price FROM MenuItems WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", itemId);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    string description = reader.IsDBNull(2) ? "No description." : reader.GetString(2);
                    decimal price = reader.GetDecimal(3);

                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine($"ID          : {id:D3}");
                    Console.WriteLine($"Name        : {name}");
                    Console.WriteLine($"Description : {description}");
                    Console.WriteLine($"Price       : ${price:F2}");
                    Console.WriteLine("---------------------------------------------------------------");
                }
                else
                {
                    Console.WriteLine($"[!] No menu item found with ID {itemId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.Message);
            }
        }




        public static void AddMenuItem()
        {
            Console.Write("Enter Name: ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Name cannot be empty.");
                return;
            }

            Console.Write("Enter Description (optional): ");
            string description = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter Price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price <= 0)
            {
                Console.WriteLine("Invalid price.");
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO MenuItems (Name, Description, Price)
                VALUES (@name, @description, @price);";

            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@price", price);

            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                Console.WriteLine("Menu item added successfully.");
            else
                Console.WriteLine("Failed to add menu item.");
        }

        public static void UpdateMenuItem(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name, Description, Price FROM MenuItems WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                Console.WriteLine($"Menu item with ID {id} not found.");
                return;
            }

            string currentName = reader.GetString(0);
            string currentDesc = reader.IsDBNull(1) ? "" : reader.GetString(1);
            decimal currentPrice = reader.GetDecimal(2);

            Console.WriteLine("=== Update Menu Item ===");
            Console.WriteLine($"Current Name: {currentName}");
            Console.Write("New Name (leave empty to keep): ");
            string newName = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(newName)) newName = currentName;

            Console.WriteLine($"Current Description: {currentDesc}");
            Console.Write("New Description (leave empty to keep): ");
            string newDesc = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(newDesc)) newDesc = currentDesc;

            Console.WriteLine($"Current Price: ${currentPrice:F2}");
            Console.Write("New Price (leave empty to keep): ");
            string newPriceInput = Console.ReadLine()?.Trim();
            decimal newPrice = decimal.TryParse(newPriceInput, out decimal parsedPrice) ? parsedPrice : currentPrice;

            reader.Close(); // close reader before new command

            cmd = conn.CreateCommand();
            cmd.CommandText = @"
        UPDATE MenuItems 
        SET Name = @name, Description = @desc, Price = @price 
        WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@name", newName);
            cmd.Parameters.AddWithValue("@desc", newDesc);
            cmd.Parameters.AddWithValue("@price", newPrice);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0
                ? $"✅ Menu item {id} updated."
                : $"❌ Failed to update menu item.");
        }

        public static void RemoveMenuItem(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM MenuItems WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                Console.WriteLine($"🗑️ Menu item {id} removed.");
            else
                Console.WriteLine($"⚠️ Menu item {id} not found.");
        }


    }
}
