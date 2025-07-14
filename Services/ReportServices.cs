using ConsoleTableExt;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UrbanZenith.Database;

namespace UrbanZenith.Services
{
    public static class ReportService
    {
        // 1. Daily Sales Summary
        public static void ShowDailySalesReport(DateTime? date = null)
        {
            DateTime targetDate = date ?? DateTime.Today;

            string sql = @"
                SELECT COUNT(*) AS TotalPayments,
                       SUM(PaidAmount) AS TotalRevenue
                FROM Payments
                WHERE DATE(PaidAt) = @date
            ";

            var parameters = new[]
            {
                new SQLiteParameter("@date", targetDate.ToString("yyyy-MM-dd"))
            };

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                if (reader.Read())
                {
                    Console.WriteLine("─────── Daily Sales Report ───────");
                    Console.WriteLine($"Date           : {targetDate:yyyy-MM-dd}");
                    Console.WriteLine($"Payments Made  : {reader["TotalPayments"]}");
                    Console.WriteLine($"Total Revenue  : {Convert.ToDecimal(reader["TotalRevenue"]):C}");
                    Console.WriteLine("──────────────────────────────────");
                }
            }, parameters);
        }

        // 2. Sales by Payment Method
        public static void ShowSalesByPaymentMethod()
        {
            string sql = @"
                SELECT PaymentMethod,
                       COUNT(*) AS Transactions,
                       SUM(PaidAmount) AS Total
                FROM Payments
                GROUP BY PaymentMethod
                ORDER BY Total DESC
            ";

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                Console.WriteLine("────── Sales by Payment Method ──────");
                Console.WriteLine("Method       | Count | Revenue");
                Console.WriteLine("--------------------------------------");

                while (reader.Read())
                {
                    string method = reader["PaymentMethod"].ToString();
                    int count = Convert.ToInt32(reader["Transactions"]);
                    decimal total = Convert.ToDecimal(reader["Total"]);

                    Console.WriteLine($"{method,-12} | {count,5} | {total,8:C}");
                }

                Console.WriteLine("--------------------------------------");
            });
        }

        public static void ShowTopSellingItems()
        {
            string sql = @"
                SELECT 
                    m.Name AS ItemName,
                    SUM(oi.Quantity) AS QuantitySold,
                    SUM(oi.Quantity * oi.Price) AS TotalRevenue
                FROM OrderItems oi
                JOIN MenuItems m ON m.Id = oi.MenuItemId
                GROUP BY oi.MenuItemId
                ORDER BY QuantitySold DESC
    ";

            var rows = new List<object[]>();

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    rows.Add(new object[]
                    {
                reader["ItemName"],
                reader["QuantitySold"],
                $"{Convert.ToDecimal(reader["TotalRevenue"]):C}"
                    });
                }
            });

            if (rows.Count == 0)
            {
                Console.WriteLine("No items sold yet.");
                return;
            }

            ConsoleTableBuilder
                .From(rows)
                .WithTitle("Top Selling Menu Items", ConsoleColor.Green, ConsoleColor.Black)
                .WithColumn("Item", "Quantity Sold", "Total Revenue")
                .ExportAndWriteLine();
        }


        // Add more methods like TopSellingItems, RevenueByTable, etc.
    }
}
