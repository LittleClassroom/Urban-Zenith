using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UrbanZenith.Database;
using Spectre.Console;

namespace UrbanZenith.Services
{
    public static class ReportService
    {
        public static decimal SafeDecimal(object dbValue)
        {
            return dbValue != DBNull.Value ? Convert.ToDecimal(dbValue) : 0m;
        }

        public static void ShowDailySalesReport(DateTime? date = null)
        {
            AnsiConsole.Clear();
            DateTime targetDate = date ?? DateTime.Today;

            AnsiConsole.Write(
                new Rule($"[bold yellow]Daily Sales Report - {targetDate:yyyy-MM-dd}[/]")
                    .RuleStyle("grey")
                    .Centered());
            AnsiConsole.WriteLine();

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
                    var totalPayments = reader["TotalPayments"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalPayments"]);
                    var totalRevenue = SafeDecimal(reader["TotalRevenue"]);

                    var panel = new Panel(
                        new Markup($"[bold blue]Payments Made:[/] [green]{totalPayments}[/]\n" +
                                 $"[bold blue]Total Revenue:[/] [green]{totalRevenue:C}[/]")
                    )
                    .Header("[bold]Summary[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green);

                    AnsiConsole.Write(panel);

                    if (totalPayments == 0)
                    {
                        AnsiConsole.MarkupLine($"\n[red][!] No sales recorded for {targetDate:yyyy-MM-dd}.[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red][!] Could not retrieve report data for {targetDate:yyyy-MM-dd}.[/]");
                }
            }, parameters);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void ShowSalesByPaymentMethod()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Sales by Payment Method[/]")
                    .RuleStyle("grey")
                    .Centered());
            AnsiConsole.WriteLine();

            string sql = @"
                SELECT PaymentMethod,
                       COUNT(*) AS Transactions,
                       SUM(PaidAmount) AS Total
                FROM Payments
                GROUP BY PaymentMethod
                ORDER BY Total DESC
            ";

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Method[/]").Centered())
                .AddColumn(new TableColumn("[bold blue]Transactions[/]").Centered())
                .AddColumn(new TableColumn("[bold blue]Revenue[/]").RightAligned());

            bool hasData = false;
            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    hasData = true;
                    string method = reader["PaymentMethod"].ToString();
                    int count = Convert.ToInt32(reader["Transactions"]);
                    decimal total = SafeDecimal(reader["Total"]);

                    table.AddRow(
                        new Markup($"[white]{method}[/]"),
                        new Markup($"[cyan]{count}[/]"),
                        new Markup($"[green]{total:C}[/]")
                    );
                }
            });

            if (!hasData)
            {
                AnsiConsole.MarkupLine("[red][!] No sales data available by payment method.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void ShowTopSellingItems()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Top Selling Menu Items[/]")
                    .RuleStyle("grey")
                    .Centered());
            AnsiConsole.WriteLine();

            string sql = @"
                SELECT
                    m.Name AS ItemName,
                    SUM(oi.Quantity) AS QuantitySold,
                    SUM(oi.Quantity * oi.Price) AS TotalRevenue
                FROM OrderItems oi
                JOIN MenuItems m ON m.Id = oi.MenuItemId
                GROUP BY oi.MenuItemId, m.Name
                ORDER BY QuantitySold DESC, TotalRevenue DESC
            ";

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold magenta]Item Name[/]"))
                .AddColumn(new TableColumn("[bold magenta]Quantity Sold[/]").Centered())
                .AddColumn(new TableColumn("[bold magenta]Total Revenue[/]").RightAligned());

            bool hasData = false;
            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    hasData = true;
                    string itemName = reader["ItemName"].ToString();
                    int quantitySold = Convert.ToInt32(reader["QuantitySold"]);
                    decimal totalRevenue = SafeDecimal(reader["TotalRevenue"]);

                    table.AddRow(
                        new Markup($"[white]{itemName}[/]"),
                        new Markup($"[cyan]{quantitySold}[/]"),
                        new Markup($"[green]{totalRevenue:C}[/]")
                    );
                }
            });

            if (!hasData)
            {
                AnsiConsole.MarkupLine("[red][!] No items have been sold yet.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }
    }
}