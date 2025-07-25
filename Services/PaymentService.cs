using System;
using System.Collections.Generic;
using UrbanZenith.Database;
using System.Data.SQLite;
using Spectre.Console;

namespace UrbanZenith.Services
{
    public static class PaymentService
    {
        public static void ProcessPaymentWithPrompt(int tableId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold][yellow]Process Payment for Table #{tableId}[/][/]") // bold then yellow
                .RuleStyle("grey")
                .Centered());

            int? activeOrderId = OrderService.GetActiveOrderIdByTableId(tableId);
            if (activeOrderId == null)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] No active order found for table [bold red]{tableId}[/].");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            decimal total = OrderService.CalculateTotalByTableId(tableId);
            if (total <= 0)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Order total is zero or invalid. Cannot process payment.");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            AnsiConsole.MarkupLine($"[bold white]Total due for Table [/][blue]#{tableId}[/][bold white] (Order [/][blue]#{activeOrderId}[/][bold white]): [/][green]{total:C}[/]");
            AnsiConsole.WriteLine();

            decimal paymentAmount = AnsiConsole.Prompt(
                new TextPrompt<decimal>("[bold green]Enter payment amount:[/]")
                    .PromptStyle("cyan")
                    .ValidationErrorMessage("[red][!] Invalid payment amount. Please enter a positive number.[/]")
                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Payment amount must be a positive number.[/]"))
            );

            if (paymentAmount < total)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Payment of [yellow]{paymentAmount:C}[/] is less than total due [green]{total:C}[/]. Please pay the full amount.");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            decimal change = 0m;
            if (paymentAmount > total)
            {
                change = paymentAmount - total;
                AnsiConsole.MarkupLine($"[bold green]Change to return:[/][cyan]{change:C}[/]");
            }
            AnsiConsole.WriteLine();

            string paymentMethod = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Select payment method:[/]")
                    .PageSize(5)
                    .MoreChoicesText("[grey](Move up and down to reveal more payment methods)[/]")
                    .AddChoices(new[] { "Cash", "Card", "QR", "E-wallet" })
                    .HighlightStyle(new Style(Color.Yellow, Color.Black, Decoration.None))
            );

            bool confirm = AnsiConsole.Confirm($"[yellow]Confirm payment of [/][bold green]{paymentAmount:C}[/] using [bold cyan]{paymentMethod}[/]?");

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[grey]Payment cancelled.[/]");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            try
            {
                ProcessPayment(activeOrderId.Value, tableId, paymentAmount, paymentMethod);
                OrderService.CompleteOrder(activeOrderId.Value);
                AnsiConsole.MarkupLine("[bold green]✅ Payment processed successfully.[/]"); // Added missing [/] here for bold green
                if (change > 0)
                {
                    AnsiConsole.MarkupLine($"[bold green]Don't forget to return change: [/][cyan]{change:C}[/]");

                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]Failed to process payment: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
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

                AnsiConsole.MarkupLine($"[green]Payment of [/][bold green]{amountPaid:C}[/][green] for Order [/][blue]#{orderId}[/][green] at Table [/][blue]#{tableId}[/][green] recorded successfully using [/][bold cyan]{paymentMethod}[/].");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]Failed to record payment in database: {ex.Message}[/]");
                throw;
            }
        }

        private static int GetTotalPaymentCount()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Payments;";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static void ShowPaymentHistory(int page = 1, int pageSize = 10)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]Payment History[/]").RuleStyle("grey").Centered());

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            int totalPayments = GetTotalPaymentCount();
            int totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            int offset = (page - 1) * pageSize;

            AnsiConsole.MarkupLine($"[grey]Showing payment history page [bold blue]{page}[/] of [bold blue]{totalPages}[/] ([bold yellow]{totalPayments}[/] total payments).[/]");
            AnsiConsole.WriteLine();

            string sql = @"
                SELECT Id, OrderId, PaymentMethod, PaidAmount, PaidAt
                FROM Payments
                ORDER BY PaidAt DESC
                LIMIT @pageSize OFFSET @offset;
            ";

            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@pageSize", pageSize),
                new SQLiteParameter("@offset", offset)
            };

            var paymentTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .AddColumn(new TableColumn("[bold green]Payment ID[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Order ID[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Method[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Amount Paid[/]").RightAligned())
                .AddColumn(new TableColumn("[bold green]Paid At[/]").Centered());

            bool hasRows = false;
            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                while (reader.Read())
                {
                    hasRows = true;
                    paymentTable.AddRow(
                        new Markup($"[white]P-{reader.GetInt32(0):D3}[/]"),
                        new Markup($"[cyan]O-{reader.GetInt32(1):D3}[/]"),
                        new Markup($"[yellow]{reader.GetString(2)}[/]"),
                        new Markup($"[green]{Convert.ToDecimal(reader["PaidAmount"]):C}[/]"),
                        new Markup($"[grey]{reader.GetDateTime(4):yyyy-MM-dd HH:mm:ss}[/]")
                    );
                }
            }, parameters.ToArray());

            if (!hasRows)
            {
                AnsiConsole.MarkupLine(page == 1 ? "[red][!] No payment records found in the system.[/]" : "[red][!] No more payment records on this page.[/]");
            }
            else
            {
                AnsiConsole.Write(paymentTable);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]Pages: [bold blue]{page}[/] / [bold blue]{totalPages}[/] (Use 'payment history <page_number>' to navigate)[/]");
            AnsiConsole.Write(new Rule().RuleStyle("grey"));
            AnsiConsole.WriteLine();
        }

        public static void ShowPaymentDetail(int paymentId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Payment Details for ID #{paymentId}[/]").RuleStyle("grey").Centered());

            string sql = @"
                SELECT p.Id, p.OrderId, p.PaymentMethod, p.PaidAmount, p.PaidAt,
                       o.TableId, t.Name AS TableName, t.Type AS TableType, o.OrderDate, o.Status
                FROM Payments p
                LEFT JOIN Orders o ON p.OrderId = o.Id
                LEFT JOIN Tables t ON o.TableId = t.Id
                WHERE p.Id = @paymentId
            ";

            var parameters = new[]
            {
                new SQLiteParameter("@paymentId", paymentId)
            };

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Detail[/]"))
                .AddColumn(new TableColumn("[bold blue]Value[/]"));

            bool found = false;

            DatabaseContext.ExecuteQuery(sql, reader =>
            {
                if (reader.Read())
                {
                    found = true;
                    table.AddRow(new Markup("[cyan]Payment ID[/]"), new Markup($"[white]P-{reader["Id"]:D3}[/]"));
                    table.AddRow(new Markup("[cyan]Order ID[/]"), new Markup($"[white]O-{reader["OrderId"]:D3}[/]"));
                    table.AddRow(new Markup("[cyan]Payment Method[/]"), new Markup($"[yellow]{reader["PaymentMethod"]}[/]"));
                    table.AddRow(new Markup("[cyan]Amount Paid[/]"), new Markup($"[green]{Convert.ToDecimal(reader["PaidAmount"]):C}[/]"));
                    table.AddRow(new Markup("[cyan]Paid At[/]"), new Markup($"[grey]{reader["PaidAt"]}[/]"));
                    // Added a rule as a separator for better visual grouping
                    table.AddRow(new Rule().RuleStyle("dim").NoBorder(), new Rule().RuleStyle("dim").NoBorder());

                    table.AddRow(new Markup("[cyan]Table ID[/]"), new Markup($"[white]{(reader["TableId"] == DBNull.Value ? "N/A" : reader["TableId"])}[/]"));
                    table.AddRow(new Markup("[cyan]Table Name[/]"), new Markup($"[white]{(reader["TableName"] == DBNull.Value ? "N/A" : reader["TableName"])}[/]"));
                    table.AddRow(new Markup("[cyan]Table Type[/]"), new Markup($"[white]{(reader["TableType"] == DBNull.Value ? "N/A" : reader["TableType"])}[/]"));
                    table.AddRow(new Markup("[cyan]Order Date[/]"), new Markup($"[grey]{(reader["OrderDate"] == DBNull.Value ? "N/A" : reader["OrderDate"])}[/]"));
                    table.AddRow(new Markup("[cyan]Order Status[/]"), new Markup($"[yellow]{(reader["Status"] == DBNull.Value ? "N/A" : reader["Status"])}[/]"));
                }
            }, parameters);

            if (!found)
            {
                AnsiConsole.MarkupLine($"[red][!] Payment with ID [bold red]{paymentId}[/] not found.[/]");
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