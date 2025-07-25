using System;
using System.Data.SQLite;
using UrbanZenith.Database;
using Spectre.Console;
using System.Linq;

namespace UrbanZenith.Services
{
    public static class TableService
    {
        public static void ListTables()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]Restaurant Tables[/]").RuleStyle("grey").Centered());

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.Id, t.Name, t.Type, t.Status, s.Name as StaffName
                FROM Tables t
                LEFT JOIN Staff s ON t.StaffId = s.Id
                ORDER BY t.Id;";

            using var reader = cmd.ExecuteReader();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .AddColumn(new TableColumn("[bold green]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Name[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Type[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Status[/]").Centered())
                .AddColumn(new TableColumn("[bold green]Assigned Staff[/]").Centered());

            bool hasRows = false;
            while (reader.Read())
            {
                hasRows = true;
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string type = reader.IsDBNull(2) ? "Standard" : reader.GetString(2);
                string status = reader.IsDBNull(3) ? "Unoccupied" : reader.GetString(3);
                string staffName = reader.IsDBNull(4) ? "[grey]N/A[/]" : reader.GetString(4);

                string statusMarkup = status switch
                {
                    "Occupied" => $"[red]{status}[/]",
                    "Available" => $"[green]{status}[/]",
                    "Unoccupied" => $"[green]{status}[/]",
                    "Broken" => $"[orange3]{status}[/]",
                    _ => status
                };

                table.AddRow(
                    new Markup($"[white]{id:D2}[/]"),
                    new Markup($"[cyan]{name}[/]"),
                    new Markup($"[blue]{type}[/]"),
                    new Markup(statusMarkup),
                    new Markup($"[yellow]{staffName}[/]")
                );
            }

            if (!hasRows)
            {
                AnsiConsole.MarkupLine("[red][[!]] No tables found in the system.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void ListAvailableTables()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]Available Tables[/]").RuleStyle("grey").Centered());

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Name, Type FROM Tables
                WHERE Status = 'Available'
                ORDER BY Id;";

