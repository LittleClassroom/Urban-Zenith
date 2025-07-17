using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;
using Spectre.Console;

namespace UrbanZenith.Commands
{
    public class ItemMenuCommand : ICommand, IMenuCommand
    {
        public string Name => "menu";
        public string Description => "Manage menu items (e.g., 'menu add', 'menu view <id>').";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowHelp();
                return;
            }

            var parts = args.Split(' ', 2);
            string subcommand = parts[0].ToLower();
            string subArgs = parts.Length > 1 ? parts[1] : "";

            switch (subcommand)
            {
                case "info":
                    if (int.TryParse(subArgs, out int id))
                        MenuService.InfoMenuItem(id);
                    else
                        AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]menu info <id>[/]");
                    break;

                case "add":
                    MenuService.AddMenuItem();
                    break;

                case "list":
                    int page = 1;
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedPage))
                        page = parsedPage;
                    MenuService.ListMenuItems(page);
                    break;

                case "update":
                    if (int.TryParse(subArgs, out int updateId))
                        MenuService.UpdateMenuItem(updateId);
                    else
                        AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]menu update <id>[/]");
                    break;

                case "remove":
                    if (int.TryParse(subArgs, out int removeId))
                        MenuService.RemoveMenuItem(removeId);
                    else
                        AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]menu remove <id>[/]");
                    break;

                default:
                    AnsiConsole.MarkupLine($"[red][[!]][/] Unknown menu command: '[bold red]{subcommand}[/]'");
                    ShowHelp();
                    break;
            }
        }

        public void ShowMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Menu Item Management[/]")
                        .RuleStyle("grey")
                        .Centered());

                var menuOptions = new SelectionPrompt<string>()
                    .Title("[bold green]Choose an action:[/]")
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                        "1. List menu items",
                        "2. View menu item details",
                        "3. Add new menu item",
                        "4. Update existing menu item",
                        "5. Remove menu item",
                        "0. Back to main menu"
                    })
                    .HighlightStyle(new Style(Color.Yellow, Color.Black, Decoration.None));

                string choice = AnsiConsole.Prompt(menuOptions);

                AnsiConsole.WriteLine();

                switch (choice.Split('.')[0].Trim())
                {
                    case "1":
                        MenuService.ListMenuItems();
                        break;

                    case "2":
                        int viewId = AnsiConsole.Prompt(
                            new TextPrompt<int>("[green]Enter Menu Item ID to view:[/]")
                                .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a number.[/]")
                                .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                        MenuService.InfoMenuItem(viewId);
                        break;

                    case "3":
                        MenuService.AddMenuItem();
                        break;

                    case "4":
                        int updateId = AnsiConsole.Prompt(
                            new TextPrompt<int>("[green]Enter Menu Item ID to update:[/]")
                                .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a number.[/]")
                                .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                        MenuService.UpdateMenuItem(updateId);
                        break;

                    case "5":
                        int removeId = AnsiConsole.Prompt(
                            new TextPrompt<int>("[green]Enter Menu Item ID to remove:[/]")
                                .ValidationErrorMessage("[red][[!]] Invalid ID. Please enter a number.[/]")
                                .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]ID must be a positive number.[/]")));
                        MenuService.RemoveMenuItem(removeId);
                        break;

                    case "0":
                        return;

                    default:
                        AnsiConsole.MarkupLine("[red][[!]] Invalid choice. Please select an option from the list.[/]");
                        break;
                }

                AnsiConsole.MarkupLine("\n[grey]Press Enter to continue...[/]");
                Console.ReadLine();
            }
        }

        private void ShowHelp()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Menu Command Usage[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Command[/]"))
                .AddColumn(new TableColumn("[bold blue]Description[/]"));

            table.AddRow("[cyan]menu add[/]", "[white]Add a new menu item.[/]");
            table.AddRow("[cyan]menu list[/]", "[white]List all menu items (first page).[/]");
            table.AddRow("[cyan]menu list[/] [grey]<page num>[/]", "[white]List menu items by page number.[/]"); 
            table.AddRow("[cyan]menu info[/] [grey]<id>[/]", "[white]View details of a specific menu item.[/]");   
            table.AddRow("[cyan]menu update[/] [grey]<id>[/]", "[white]Update an existing menu item.[/]"); 
            table.AddRow("[cyan]menu remove[/] [grey]<id>[/]", "[white]Remove a menu item.[/]"); 

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}