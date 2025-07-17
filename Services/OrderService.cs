using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UrbanZenith.Database;
using UrbanZenith.Models;
using ConsoleTableExt;
using Spectre.Console;

namespace UrbanZenith.Services
{
    public static class OrderService
    {
        public static int CreateNewOrder(int tableId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            // Declared checkTableCmd within the scope of its usage
            var checkTableCmd = conn.CreateCommand();
            checkTableCmd.CommandText = "SELECT Status FROM Tables WHERE Id = @tableId";
            checkTableCmd.Parameters.AddWithValue("@tableId", tableId);

            var status = checkTableCmd.ExecuteScalar()?.ToString();
            if (status == "Occupied")
            {
                Console.WriteLine($"Error: Table {tableId} is already occupied.");
                return -1;
            }

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Orders (TableId, Status)
                VALUES (@tableId, 'Active');
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@tableId", tableId);

            long newOrderId = (long)cmd.ExecuteScalar();

            var updateTableCmd = conn.CreateCommand();
            updateTableCmd.CommandText = "UPDATE Tables SET Status = 'Occupied' WHERE Id = @tableId";
            updateTableCmd.Parameters.AddWithValue("@tableId", tableId);
            updateTableCmd.ExecuteNonQuery();

            Console.WriteLine($"New order created with ID {newOrderId} for Table {tableId}.");
            return (int)newOrderId;
        }

        private static int GetTotalOrderCount()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Orders;";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ListOrders(int page = 1, int pageSize = 10)
        {
            Console.Clear();
            Console.WriteLine("=== Order List ===");

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            int totalOrders = GetTotalOrderCount();
            int totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            int offset = (page - 1) * pageSize;

            Console.WriteLine($"Showing orders page {page} of {totalPages} ({totalOrders} total orders).");
            Console.WriteLine("-------------------------------------------------------------------");

            var ordersData = new List<object[]>();

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT Orders.Id, Tables.Name, Orders.Status, Orders.OrderDate
        FROM Orders
        JOIN Tables ON Orders.TableId = Tables.Id
        ORDER BY Orders.OrderDate DESC
        LIMIT @pageSize OFFSET @offset;
    ";
            cmd.Parameters.AddWithValue("@pageSize", pageSize);
            cmd.Parameters.AddWithValue("@offset", offset);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ordersData.Add(new object[]
                {
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            if (ordersData.Count == 0)
            {
                Console.WriteLine(page == 1 ? "No orders found in the system." : "No more orders on this page.");
            }
            else
            {
                var table = new Spectre.Console.Table();

                table.AddColumn("Order ID");
                table.AddColumn("Table");
                table.AddColumn("Status");
                table.AddColumn("Order Date");

                foreach (var row in ordersData)
                {
                    table.AddRow(
                        row[0].ToString(),
                        row[1].ToString(),
                        row[2].ToString(),
                        row[3].ToString()
                    );
                }

                AnsiConsole.Write(table);
            }

            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine($"Pages: {page} / {totalPages} (Use 'order list <page_number>' to navigate)");
            Console.WriteLine("===================================================================");
            Console.WriteLine();
        }

        public static void CompleteOrder(int orderId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            // Declare checkOrderCmd here to keep it in scope for all its uses
            var checkOrderCmd = conn.CreateCommand();
            checkOrderCmd.CommandText = "SELECT Status, TableId FROM Orders WHERE Id = @orderId";
            checkOrderCmd.Parameters.AddWithValue("@orderId", orderId);

            using var reader = checkOrderCmd.ExecuteReader();
            if (!reader.Read())
            {
                Console.WriteLine($"Error: Order {orderId} does not exist.");
                return;
            }

            string status = reader.GetString(0);
            int tableId = reader.GetInt32(1);

            if (status == "Completed")
            {
                Console.WriteLine($"Order {orderId} is already completed.");
                return;
            }

            var updateOrderCmd = conn.CreateCommand();
            updateOrderCmd.CommandText = "UPDATE Orders SET Status = 'Completed' WHERE Id = @orderId";
            updateOrderCmd.Parameters.AddWithValue("@orderId", orderId);
            updateOrderCmd.ExecuteNonQuery();

            var updateTableCmd = conn.CreateCommand();
            updateTableCmd.CommandText = "UPDATE Tables SET Status = 'Available' WHERE Id = @tableId";
            updateTableCmd.Parameters.AddWithValue("@tableId", tableId);
            updateTableCmd.ExecuteNonQuery();

            Console.WriteLine($"Order {orderId} marked as completed. Table {tableId} is now unoccupied.");
        }

        public static int? GetActiveOrderIdByTableId(int tableId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id FROM Orders WHERE TableId = @tableId AND Status = 'Active' LIMIT 1";
            cmd.Parameters.AddWithValue("@tableId", tableId);

            var result = cmd.ExecuteScalar();
            if (result == null)
                return null;

            return Convert.ToInt32(result);
        }

        public static decimal CalculateTotalByTableId(int tableId)
        {
            decimal total = 0m;

            string sql = @"
                SELECT oi.Quantity, mi.Price
                FROM Orders o
                INNER JOIN OrderItems oi ON o.Id = oi.OrderId
                INNER JOIN MenuItems mi ON oi.MenuItemId = mi.Id
                WHERE o.TableId = @tableId AND o.Status = 'Active';
            ";

            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@tableId", tableId)
            };

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    int quantity = Convert.ToInt32(reader["Quantity"]);
                    decimal price = Convert.ToDecimal(reader["Price"]);
                    total += quantity * price;
                }
            }, parameters);

            return total;
        }

        public static void CancelOrder(int orderId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var checkOrderCmd = conn.CreateCommand();
            checkOrderCmd.CommandText = "SELECT Status, TableId FROM Orders WHERE Id = @orderId";
            checkOrderCmd.Parameters.AddWithValue("@orderId", orderId);

            using var reader = checkOrderCmd.ExecuteReader();
            if (!reader.Read())
            {
                Console.WriteLine($"Error: Order {orderId} does not exist.");
                return;
            }

            string status = reader.GetString(0);
            int tableId = reader.GetInt32(1);

            if (status == "Completed")
            {
                Console.WriteLine($"Order {orderId} is already completed and cannot be cancelled.");
                return;
            }
            if (status == "Cancelled")
            {
                Console.WriteLine($"Order {orderId} is already cancelled.");
                return;
            }

            // Update order status to 'Cancelled'
            var updateOrderCmd = conn.CreateCommand();
            updateOrderCmd.CommandText = "UPDATE Orders SET Status = 'Cancelled' WHERE Id = @orderId";
            updateOrderCmd.Parameters.AddWithValue("@orderId", orderId);
            updateOrderCmd.ExecuteNonQuery();

            // Update associated table status to 'Available'
            var updateTableCmd = conn.CreateCommand();
            updateTableCmd.CommandText = "UPDATE Tables SET Status = 'Available' WHERE Id = @tableId";
            updateTableCmd.Parameters.AddWithValue("@tableId", tableId);
            updateTableCmd.ExecuteNonQuery();

            Console.WriteLine($"Order {orderId} has been cancelled. Table {tableId} is now available.");
        }



    }
}