using ConsoleTableExt;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Spectre.Console;
using UrbanZenith.Database;

namespace UrbanZenith.Services
{
    public static class StaffService
    {
        public static void ListStaff()
        {
            AnsiConsole.Clear();
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name FROM Staff ORDER BY Id";

            using var reader = cmd.ExecuteReader();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold yellow]Staff List[/]")
                .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold]Name[/]").LeftAligned());

            bool hasRows = false;

            while (reader.Read())
            {
                hasRows = true;
                table.AddRow(
                    new Markup($"[cyan]S-{reader.GetInt32(0):D2}[/]"),
                    new Markup($"[white]{reader.GetString(1)}[/]")
                );
            }

            if (hasRows)
            {
                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.MarkupLine("[red][!] No staff records found.[/]");
            }
        }


        public static void GetStaffInfo(int id)
        {
            AnsiConsole.Clear();
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Role, Username FROM Staff WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Blue)
                    .Title($"[bold yellow]Staff Information[/]")
                    .AddColumn(new TableColumn("[bold cyan]Field[/]"))
                    .AddColumn(new TableColumn("[bold white]Value[/]"));

                table.AddRow("ID", $"[green]S-{reader.GetInt32(0):D2}[/]");
                table.AddRow("Name", $"[white]{reader.GetString(1)}[/]");
                table.AddRow("Role", $"[yellow]{reader.GetString(2)}[/]");
                table.AddRow("Username", $"[blue]{reader.GetString(3)}[/]");

                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]❌ No staff found with ID [bold]{id}[/].[/]");
            }
        }


        public static void AddStaff()
        {
            string name = AnsiConsole.Ask<string>("[bold green]Enter Name:[/]");
            string role = AnsiConsole.Ask<string>("[bold green]Enter Role:[/]");
            string username = AnsiConsole.Ask<string>("[bold green]Enter Username:[/]");
            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold green]Enter Password:[/]").PromptStyle("yellow").Secret()
            );

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Staff (Name, Role, Username, Password) 
                VALUES (@name, @role, @username, @password)";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    AnsiConsole.MarkupLine("[green] Staff member added successfully.[/]");
                else
                    AnsiConsole.MarkupLine("[red] Failed to add staff member.[/]");
            }
            catch (SQLiteException ex)
            {
                if (ex.ResultCode == SQLiteErrorCode.Constraint)
                    AnsiConsole.MarkupLine("[red]Username already exists.[/]");
                else
                    AnsiConsole.MarkupLine($"[red][[ Database error ]]: {ex.Message}[/]");
            }
        }

        public static void RemoveStaff(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Staff WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
                AnsiConsole.MarkupLine($"[green]✅ Staff member with ID [bold]{id}[/] removed.[/]");
            else
                AnsiConsole.MarkupLine($"[red]❌ No staff found with ID [bold]{id}[/].[/]");
        }

        public static void UpdateStaff(int id)
        {
            AnsiConsole.Clear();
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name, Role, Username FROM Staff WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                AnsiConsole.MarkupLine($"[red]❌ Staff with ID [bold]{id}[/] not found.[/]");
                return;
            }

            string currentName = reader.GetString(0);
            string currentRole = reader.GetString(1);
            string currentUsername = reader.GetString(2);

            AnsiConsole.MarkupLine($"[bold yellow]--- Updating Staff ID {id} ---[/]");
            AnsiConsole.MarkupLine($"[bold]Current Name:[/] {currentName}");
            string newName = AnsiConsole.Ask<string>("New Name (leave blank to keep):").Trim();
            if (string.IsNullOrWhiteSpace(newName)) newName = currentName;

            AnsiConsole.MarkupLine($"[bold]Current Role:[/] {currentRole}");
            string newRole = AnsiConsole.Ask<string>("New Role (Waiter, Cashier, Admin):").Trim();
            if (string.IsNullOrWhiteSpace(newRole)) newRole = currentRole;

            AnsiConsole.MarkupLine($"[bold]Current Username:[/] {currentUsername}");
            string newUsername = AnsiConsole.Ask<string>("New Username (leave blank to keep):").Trim();
            if (string.IsNullOrWhiteSpace(newUsername)) newUsername = currentUsername;

            string updatePwd = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Update password?")
                    .AddChoices("yes", "no")
            );

            string newPassword = null;
            if (updatePwd == "yes")
            {
                newPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("New Password:").PromptStyle("yellow").Secret()
                );
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    AnsiConsole.MarkupLine("[red]❌ Password cannot be empty.[/]");
                    return;
                }
            }

            reader.Close();

            cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Staff 
                SET Name = @name, Role = @role, Username = @username" +
                (newPassword != null ? ", Password = @password" : "") +
                " WHERE Id = @id";

            cmd.Parameters.AddWithValue("@name", newName);
            cmd.Parameters.AddWithValue("@role", newRole);
            cmd.Parameters.AddWithValue("@username", newUsername);
            if (newPassword != null)
                cmd.Parameters.AddWithValue("@password", newPassword);
            cmd.Parameters.AddWithValue("@id", id);

            if (cmd.ExecuteNonQuery() > 0)
                AnsiConsole.MarkupLine("[green]✅ Staff updated.[/]");
            else
                AnsiConsole.MarkupLine("[red]❌ Update failed.[/]");
        }
    }
}
