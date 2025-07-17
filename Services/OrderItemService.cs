using System;
using System.Data.SQLite;
using UrbanZenith.Database;
using UrbanZenith.Models;
using Spectre.Console;

namespace UrbanZenith.Services
{
    public static class OrderItemService
    {
        public static void AddItemsToOrder(int orderId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Adding Items to Order #[blue]{orderId}[/][/]").RuleStyle("grey").Centered());

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            while (true)
            {
                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]Enter Menu Item ID (or 'done' to finish):[/]")
                        .PromptStyle("cyan")
                        .ValidationErrorMessage("[red][[!]] Invalid input. Please enter a number or 'done'.[/]")
                        .Validate(val =>
                        {
                            if (val.Equals("done", StringComparison.OrdinalIgnoreCase)) return ValidationResult.Success();
                            return int.TryParse(val, out _) ? ValidationResult.Success() : ValidationResult.Error("[red]That's not a valid ID.[/]");
                        }));

                if (input.Equals("done", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (!int.TryParse(input, out int menuItemId))
                {
                    AnsiConsole.MarkupLine("[red][!] Invalid ID. Please enter a number.[/]");
                    continue;
                }

                var itemCmd = conn.CreateCommand();
                itemCmd.CommandText = "SELECT Name, Price FROM MenuItems WHERE Id = @id";
                itemCmd.Parameters.AddWithValue("@id", menuItemId);

                using var reader = itemCmd.ExecuteReader();
                if (!reader.Read())
                {
                    AnsiConsole.MarkupLine($"[red][!] Menu item with ID [bold red]{menuItemId}[/] not found.[/]");
                    continue;
                }

                string name = reader.GetString(0);
                decimal price = reader.GetDecimal(1);
                reader.Close();

                AnsiConsole.MarkupLine($"[grey]Selected:[/] [bold cyan]{name}[/] - [lime]${price:F2}[/]");

                int quantity = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green]Enter quantity:[/]")
                        .PromptStyle("cyan")
                        .ValidationErrorMessage("[red][[!]] Invalid quantity. Please enter a positive number.[/]")
                        .Validate(qty => qty > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Quantity must be greater than zero.[/]")));

                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO OrderItems (OrderId, MenuItemId, Quantity, Price)
                    VALUES (@orderId, @menuItemId, @quantity, @price);
                ";
                insertCmd.Parameters.AddWithValue("@orderId", orderId);
                insertCmd.Parameters.AddWithValue("@menuItemId", menuItemId);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@price", price);

                insertCmd.ExecuteNonQuery();

                AnsiConsole.MarkupLine($"[bold green]✔ Added [blue]{quantity}x {name}[/] to Order [blue]#{orderId}[/].[/]");
            }

            AnsiConsole.MarkupLine("[bold green]✅ Finished adding items.[/]");
        }

        private static void AddOrUpdateOrderItem(int orderId, int menuItemId, int quantity)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"
                SELECT Id, Quantity, Price FROM OrderItems
                WHERE OrderId = @orderId AND MenuItemId = @menuItemId;";
            checkCmd.Parameters.AddWithValue("@orderId", orderId);
            checkCmd.Parameters.AddWithValue("@menuItemId", menuItemId);

            using var reader = checkCmd.ExecuteReader();

            if (reader.Read())
            {
                int existingId = reader.GetInt32(0);
                int existingQty = reader.GetInt32(1);
                decimal itemPrice = reader.GetDecimal(2);

                reader.Close();
                var updateCmd = conn.CreateCommand();
                updateCmd.CommandText = @"
                    UPDATE OrderItems SET Quantity = @qty WHERE Id = @id;";
                updateCmd.Parameters.AddWithValue("@qty", existingQty + quantity);
                updateCmd.Parameters.AddWithValue("@id", existingId);
                updateCmd.ExecuteNonQuery();

                AnsiConsole.MarkupLine($"[bold green]Updated MenuItem [blue]{menuItemId}[/] quantity to [yellow]{existingQty + quantity}[/].[/]");
            }
            else
            {
                reader.Close();

                var getItemPriceCmd = conn.CreateCommand();
                getItemPriceCmd.CommandText = "SELECT Price FROM MenuItems WHERE Id = @menuItemId";
                getItemPriceCmd.Parameters.AddWithValue("@menuItemId", menuItemId);
                var priceObj = getItemPriceCmd.ExecuteScalar();
                if (priceObj == null)
                {
                    AnsiConsole.MarkupLine($"[red][!] Menu item with ID [bold red]{menuItemId}[/] not found, cannot add to order.[/]");
                    return;
                }
                decimal price = Convert.ToDecimal(priceObj);

                var insertCmd = conn.CreateCommand();
                insertCmd.CommandText = @"
                    INSERT INTO OrderItems (OrderId, MenuItemId, Quantity, Price)
                    VALUES (@orderId, @menuItemId, @quantity, @price);";
                insertCmd.Parameters.AddWithValue("@orderId", orderId);
                insertCmd.Parameters.AddWithValue("@menuItemId", menuItemId);
                insertCmd.Parameters.AddWithValue("@quantity", quantity);
                insertCmd.Parameters.AddWithValue("@price", price);
                insertCmd.ExecuteNonQuery();

                AnsiConsole.MarkupLine($"[bold green]Added MenuItem [blue]{menuItemId}[/] x [yellow]{quantity}[/] to order.[/]");
            }
        }

