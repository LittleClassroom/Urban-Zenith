using System;
using System.Collections.Generic;
using System.Data.SQLite;
using UrbanZenith.Database;
using UrbanZenith.Models;
using Spectre.Console;

namespace UrbanZenith.Services
{
    public static class MenuService
    {

        public static void ListMenuItems(int page = 1)
        {
            AnsiConsole.Clear();
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

            var table = new Spectre.Console.Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .Title($"[bold yellow]Menu Items - Page {page}/{totalPages}[/]")
                .AddColumn(new TableColumn("[bold green]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Name[/]"))
                .AddColumn(new TableColumn("[bold green]Price[/]").RightAligned());

            using var reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                AnsiConsole.MarkupLine($"[red][[!]] No menu items found on page {page}.[/]");
                return;
            }

            while (reader.Read())
            {
                table.AddRow(
                    new Markup($"[white]{reader["Id"]}[/]"),
                    new Markup($"[cyan]{reader["Name"]}[/]"),
                    new Markup($"[lime]$[bold]{Convert.ToDecimal(reader["Price"]):F2}[/][/]")
                );
            }

            AnsiConsole.Write(table);
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
            AnsiConsole.Clear();
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
                    string description = reader.IsDBNull(2) ? "No description provided." : reader.GetString(2);
                    decimal price = reader.GetDecimal(3);

                    var itemGrid = new Grid()
                        .AddColumn(new GridColumn().NoWrap().PadRight(2).Width(15))
                        .AddColumn()
                        .AddRow(new Markup("[blue]ID         :[/]", new Style(foreground: Color.Blue)), new Markup($"[yellow]I-{id:D3}[/]", new Style(foreground: Color.Yellow)))
                        .AddRow(new Markup("[blue]Name       :[/]", new Style(foreground: Color.Blue)), new Markup($"[cyan]{name}[/]", new Style(foreground: Color.Cyan1)))
                        .AddRow(new Markup("[blue]Description:[/]", new Style(foreground: Color.Blue)), new Markup($"[white]{description}[/]", new Style(foreground: Color.White)))
                        .AddRow(new Markup("[blue]Price      :[/]", new Style(foreground: Color.Blue)), new Markup($"[lime]${price:F2}[/]", new Style(foreground: Color.Lime)));

                    var itemPanel = new Panel(itemGrid)
                        .Header($"[bold green]Item Details (ID: I-{id:D3})[/]")
                        .Border(BoxBorder.Rounded)
                        .BorderColor(Color.Green);

                    AnsiConsole.Write(itemPanel);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red][!] No menu item found with ID [bold red]{itemId}[/].[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        public static void AddMenuItem()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]Add New Menu Item[/]").RuleStyle("grey").Centered());

            string name = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter Name:[/]")
                    .PromptStyle("cyan")
                    .ValidationErrorMessage("[red][[!]] Name cannot be empty and must be at least 2 characters.[/]")
                    .Validate(n => !string.IsNullOrWhiteSpace(n) && n.Length >= 2));

            string description = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter Description (optional):[/]")
                    .PromptStyle("cyan")
                    .AllowEmpty());

            decimal price = AnsiConsole.Prompt(
                new TextPrompt<decimal>("[green]Enter Price:[/]")
                    .PromptStyle("cyan")
                    .ValidationErrorMessage("[red][[!]] Invalid price. Please enter a positive number.[/]")
                    .Validate(p => p > 0));

            try
            {
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
                    AnsiConsole.MarkupLine("[bold green]✅ Menu item added successfully.[/]");
                else
                    AnsiConsole.MarkupLine("[bold red]❌ Failed to add menu item.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        public static void UpdateMenuItem(int id)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Update Menu Item (ID: [blue]I-{id:D3}[/])[/]").RuleStyle("grey").Centered());

            try
            {
                using var conn = DatabaseContext.GetConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Name, Description, Price FROM MenuItems WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    AnsiConsole.MarkupLine($"[red][!] Menu item with ID [bold red]{id}[/] not found.[/]");
                    return;
                }

                string currentName = reader.GetString(0);
                string currentDesc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                decimal currentPrice = reader.GetDecimal(2);
                reader.Close();

                AnsiConsole.MarkupLine($"[grey]Current Name:[/] [blue]{currentName}[/]");
                string newName = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]New Name (leave empty to keep):[/]")
                        .PromptStyle("cyan")
                        .DefaultValue(currentName)
                        .AllowEmpty());

                AnsiConsole.MarkupLine($"[grey]Current Description:[/] [blue]{currentDesc}[/]");
                string newDesc = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]New Description (leave empty to keep):[/]")
                        .PromptStyle("cyan")
                        .DefaultValue(currentDesc)
                        .AllowEmpty());

                AnsiConsole.MarkupLine($"[grey]Current Price:[/] [blue]${currentPrice:F2}[/]");
                decimal newPrice = AnsiConsole.Prompt(
                    new TextPrompt<decimal>("[green]New Price (leave empty to keep):[/]")
                        .PromptStyle("cyan")
                        .DefaultValue(currentPrice)
                        .ValidationErrorMessage("[red][[!]] Invalid price. Please enter a positive number.[/]")
                        .Validate(p => p > 0));


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
                if (rows > 0)
                    AnsiConsole.MarkupLine($"[bold green]✅ Menu item [blue]I-{id:D3}[/] updated successfully.[/]");
                else
                    AnsiConsole.MarkupLine("[bold red]❌ Failed to update menu item.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        public static void RemoveMenuItem(int id)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Remove Menu Item (ID: [blue]I-{id:D3}[/])[/]").RuleStyle("grey").Centered());

            try
            {
                using var conn = DatabaseContext.GetConnection();
                conn.Open();

                var confirm = AnsiConsole.Confirm($"[yellow]Are you sure you want to remove menu item [bold blue]I-{id:D3}[/]?[/]");
                if (!confirm)
                {
                    AnsiConsole.MarkupLine("[grey]Operation cancelled.[/]");
                    return;
                }

                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM MenuItems WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    AnsiConsole.MarkupLine($"[bold green]🗑️ Menu item [bold blue]I-{id:D3}[/] removed successfully.[/]");
                else
                    AnsiConsole.MarkupLine($"[bold red]⚠️ Menu item [bold red]I-{id}[/] not found.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }
    }
}