using System;
using System.Data.SQLite;
using UrbanZenith.Database;
using UrbanZenith.Models;

namespace UrbanZenith.Services
{
    public static class OrderItemService
    {
        public static void AddItemsToOrder(int orderId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            while (true)
            {
                Console.Write("Enter Menu Item ID (or 'done' to finish): ");
                string input = Console.ReadLine()?.Trim().ToLower();

                if (input == "done")
                    break;

                if (!int.TryParse(input, out int menuItemId))
                {
                    Console.WriteLine("Invalid ID. Please enter a number.");
                    continue;
                }

                // Retrieve menu item details
                var itemCmd = conn.CreateCommand();
                itemCmd.CommandText = "SELECT Name, Price FROM MenuItems WHERE Id = @id";
                itemCmd.Parameters.AddWithValue("@id", menuItemId);

                using var reader = itemCmd.ExecuteReader();
                if (!reader.Read())
                {
                    Console.WriteLine($"Menu item with ID {menuItemId} not found.");
                    continue;
                }

                string name = reader.GetString(0);
                decimal price = reader.GetDecimal(1);

                Console.WriteLine($"Selected: {name} - ${price:F2}");

                Console.Write("Enter quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
                    continue;
                }

                // Insert into OrderItems (INCLUDING Price)
                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
            INSERT INTO OrderItems (OrderId, MenuItemId, Quantity, Price)
            VALUES (@orderId, @menuItemId, @quantity, @price);
        ";
                insertCmd.Parameters.AddWithValue("@orderId", orderId);
                insertCmd.Parameters.AddWithValue("@menuItemId", menuItemId);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@price", price); // ⬅️ IMPORTANT

                insertCmd.ExecuteNonQuery();

                Console.WriteLine($"✔ Added {quantity}x {name} to Order #{orderId}.");
            }

            Console.WriteLine("✅ Finished adding items.");
        }


        private static void AddOrUpdateOrderItem(int orderId, int menuItemId, int quantity)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            // Check if order item already exists
            var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"
                SELECT Id, Quantity FROM OrderItems
                WHERE OrderId = @orderId AND MenuItemId = @menuItemId;";
            checkCmd.Parameters.AddWithValue("@orderId", orderId);
            checkCmd.Parameters.AddWithValue("@menuItemId", menuItemId);

            using var reader = checkCmd.ExecuteReader();

            if (reader.Read())
            {
                int existingId = reader.GetInt32(0);
                int existingQty = reader.GetInt32(1);

                // Update quantity
                reader.Close();
                var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE OrderItems SET Quantity = @qty WHERE Id = @id;";
                updateCmd.Parameters.AddWithValue("@qty", existingQty + quantity);
                updateCmd.Parameters.AddWithValue("@id", existingId);
                updateCmd.ExecuteNonQuery();

                Console.WriteLine($"Updated MenuItem {menuItemId} quantity to {existingQty + quantity}.");
            }
            else
            {
                // Insert new order item
                reader.Close();
                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO OrderItems (OrderId, MenuItemId, Quantity)
                    VALUES (@orderId, @menuItemId, @quantity);";
                insertCmd.Parameters.AddWithValue("@orderId", orderId);
                insertCmd.Parameters.AddWithValue("@menuItemId", menuItemId);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.ExecuteNonQuery();

                Console.WriteLine($"Added MenuItem {menuItemId} x {quantity} to order.");
            }
        }

        public static void ListOrderItems(int orderId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT oi.Id, m.Name, oi.Quantity, m.Price, (oi.Quantity * m.Price) as Total
                FROM OrderItems oi
                JOIN MenuItems m ON oi.MenuItemId = m.Id
                WHERE oi.OrderId = @orderId;";
            cmd.Parameters.AddWithValue("@orderId", orderId);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"=== Items for Order {orderId} ===");
            decimal grandTotal = 0;
            while (reader.Read())
            {
                int orderItemId = reader.GetInt32(0);
                string name = reader.GetString(1);
                int qty = reader.GetInt32(2);
                decimal price = reader.GetDecimal(3);
                decimal total = reader.GetDecimal(4);

                Console.WriteLine($"[{orderItemId}] {name} x {qty} @ ${price:F2} = ${total:F2}");
                grandTotal += total;
            }
            Console.WriteLine($"Total Order Amount: ${grandTotal:F2}");
            Console.WriteLine("=============================");
        }

        public static void ListItemsForTable(int tableId)
        {
            int? orderId = OrderService.GetActiveOrderIdByTableId(tableId);
            if (orderId == null)
            {
                Console.WriteLine($"❌ No active order found for Table {tableId}.");
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT mi.Name, oi.Quantity, mi.Price,
               (oi.Quantity * mi.Price) AS Total
        FROM OrderItems oi
        JOIN MenuItems mi ON oi.MenuItemId = mi.Id
        WHERE oi.OrderId = @orderId;
    ";
            cmd.Parameters.AddWithValue("@orderId", orderId.Value);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"\n🧾 Items for Table {tableId} (Order #{orderId}):");
            Console.WriteLine("----------------------------------------------");

            decimal grandTotal = 0;
            while (reader.Read())
            {
                string itemName = reader.GetString(0);
                int quantity = reader.GetInt32(1);
                decimal price = reader.GetDecimal(2);
                decimal total = reader.GetDecimal(3);

                Console.WriteLine($"{quantity}x {itemName} @ ${price:F2} = ${total:F2}");
                grandTotal += total;
            }

            Console.WriteLine("----------------------------------------------");
            Console.WriteLine($"Total Amount: ${grandTotal:F2}");
        }

        public static void RemoveOrderItem(int orderItemId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM OrderItems WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", orderItemId);

            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                Console.WriteLine($"Order item {orderItemId} removed.");
            else
                Console.WriteLine($"Order item {orderItemId} not found.");
        }

        public static void UpdateOrderItemQuantity(int orderItemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                Console.WriteLine("Quantity must be greater than zero.");
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE OrderItems SET Quantity = @qty WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@qty", newQuantity);
            cmd.Parameters.AddWithValue("@id", orderItemId);

            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                Console.WriteLine($"Order item {orderItemId} quantity updated to {newQuantity}.");
            else
                Console.WriteLine($"Order item {orderItemId} not found.");
        }
    }
}