        public static void ListOrderItems(int orderId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Items for Order #[blue]{orderId}[/][/]").RuleStyle("grey").Centered());

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT oi.Id, m.Name, oi.Quantity, oi.Price, (oi.Quantity * oi.Price) as Total
                FROM OrderItems oi
                JOIN MenuItems m ON oi.MenuItemId = m.Id
                WHERE oi.OrderId = @orderId;
            ";
            cmd.Parameters.AddWithValue("@orderId", orderId);

            using var reader = cmd.ExecuteReader();

            var table = new Spectre.Console.Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold green]Order Item ID[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Item Name[/]"))
                .AddColumn(new TableColumn("[bold green]Quantity[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Price/Unit[/]").RightAligned())
                .AddColumn(new TableColumn("[bold green]Total[/]").RightAligned());

            decimal grandTotal = 0;
            if (!reader.HasRows)
            {
                AnsiConsole.MarkupLine($"[red][!] No items found for Order [bold red]#{orderId}[/].[/]");
                return;
            }

            while (reader.Read())
            {
                int orderItemId = reader.GetInt32(0);
                string name = reader.GetString(1);
                int qty = reader.GetInt32(2);
                decimal price = reader.GetDecimal(3);
                decimal total = reader.GetDecimal(4);

                table.AddRow(
                    new Markup($"[white]{orderItemId}[/]"),
                    new Markup($"[cyan]{name}[/]"),
                    new Markup($"[yellow]{qty}[/]"),
                    new Markup($"[lime]${price:F2}[/]"),
                    new Markup($"[lime]${total:F2}[/]")
                );
                grandTotal += total;
            }
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[bold white]Grand Total:[/] [lime]${grandTotal:F2}[/]");
        }

        public static void ListItemsForTable(int tableId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Items for Table #[blue]{tableId}[/][/]").RuleStyle("grey").Centered());

            int? orderId = OrderService.GetActiveOrderIdByTableId(tableId);
            if (orderId == null)
            {
                AnsiConsole.MarkupLine($"[red]❌ No active order found for Table [bold red]{tableId}[/].[/]");
                return;
            }

            AnsiConsole.MarkupLine($"[grey]Displaying items for Order #[blue]{orderId}[/] linked to Table #[blue]{tableId}[/].[/]");

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT mi.Name, oi.Quantity, oi.Price,
                       (oi.Quantity * oi.Price) AS Total
                FROM OrderItems oi
                JOIN MenuItems mi ON oi.MenuItemId = mi.Id
                WHERE oi.OrderId = @orderId;
            ";
            cmd.Parameters.AddWithValue("@orderId", orderId.Value);

            using var reader = cmd.ExecuteReader();

            var table = new Spectre.Console.Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .AddColumn(new TableColumn("[bold blue]Item Name[/]"))
                .AddColumn(new TableColumn("[bold blue]Quantity[/]").Centered())
                .AddColumn(new TableColumn("[bold blue]Price/Unit[/]").RightAligned())
                .AddColumn(new TableColumn("[bold blue]Total[/]").RightAligned());

            decimal grandTotal = 0;
            if (!reader.HasRows)
            {
                AnsiConsole.MarkupLine($"[red][!] No items found for Table [bold red]{tableId}[/] in Order [bold red]#{orderId}[/].[/]");
                return;
            }

            while (reader.Read())
            {
                string itemName = reader.GetString(0);
                int quantity = reader.GetInt32(1);
                decimal price = reader.GetDecimal(2);
                decimal total = reader.GetDecimal(3);

                table.AddRow(
                    new Markup($"[cyan]{itemName}[/]"),
                    new Markup($"[yellow]{quantity}[/]"),
                    new Markup($"[lime]${price:F2}[/]"),
                    new Markup($"[lime]${total:F2}[/]")
                );
                grandTotal += total;
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[bold white]Total Order Amount for Table [blue]{tableId}[/]:[/] [lime]${grandTotal:F2}[/]");
        }

        public static void RemoveOrderItem(int orderItemId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Remove Order Item (ID: [blue]{orderItemId}[/])[/]").RuleStyle("grey").Centered());

            try
            {
                using var conn = DatabaseContext.GetConnection();
                conn.Open();

                var confirm = AnsiConsole.Confirm($"[yellow]Are you sure you want to remove order item [bold blue]{orderItemId}[/]?[/]");
                if (!confirm)
                {
                    AnsiConsole.MarkupLine("[grey]Operation cancelled.[/]");
                    return;
                }

                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM OrderItems WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", orderItemId);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    AnsiConsole.MarkupLine($"[bold green]🗑️ Order item [bold blue]{orderItemId}[/] removed successfully.[/]");
                else
                    AnsiConsole.MarkupLine($"[bold red]⚠️ Order item [bold red]{orderItemId}[/] not found.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        public static void UpdateOrderItemQuantity(int orderItemId, int newQuantity)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Update Order Item Quantity (ID: [blue]{orderItemId}[/])[/]").RuleStyle("grey").Centered());

            if (newQuantity <= 0)
            {
                AnsiConsole.MarkupLine("[red][!] Quantity must be greater than zero. If you wish to remove the item, use the remove command.[/]");
                return;
            }

            try
            {
                using var conn = DatabaseContext.GetConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE OrderItems SET Quantity = @qty WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@qty", newQuantity);
                cmd.Parameters.AddWithValue("@id", orderItemId);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    AnsiConsole.MarkupLine($"[bold green]✅ Order item [bold blue]{orderItemId}[/] quantity updated to [yellow]{newQuantity}[/].[/]");
                else
                    AnsiConsole.MarkupLine($"[bold red]⚠️ Order item [bold red]{orderItemId}[/] not found.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }
    }
}