﻿using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class OrderCommand : ICommand, IMenuCommand
    {
        public string Name => "order";
        public string Description => "Manage orders (new, list, complete, additem, viewitems, removeitem, updateitem, cancel)"; // Updated description

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
                            Console.WriteLine("Usage: order new <tableId>");
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
                            Console.WriteLine("Usage: order complete <orderId>");
                            return;
                        }
                        OrderService.CompleteOrder(completeId);
                        break;

                    case "cancel": 
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int cancelId))
                        {
                            Console.WriteLine("Usage: order cancel <orderId>");
                            return;
                        }
                        OrderService.CancelOrder(cancelId);
                        break;

                    case "additem":
                        Console.Write("Enter Table ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int tableId))
                        {
                            Console.WriteLine("Invalid table ID.");
                            return;
                        }

                        int? activeOrderId = OrderService.GetActiveOrderIdByTableId(tableId);
                        if (activeOrderId == null)
                        {
                            activeOrderId = OrderService.CreateNewOrder(tableId);
                            if (activeOrderId == -1) return;
                        }

                        OrderItemService.AddItemsToOrder(activeOrderId.Value);
                        break;

                    case "viewitems":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int viewTableId))
                        {
                            Console.WriteLine("Usage: order viewitems <tableId>");
                            return;
                        }
                        OrderItemService.ListItemsForTable(viewTableId);
                        break;

                    case "removeitem":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int removeItemId))
                        {
                            Console.WriteLine("Usage: order removeitem <orderItemId>");
                            return;
                        }
                        OrderItemService.RemoveOrderItem(removeItemId);
                        break;

                    case "updateitem":
                        if (parts.Length < 3 ||
                            !int.TryParse(parts[1], out int updateItemId) ||
                            !int.TryParse(parts[2], out int newQty))
                        {
                            Console.WriteLine("Usage: order updateitem <orderItemId> <newQuantity>");
                            return;
                        }
                        OrderItemService.UpdateOrderItemQuantity(updateItemId, newQty);
                        break;

                    default:
                        Console.WriteLine($"Unknown order command: '{subcommand}'");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        public void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Order Management Menu ===");
                Console.WriteLine("1. New order");
                Console.WriteLine("2. List orders");
                Console.WriteLine("3. Complete order");
                Console.WriteLine("4. Add item to order");
                Console.WriteLine("5. View items for table");
                Console.WriteLine("6. Remove order item");
                Console.WriteLine("7. Update order item quantity");
                Console.WriteLine("8. Cancel order"); // New menu option
                Console.WriteLine("0. Back to main menu");
                Console.WriteLine("================================");
                Console.Write("Order >");

                string input = Console.ReadLine()?.Trim();
                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            Console.Write("Enter Table ID: ");
                            if (int.TryParse(Console.ReadLine(), out int newTableId))
                                OrderService.CreateNewOrder(newTableId);
                            else
                                Console.WriteLine("Invalid Table ID.");
                            break;

                        case "2":
                            OrderService.ListOrders();
                            break;

                        case "3":
                            Console.Write("Enter Order ID to complete: ");
                            if (int.TryParse(Console.ReadLine(), out int completeId))
                                OrderService.CompleteOrder(completeId);
                            else
                                Console.WriteLine("Invalid Order ID.");
                            break;

                        case "8": // New menu case
                            Console.Write("Enter Order ID to cancel: ");
                            if (int.TryParse(Console.ReadLine(), out int cancelId))
                                OrderService.CancelOrder(cancelId);
                            else
                                Console.WriteLine("Invalid Order ID.");
                            break;

                        case "4":
                            Console.Write("Enter Table ID: ");
                            if (int.TryParse(Console.ReadLine(), out int tableId))
                            {
                                int? activeOrderId = OrderService.GetActiveOrderIdByTableId(tableId);
                                if (activeOrderId == null)
                                {
                                    activeOrderId = OrderService.CreateNewOrder(tableId);
                                    if (activeOrderId == -1) break;
                                }
                                OrderItemService.AddItemsToOrder(activeOrderId.Value);
                            }
                            else
                                Console.WriteLine("Invalid Table ID.");
                            break;

                        case "5":
                            Console.Write("Enter Table ID: ");
                            if (int.TryParse(Console.ReadLine(), out int viewTableId))
                                OrderItemService.ListItemsForTable(viewTableId);
                            else
                                Console.WriteLine("Invalid Table ID.");
                            break;

                        case "6":
                            Console.Write("Enter Order Item ID to remove: ");
                            if (int.TryParse(Console.ReadLine(), out int removeItemId))
                                OrderItemService.RemoveOrderItem(removeItemId);
                            else
                                Console.WriteLine("Invalid Order Item ID.");
                            break;

                        case "7":
                            Console.Write("Enter Order Item ID to update: ");
                            if (!int.TryParse(Console.ReadLine(), out int updateItemId))
                            {
                                Console.WriteLine("Invalid Order Item ID.");
                                break;
                            }
                            Console.Write("Enter new quantity: ");
                            if (!int.TryParse(Console.ReadLine(), out int newQty))
                            {
                                Console.WriteLine("Invalid quantity.");
                                break;
                            }
                            OrderItemService.UpdateOrderItemQuantity(updateItemId, newQty);
                            break;

                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message}");
                }
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  order new <tableId>");
            Console.WriteLine("  order list [page_number]");
            Console.WriteLine("  order complete <orderId>");
            Console.WriteLine("  order cancel <orderId>");
            Console.WriteLine("  order additem");
            Console.WriteLine("  order viewitems <tableId>");
            Console.WriteLine("  order removeitem <orderItemId>");
            Console.WriteLine("  order updateitem <orderItemId> <newQuantity>");
        }
    }
}