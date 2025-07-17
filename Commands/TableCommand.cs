using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;
using Spectre.Console;

namespace UrbanZenith.Commands
{
    public class TableCommand : ICommand, IMenuCommand
    {
        public string Name => "table";
        public string Description => "Manage tables (list, available, add, remove, reset, status, assign, unassign, update)";

        public void Execute(string args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args))
                {
                    ShowHelp();
                    return;
                }

                var parts = args.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
                string subcommand = parts[0].ToLower();

                string subArgs = parts.Length > 1 ? string.Join(" ", parts, 1, parts.Length - 1) : string.Empty;

                switch (subcommand)
                {
                    case "list":
                        TableService.ListTables();
                        break;
                    case "available":
                        TableService.ListAvailableTables();
                        break;
                    case "add":
                        TableService.AddTable();
                        break;
                    case "remove":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int removeId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]table remove <id>[/]");
                            return;
                        }
                        TableService.RemoveTable(removeId);
                        break;
                    case "reset":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int resetId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]table reset <id>[/]");
                            return;
                        }
                        TableService.ResetTable(resetId);
                        break;
                    case "status":
                        if (parts.Length < 3 || !int.TryParse(parts[1], out int statusId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]table status <id> <new_status>[/]");
                            return;
                        }
                        string newStatus = parts[2];
                        TableService.UpdateTableStatus(statusId, newStatus);
                        break;
                    case "assign":
                        if (parts.Length < 3 || !int.TryParse(parts[1], out int tableId) || !int.TryParse(parts[2], out int staffId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]table assign <tableId> <staffId>[/]");
                            return;
                        }
                        TableService.AssignStaff(tableId, staffId);
                        break;
                    case "unassign":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int unassignTableId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]table unassign <tableId>[/]");
                            return;
                        }
                        TableService.UnassignStaff(unassignTableId);
                        break;
                    case "update":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int updateId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]table update <id>[/]");
                            return;
                        }
                        TableService.UpdateTable(updateId);
                        break;
                    case "help":
                        ShowHelp();
                        break;
                    default:
                        AnsiConsole.MarkupLine($"[red][[!]] Unknown table command: '[bold red]{subcommand}[/]'[/]");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        public void ShowMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Table Management Menu[/]")
                        .RuleStyle("grey")
                        .Centered());

                AnsiConsole.MarkupLine("[bold green]1.[/] List all tables");
                AnsiConsole.MarkupLine("[bold green]2.[/] List available tables");
                AnsiConsole.MarkupLine("[bold green]3.[/] Add a new table");
                AnsiConsole.MarkupLine("[bold green]4.[/] Remove a table");
                AnsiConsole.MarkupLine("[bold green]5.[/] Reset table status");
                AnsiConsole.MarkupLine("[bold green]6.[/] Update table status");
                AnsiConsole.MarkupLine("[bold green]7.[/] Assign staff to table");
                AnsiConsole.MarkupLine("[bold green]8.[/] Unassign staff from table");
                AnsiConsole.MarkupLine("[bold green]9.[/] Update table info");
                AnsiConsole.MarkupLine("[bold red]0.[/] Back to main menu");
                AnsiConsole.Write(new Rule().RuleStyle("grey"));

                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold blue]Table >[/]")
                        .ValidationErrorMessage("[red][[!]] Invalid choice. Please enter a number from the list.[/]")
                        .Validate(val =>
                        {
                            if (string.IsNullOrEmpty(val)) return ValidationResult.Error("[red]Choice cannot be empty.[/]");
                            return int.TryParse(val, out int num) && (num >= 0 && num <= 9)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red][[!]] Invalid choice. Please enter a number between 0 and 9.[/]");
                        }));

                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            TableService.ListTables();
                            break;
                        case "2":
                            TableService.ListAvailableTables();
                            break;
                        case "3":
                            TableService.AddTable();
                            break;
                        case "4":
                            int removeId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter table ID to remove:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                            TableService.RemoveTable(removeId);
                            break;
                        case "5":
                            int resetId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter table ID to reset:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                            TableService.ResetTable(resetId);
                            break;
                        case "6":
                            int statusId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter table ID:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                            string newStatus = AnsiConsole.Prompt(
                                new TextPrompt<string>("[green]Enter new status:[/]")
                                    .ValidationErrorMessage("[red][[!]] Status cannot be empty.[/]")
                                    .Validate(val => string.IsNullOrWhiteSpace(val) ? ValidationResult.Error("[red]Status cannot be empty.[/]") : ValidationResult.Success()));
                            TableService.UpdateTableStatus(statusId, newStatus);
                            break;
                        case "7":
                            int tableIdAssign = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter table ID:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                            int staffIdAssign = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter staff ID:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid staff ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Staff ID must be a positive number.[/]")));
                            TableService.AssignStaff(tableIdAssign, staffIdAssign);
                            break;
                        case "8":
                            int unassignId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter table ID to unassign staff from:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                            TableService.UnassignStaff(unassignId);
                            break;
                        case "9":
                            int updateId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter table ID to update:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                            TableService.UpdateTable(updateId);
                            break;
                        default:
                            AnsiConsole.MarkupLine("[red][[!]] Invalid option. Please select an option from the menu.[/]");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
                }

                AnsiConsole.MarkupLine("\n[grey]Press Enter to continue...[/]");
                Console.ReadLine();
            }
        }

        private void ShowHelp()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Table Command Usage[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Command[/]"))
                .AddColumn(new TableColumn("[bold blue]Description[/]"));

            table.AddRow("[cyan]table list[/]", "[white]List all tables.[/]");
            table.AddRow("[cyan]table available[/]", "[white]List all available tables.[/]");
            table.AddRow("[cyan]table add[/]", "[white]Add a new table (prompts for details).[/]");
            table.AddRow("[cyan]table remove[/] [grey]<id>[/]", "[white]Remove a table by ID.[/]");
            table.AddRow("[cyan]table reset[/] [grey]<id>[/]", "[white]Reset a table's status to available.[/]");
            table.AddRow("[cyan]table status[/] [grey]<id> <new_status>[/]", "[white]Update a table's status.[/]");
            table.AddRow("[cyan]table assign[/] [grey]<tableId> <staffId>[/]", "[white]Assign a staff member to a table.[/]");
            table.AddRow("[cyan]table unassign[/] [grey]<tableId>[/]", "[white]Unassign staff from a table.[/]");
            table.AddRow("[cyan]table update[/] [grey]<id>[/]", "[white]Update table details (e.g., capacity).[/]");
            table.AddRow("[cyan]table help[/]", "[white]Display this help message.[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}