using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;
using Spectre.Console;

namespace UrbanZenith.Commands
{
    public class StaffCommand : ICommand, IMenuCommand
    {
        public string Name => "staff";
        public string Description => "Manage staff members (list, add, remove, info, update)";

        public void Execute(string args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args))
                {
                    ShowHelp();
                    return;
                }

                var parts = args.Split(' ', 2);
                string subcommand = parts[0].ToLower();
                string subArgs = parts.Length > 1 ? parts[1] : string.Empty;

                switch (subcommand)
                {
                    case "list":
                        StaffService.ListStaff();
                        break;

                    case "add":
                        StaffService.AddStaff();
                        break;

                    case "remove":
                        if (int.TryParse(subArgs, out int removeId))
                            StaffService.RemoveStaff(removeId);
                        else
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]staff remove <id>[/]");
                        break;

                    case "info":
                        if (int.TryParse(subArgs, out int infoId))
                            StaffService.GetStaffInfo(infoId);
                        else
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]staff info <id>[/]");
                        break;

                    case "update":
                        if (int.TryParse(subArgs, out int updateId))
                            StaffService.UpdateStaff(updateId);
                        else
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]staff update <id>[/]");
                        break;

                    case "help":
                        ShowHelp();
                        break;

                    default:
                        AnsiConsole.MarkupLine($"[red][[!]] Unknown staff command: '[bold red]{subcommand}[/]'[/]");
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
                    new Rule("[bold yellow]Staff Management Menu[/]")
                        .RuleStyle("grey")
                        .Centered());

                AnsiConsole.MarkupLine("[bold green]1.[/] List all staff");
                AnsiConsole.MarkupLine("[bold green]2.[/] Add new staff member");
                AnsiConsole.MarkupLine("[bold green]3.[/] Remove staff member");
                AnsiConsole.MarkupLine("[bold green]4.[/] View staff info");
                AnsiConsole.MarkupLine("[bold green]5.[/] Update staff details");
                AnsiConsole.MarkupLine("[bold red]0.[/] Back to main menu");

                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold blue]Choose an option:[/]")
                        .ValidationErrorMessage("[red][[!]] Invalid choice. Please enter a number from the list.[/]")
                        .Validate(val =>
                        {
                            if (string.IsNullOrEmpty(val)) return ValidationResult.Error("[red]Choice cannot be empty.[/]");
                            return int.TryParse(val, out int num) && (num >= 0 && num <= 5)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red][[!]] Invalid choice. Please enter 0, 1, 2, 3, 4, or 5.[/]");
                        }));

                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            StaffService.ListStaff();
                            break;
                        case "2":
                            StaffService.AddStaff();
                            break;
                        case "3":
                            int removeId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Staff ID to remove (e.g., 101):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Staff ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Staff ID must be a positive number.[/]")));
                            StaffService.RemoveStaff(removeId);
                            break;
                        case "4":
                            int infoId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Staff ID to view (e.g., 101):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Staff ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Staff ID must be a positive number.[/]")));
                            StaffService.GetStaffInfo(infoId);
                            break;
                        case "5":
                            int updateId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Staff ID to update (e.g., 101):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Staff ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Staff ID must be a positive number.[/]")));
                            StaffService.UpdateStaff(updateId);
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
                new Rule("[bold yellow]Staff Command Usage[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Command[/]"))
                .AddColumn(new TableColumn("[bold blue]Description[/]"));

            table.AddRow("[cyan]staff list[/]", "[white]List all staff members.[/]");
            table.AddRow("[cyan]staff add[/]", "[white]Add a new staff member (prompts for details).[/]");
            table.AddRow("[cyan]staff remove[/] [grey]<id>[/]", "[white]Remove a staff member by ID.[/]");
            table.AddRow("[cyan]staff info[/] [grey]<id>[/]", "[white]View detailed information for a staff member by ID.[/]");
            table.AddRow("[cyan]staff update[/] [grey]<id>[/]", "[white]Update details for a staff member by ID.[/]");
            table.AddRow("[cyan]staff help[/]", "[white]Display this help message.[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}