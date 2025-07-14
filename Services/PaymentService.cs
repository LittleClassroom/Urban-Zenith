using System;
using System.Collections.Generic;
using UrbanZenith.Database;
using System.Data.SQLite;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ConsoleTableExt;
namespace UrbanZenith.Services
{
    public static class PaymentService
    {
        public static void ProcessPaymentWithPrompt(int tableId)
        {
            int? activeOrderId = OrderService.GetActiveOrderIdByTableId(tableId);
            if (activeOrderId == null)
            {
                Console.WriteLine($"No active order found for table {tableId}.");
                return;
            }

            decimal total = OrderService.CalculateTotalByTableId(tableId);
            if (total <= 0)
            {
                Console.WriteLine("Order total is zero or invalid.");
                return;
            }

            Console.WriteLine($"Total due for table {tableId} (Order #{activeOrderId}): {total:C}");

            Console.Write("Enter payment amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal paymentAmount) || paymentAmount <= 0)
            {
                Console.WriteLine("Invalid payment amount.");
                return;
            }

            if (paymentAmount < total)
            {
                Console.WriteLine("Payment is less than total due. Please pay full amount.");
                return;
            }

            if (paymentAmount > total)
            {
                decimal change = paymentAmount - total;
                Console.WriteLine($"Change to return: {change:C}");
            }

            Console.Write("Enter payment method (Cash, Card, QR, E-wallet): ");
            string paymentMethod = Console.ReadLine()?.Trim().ToLower();

            string[] validMethods = { "cash", "card", "qr", "e-wallet" };
            if (string.IsNullOrEmpty(paymentMethod) || Array.IndexOf(validMethods, paymentMethod) == -1)
            {
                Console.WriteLine("Invalid payment method.");
                return;
            }

            Console.Write($"Confirm payment of {paymentAmount:C} using {paymentMethod}? (Y/N): ");
            string confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm != "y")
            {
                Console.WriteLine("Payment cancelled.");
                return;
            }

            // Process
            try
            {
                ProcessPayment(activeOrderId.Value, tableId, paymentAmount, paymentMethod);
                OrderService.CompleteOrder(activeOrderId.Value);
                Console.WriteLine("Payment processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to process payment: {ex.Message}");
            }
        }

        public static void ProcessPayment(int orderId, int tableId, decimal amountPaid, string paymentMethod)
        {
            string sql = @"
                INSERT INTO Payments (OrderId, PaymentMethod, PaidAmount, PaidAt)
                VALUES (@orderId, @paymentMethod, @amountPaid, CURRENT_TIMESTAMP);
            ";

            try
            {
                DatabaseContext.ExecuteNonQuery(sql,
                    new SQLiteParameter("@orderId", orderId),
                    new SQLiteParameter("@paymentMethod", paymentMethod),
                    new SQLiteParameter("@amountPaid", amountPaid)
                );

                Console.WriteLine($"Payment of {amountPaid:C} for Order {orderId} at Table {tableId} recorded successfully using {paymentMethod}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to process payment: {ex.Message}");
            }
        }


        public static void ShowPaymentHistory()
        {
            ShowPaymentHistory(1, 10);
        }

        public static void ShowPaymentHistory(int page)
        {
            ShowPaymentHistory(page, 10);
        }


        public static void ShowPaymentHistory(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            int offset = (page - 1) * pageSize;

            string sql = @"
                SELECT Id, OrderId, PaidAmount, PaidAt
                FROM Payments
                ORDER BY PaidAt DESC
                LIMIT @pageSize OFFSET @offset
            ";

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@pageSize", pageSize),
                new SQLiteParameter("@offset", offset)
            };

            var rows = new List<object[]>();

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    rows.Add(new object[]
                    {
                        reader["Id"],
                        $"{Convert.ToDecimal(reader["PaidAmount"]):C}",
                        reader["PaidAt"]
                    });
                }
            }, parameters.ToArray());

            if (rows.Count == 0)
            {
                Console.WriteLine(page == 1 ? "No payment records found." : "No more payment records.");
                return;
            }

            Console.Clear();

            ConsoleTableBuilder
                .From(rows)
                .WithTitle($"Payment History - Page {page}", ConsoleColor.Yellow, ConsoleColor.DarkBlue)
                .WithColumn("ID", "Amount", "Paid At")
                .WithFormat(ConsoleTableBuilderFormat.MarkDown)
                .ExportAndWriteLine();
            
            Console.WriteLine("\n\nhint: payment history < page >");

        }




        public static void ShowPaymentDetail(int paymentId)
        {
            string sql = @"
                SELECT p.Id, p.OrderId, p.PaymentMethod, p.PaidAmount, p.PaidAt,
                        o.TableId, t.Name AS TableName,t.type as TableType, o.OrderDate, o.Status
                FROM Payments p
                LEFT JOIN Orders o ON p.OrderId = o.Id
                LEFT JOIN Tables t ON o.TableId = t.Id
                WHERE p.Id = @paymentId
            ";

            var parameters = new[]
            {
                new SQLiteParameter("@paymentId", paymentId)
            };

            bool found = false;

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                if (reader.Read())
                {
                    found = true;
                    Console.WriteLine("──────────── Payment Details ────────────");
                    Console.WriteLine($"| Payment ID   | {reader["Id"]}");
                    Console.WriteLine($"| Order ID     | {reader["OrderId"]}");
                    Console.WriteLine($"| Table ID     | {reader["TableId"]}");
                    Console.WriteLine($"| Table Name   | {reader["TableName"]}");
                    Console.WriteLine($"| Table Type   | {reader["TableType"]}");
                    Console.WriteLine($"| Order Status | {reader["Status"]}");
                    Console.WriteLine($"| Method       | {reader["PaymentMethod"]}");
                    Console.WriteLine($"| Amount Paid  | {Convert.ToDecimal(reader["PaidAmount"]):C}");
                    Console.WriteLine($"| Order Date   | {reader["OrderDate"]}");
                    Console.WriteLine($"| Paid At      | {reader["PaidAt"]}");
                    Console.WriteLine("────────────────────────────────────────────");
                }
            }, parameters);

            if (!found)
            {
                Console.WriteLine($"❌ Payment with ID {paymentId} not found.");
            }
        }

    }
}


