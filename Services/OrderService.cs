using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UrbanZenith.Database;
using UrbanZenith.Models;

namespace UrbanZenith.Services
{
    public static class OrderService
    {
        public static int CreateNewOrder(int tableId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var checkTableCmd = conn.CreateCommand();
            checkTableCmd.CommandText = "SELECT Status FROM Tables WHERE Id = @tableId";
            checkTableCmd.Parameters.AddWithValue("@tableId", tableId);

            var status = checkTableCmd.ExecuteScalar()?.ToString();
            if (status == "Occupied")
            {
                Console.WriteLine($"Table {tableId} is already occupied.");
                return -1;
            }

            // Insert new order
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Orders (TableId, Status)
                VALUES (@tableId, 'Active');
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@tableId", tableId);

            long newOrderId = (long)cmd.ExecuteScalar();

            // Update table status to Occupied
            var updateTableCmd = conn.CreateCommand();
            updateTableCmd.CommandText = "UPDATE Tables SET Status = 'Occupied' WHERE Id = @tableId";
            updateTableCmd.Parameters.AddWithValue("@tableId", tableId);
            updateTableCmd.ExecuteNonQuery();

            Console.WriteLine($"New order created with ID {newOrderId} for Table {tableId}.");
            return (int)newOrderId;
        }

        // List all orders with status and table info
        public static void ListOrders()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Orders.Id, Tables.Name, Orders.Status, Orders.OrderDate
                FROM Orders
                JOIN Tables ON Orders.TableId = Tables.Id
                ORDER BY Orders.OrderDate DESC;
            ";

            using var reader = cmd.ExecuteReader();

            Console.WriteLine("=== Orders ===");
            while (reader.Read())
            {
                int orderId = reader.GetInt32(0);
                string tableName = reader.GetString(1);
                string status = reader.GetString(2);
                string orderDate = reader.GetString(3);

                Console.WriteLine($"Order ID: {orderId}, Table: {tableName}, Status: {status}, Date: {orderDate}");
            }
            Console.WriteLine("==============");
        }

        // Complete an order by order ID and free the table
        public static void CompleteOrder(int orderId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            // Check if order exists and is active
            var checkOrderCmd = conn.CreateCommand();
            checkOrderCmd.CommandText = "SELECT Status, TableId FROM Orders WHERE Id = @orderId";
            checkOrderCmd.Parameters.AddWithValue("@orderId", orderId);

            using var reader = checkOrderCmd.ExecuteReader();
            if (!reader.Read())
            {
                Console.WriteLine($"Order {orderId} does not exist.");
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

    }
}
