using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;
using Spectre.Console; 

namespace UrbanZenith.Commands
{
    public class OrderCommand : ICommand, IMenuCommand
    {
        public string Name => "order";
        public string Description => "Manage orders (new, list, complete, additem, viewitems, removeitem, updateitem, cancel)";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowHelp();
                return;
            }

            var parts = args.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            string subcommand = parts[0].ToLower();

            try
            {
                switch (subcommand)
                {
                    case "new":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int newTableId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]order new <tableId>[/]");
                            return;
                        }
                        OrderService.CreateNewOrder(newTableId);
                        break;

                    case "list":
                        int page = 1;
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int specifiedPage))
                        {
                            page = specifiedPage;
                        }
                        OrderService.ListOrders(page);
                        break;

                    case "complete":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int completeId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]order complete <orderId>[/]");
                            return;
                        }
                        OrderService.CompleteOrder(completeId);
                        break;

                    case "cancel":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int cancelId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]order cancel <orderId>[/]");
                            return;
                        }
                        OrderService.CancelOrder(cancelId);
                        break;

                    case "additem":
                        int tableIdAddItem = AnsiConsole.Prompt(
                            new TextPrompt<int>("[green]Enter Table ID:[/]")
                                .ValidationErrorMessage("[red][[!]] Invalid Table ID. Please enter a number.[/]")
                                .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Table ID must be a positive number.[/]")));

                        int? activeOrderId = OrderService.GetActiveOrderIdByTableId(tableIdAddItem);
                        if (activeOrderId == null)
                        {
                            AnsiConsole.MarkupLine("[yellow]No active order found for this table. Creating a new one...[/]");
                            activeOrderId = OrderService.CreateNewOrder(tableIdAddItem);
                            if (activeOrderId == -1) return;
                        }
                        OrderItemService.AddItemsToOrder(activeOrderId.Value);
                        break;

                    case "viewitems":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int viewTableId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]order viewitems <tableId>[/]");
                            return;
                        }
                        OrderItemService.ListItemsForTable(viewTableId);
                        break;

                    case "removeitem":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int removeItemId))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]order removeitem <orderItemId>[/]");
                            return;
                        }
                        OrderItemService.RemoveOrderItem(removeItemId);
                        break;

                    case "updateitem":
                        if (parts.Length < 3 ||
                            !int.TryParse(parts[1], out int updateItemId) ||
                            !int.TryParse(parts[2], out int newQty))
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]order updateitem <orderItemId> <newQuantity>[/]");
                            return;
                        }
                        OrderItemService.UpdateOrderItemQuantity(updateItemId, newQty);
                        break;

                    default:
                        AnsiConsole.MarkupLine($"[red][[!]] Unknown order command:[/]'[bold red]{subcommand}[/]'.");
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
                    new Rule("[bold yellow]Order Management[/]")
                        .RuleStyle("grey")
                        .Centered());

                AnsiConsole.MarkupLine("[green]1.[/] [yellow]Create new order[/]");
                AnsiConsole.MarkupLine("[green]2.[/] [yellow]List all orders[/]");
                AnsiConsole.MarkupLine("[green]3.[/] [yellow]Complete an order[/]");
                AnsiConsole.MarkupLine("[green]4.[/] [yellow]Add item(s) to an order[/]");
                AnsiConsole.MarkupLine("[green]5.[/] [yellow]View items for a table's active order[/]");
                AnsiConsole.MarkupLine("[green]6.[/] [yellow]Remove item from order[/]");
                AnsiConsole.MarkupLine("[green]7.[/] [yellow]Update order item quantity[/]");
                AnsiConsole.MarkupLine("[green]8.[/] [yellow]Cancel an order[/]");
                AnsiConsole.MarkupLine("[red]0.[/] [red]Back to main menu[/]");
                AnsiConsole.WriteLine();

                int choice = AnsiConsole.Prompt(
                    new TextPrompt<int>("[bold green]Enter your choice (0-8):[/]")
                        .PromptStyle("cyan")
                        .ValidationErrorMessage("[red][!] Invalid input. Please enter a number between 0 and 8.[/]")
                        .Validate(input =>
                        {
                            return input switch
                            {
                                >= 0 and <= 8 => ValidationResult.Success(),
                                _ => ValidationResult.Error("[red]Please select a valid option (0-8).[/]")
                            };
                        }));

                AnsiConsole.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case 1:
                            int newTableId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Table ID for new order:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Table ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Table ID must be a positive number.[/]")));
                            OrderService.CreateNewOrder(newTableId);
                            break;

                        case 2: // This is the case to modify
                            int pageNumber = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter page number (1 or more):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid page number. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Page number must be 1 or greater.[/]")));
                            OrderService.ListOrders(pageNumber);
                            break;

                        case 3:
                            int completeId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Order ID to complete:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Order ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Order ID must be a positive number.[/]")));
                            OrderService.CompleteOrder(completeId);
                            break;

                        case 8:
                            int cancelId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Order ID to cancel:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Order ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Order ID must be a positive number.[/]")));
                            OrderService.CancelOrder(cancelId);
                            break;

                        case 4:
                            int tableIdAddItem = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Table ID:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Table ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Table ID must be a positive number.[/]")));

                            int? activeOrderId = OrderService.GetActiveOrderIdByTableId(tableIdAddItem);
                            if (activeOrderId == null)
                            {
                                AnsiConsole.MarkupLine("[yellow]No active order found for this table. Creating a new one...[/]");
                                activeOrderId = OrderService.CreateNewOrder(tableIdAddItem);
                                if (activeOrderId == -1) break;
                            }
                            OrderItemService.AddItemsToOrder(activeOrderId.Value);
                            break;

                        case 5:
                            int viewTableId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Table ID to view items:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Table ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Table ID must be a positive number.[/]")));
                            OrderItemService.ListItemsForTable(viewTableId);
                            break;

                        case 6:
                            int removeItemId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Order Item ID to remove:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Order Item ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Order Item ID must be a positive number.[/]")));
                            OrderItemService.RemoveOrderItem(removeItemId);
                            break;

                        case 7:
                            int updateItemId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Order Item ID to update:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Order Item ID. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Order Item ID must be a positive number.[/]")));

                            int newQty = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter new quantity:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid quantity. Please enter a positive number.[/]")
                                    .Validate(input => input > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Quantity must be a positive number.[/]")));
                            OrderItemService.UpdateOrderItemQuantity(updateItemId, newQty);
                            break;

                        case 0:
                            return;

                        default:
                            AnsiConsole.MarkupLine("[red][[!]] Invalid option. Please select an option from the list.[/]");
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
                new Rule("[bold yellow]Order Command Usage[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Command[/]"))
                .AddColumn(new TableColumn("[bold blue]Description[/]"));

            table.AddRow("[cyan]order new[/] [grey]<tableId>[/]", "[white]Create a new order for a table.[/]");
            table.AddRow("[cyan]order list[/]", "[white]List all active orders (first page).[/]");
            table.AddRow("[cyan]order list[/] [grey]<page num>[/]", "[white]List orders by page number.[/]");
            table.AddRow("[cyan]order complete[/] [grey]<orderId>[/]", "[white]Mark an order as completed.[/]");
            table.AddRow("[cyan]order cancel[/] [grey]<orderId>[/]", "[white]Cancel an existing order.[/]");
            table.AddRow("[cyan]order additem[/]", "[white]Add items to an order (will create one if none active).[/]");
            table.AddRow("[cyan]order viewitems[/] [grey]<tableId>[/]", "[white]View all items on a table's active order.[/]");
            table.AddRow("[cyan]order removeitem[/] [grey]<orderItemId>[/]", "[white]Remove a specific item from an order.[/]");
            table.AddRow("[cyan]order updateitem[/] [grey]<orderItemId>[/]", "[white]Update quantity of an item on an order.[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}