            using var reader = cmd.ExecuteReader();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]ID[/]").Centered())
                .AddColumn(new TableColumn("[bold blue]Name[/]").Centered())
                .AddColumn(new TableColumn("[bold blue]Type[/]").Centered());

            bool hasRows = false;
            while (reader.Read())
            {
                hasRows = true;
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string type = reader.IsDBNull(2) ? "Standard" : reader.GetString(2);

                table.AddRow(
                    new Markup($"[white]{id:D2}[/]"),
                    new Markup($"[cyan]{name}[/]"),
                    new Markup($"[blue]{type}[/]")
                );
            }

            if (!hasRows)
            {
                AnsiConsole.MarkupLine("[red][[!]] No available tables at the moment.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void AddTable()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold yellow]Add New Table[/]").RuleStyle("grey").Centered());

            string name = AnsiConsole.Ask<string>("[bold green]Enter Table Name (e.g., Table 1):[/]");
            if (string.IsNullOrEmpty(name?.Trim()))
            {
                AnsiConsole.MarkupLine("[red]Table name cannot be empty.[/]");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            string type = "";
            bool isCorrect = false;

            while (!isCorrect)
            {
                AnsiConsole.MarkupLine("[bold green]Please Choose Table Type:[/]");
                AnsiConsole.MarkupLine("[blue][[1]][/]: Standard");
                AnsiConsole.MarkupLine("[blue][[2]][/]: VIP");
                AnsiConsole.MarkupLine("[blue][[3]][/]: Outdoor");
                string input = AnsiConsole.Ask<string>("[bold green]Enter Table Type (1-3):[/]").Trim();

                if (string.IsNullOrEmpty(input))
                {
                    type = "Standard";
                    isCorrect = true;
                }
                else
                {
                    switch (input)
                    {
                        case "1":
                            type = "Standard";
                            isCorrect = true;
                            break;
                        case "2":
                            type = "VIP";
                            isCorrect = true;
                            break;
                        case "3":
                            type = "Outdoor";
                            isCorrect = true;
                            break;
                        default:
                            AnsiConsole.MarkupLine("[red]Invalid input, please enter 1, 2, or 3.[/]");
                            isCorrect = false;
                            break;
                    }
                }
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Tables (Name, Type, Status) VALUES (@name, @type, 'Available');";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@type", type);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(rows > 0 ? "[bold green]✅ Table added successfully.[/]" : "[bold red]❌ Failed to add table.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void RemoveTable(int id)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Remove Table #{id}[/]").RuleStyle("grey").Centered());

            bool confirm = AnsiConsole.Confirm($"[red]Are you sure you want to remove Table [bold red]#{id}[/]? This action cannot be undone.[/]");
            if (!confirm)
            {
                AnsiConsole.MarkupLine("[grey]Table removal cancelled.[/]");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Tables WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(rows > 0 ? "[bold green]✅ Table removed.[/]" : "[bold red]❌ Table not found or removal failed.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void ResetTable(int id)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Reset Table #{id}[/]").RuleStyle("grey").Centered());

            bool confirm = AnsiConsole.Confirm($"[yellow]Are you sure you want to reset Table [bold yellow]#{id}[/] (status to Unoccupied, unassign staff)?[/]");
            if (!confirm)
            {
                AnsiConsole.MarkupLine("[grey]Table reset cancelled.[/]");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Tables SET Status = 'Unoccupied', StaffId = NULL WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(rows > 0 ? "[bold green]✅ Table reset successfully.[/]" : "[bold red]❌ Table not found or reset failed.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void UpdateTableStatus(int id, string newStatus)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Update Table #{id} Status[/]").RuleStyle("grey").Centered());

            if (!new string[] { "Occupied", "Unoccupied", "Broken" }.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[red]Invalid status. Allowed: Occupied, Unoccupied, Broken.[/]");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Tables SET Status = @status WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@status", newStatus);
            cmd.Parameters.AddWithValue("@id", id);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(rows > 0 ? "[bold green]✅ Table status updated.[/]" : "[bold red]❌ Table not found or update failed.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void AssignStaff(int tableId, int staffId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Assign Staff to Table #{tableId}[/]").RuleStyle("grey").Centered());

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = "UPDATE Tables SET StaffId = @staffId WHERE Id = @tableId;";
            cmd.Parameters.AddWithValue("@staffId", staffId);
            cmd.Parameters.AddWithValue("@tableId", tableId);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(rows > 0 ? "[bold green]✅ Staff assigned to table.[/]" : "[bold red]❌ Table or Staff not found.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void UnassignStaff(int tableId)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Unassign Staff from Table #{tableId}[/]").RuleStyle("grey").Centered());

            bool confirm = AnsiConsole.Confirm($"[yellow]Are you sure you want to unassign staff from Table [bold yellow]#{tableId}[/]?[/]");
            if (!confirm)
            {
                AnsiConsole.MarkupLine("[grey]Staff unassignment cancelled.[/]");
                AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                Console.ReadLine();
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Tables
                SET StaffId = NULL
                WHERE Id = @tableId;
            ";
            cmd.Parameters.AddWithValue("@tableId", tableId);

            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    AnsiConsole.MarkupLine($"[bold green]✅ Staff unassigned from table {tableId} successfully.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[bold red]❌ Table with ID {tableId} not found or no staff assigned.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }

        public static void UpdateTable(int id)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold yellow]Update Table #{id}[/]").RuleStyle("grey").Centered());

            AnsiConsole.MarkupLine("[bold white]Updating table info. Leave input empty to keep current value.[/]");

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            string currentName = "";
            string currentType = "";
            string currentStatus = "";
            int? currentStaffId = null;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Name, Type, Status, StaffId FROM Tables WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    currentName = reader.GetString(0);
                    currentType = reader.IsDBNull(1) ? "Standard" : reader.GetString(1);
                    currentStatus = reader.IsDBNull(2) ? "Unoccupied" : reader.GetString(2);
                    currentStaffId = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]Table not found.[/]");
                    AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
                    Console.ReadLine();
                    return;
                }
            }

            string name = AnsiConsole.Ask<string>($"[bold green]Name[/] ([grey]Current: {currentName}[/]):", currentName).Trim();
            if (string.IsNullOrEmpty(name)) name = currentName;

            string type = AnsiConsole.Ask<string>($"[bold green]Type[/] ([grey]Current: {currentType}[/], options: Standard, VIP, Outdoor):", currentType).Trim();
            if (string.IsNullOrEmpty(type)) type = currentType;
            else if (!new string[] { "Standard", "VIP", "Outdoor" }.Contains(type, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[red]Invalid type provided. Keeping previous value.[/]");
                type = currentType;
            }

            string status = AnsiConsole.Ask<string>($"[bold green]Status[/] ([grey]Current: {currentStatus}[/], options: Occupied, Unoccupied, Broken):", currentStatus).Trim();
            if (string.IsNullOrEmpty(status)) status = currentStatus;
            else if (!new string[] { "Occupied", "Unoccupied", "Broken" }.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[red]Invalid status provided. Keeping previous value.[/]");
                status = currentStatus;
            }

            string staffInput = AnsiConsole.Ask<string>($"[bold green]Staff ID[/] ([grey]Current: {(currentStaffId.HasValue ? currentStaffId.ToString() : "None")}, enter 0 to unassign[/]):", currentStaffId.HasValue ? currentStaffId.ToString() : "").Trim();
            int? staffId = currentStaffId;

            if (!string.IsNullOrEmpty(staffInput))
            {
                if (int.TryParse(staffInput, out int sid))
                {
                    if (sid == 0)
                        staffId = null;
                    else
                        staffId = sid;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid staff ID input. Keeping previous value.[/]");
                }
            }

            using var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Tables
                SET Name = @name, Type = @type, Status = @status, StaffId = @staffId
                WHERE Id = @id;";
            updateCmd.Parameters.AddWithValue("@name", name);
            updateCmd.Parameters.AddWithValue("@type", type);
            updateCmd.Parameters.AddWithValue("@status", status);
            if (staffId.HasValue)
                updateCmd.Parameters.AddWithValue("@staffId", staffId.Value);
            else
                updateCmd.Parameters.AddWithValue("@staffId", DBNull.Value);
            updateCmd.Parameters.AddWithValue("@id", id);

            try
            {
                int rows = updateCmd.ExecuteNonQuery();
                AnsiConsole.MarkupLine(rows > 0 ? "[bold green]✅ Table updated successfully.[/]" : "[bold red]❌ Failed to update table.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]An error occurred: {ex.Message}[/]");
            }

            AnsiConsole.MarkupLine("[grey]Press Enter to continue...[/]");
            Console.ReadLine();
        }
    }
}