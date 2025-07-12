using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class OrderItemCommand : ICommand
    {
        public string Name => "orderitem";
        public string Description => "Manage order items (add, list, viewitems, update, remove)";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowHelp();
                return;
            }

            var parts = args.Split(' ', 3);
            string subcommand = parts[0].ToLower();

            try
            {
                switch (subcommand)
                {
                    case "add":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int orderId))
                        {
                            Console.WriteLine("Usage: orderitem add <orderId>");
                            return;
                        }
                        OrderItemService.AddItemsToOrder(orderId);
                        break;

                    case "list":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int listOrderId))
                        {
                            Console.WriteLine("Usage: orderitem list <orderId>");
                            return;
                        }
                        OrderItemService.ListOrderItems(listOrderId);
                        break;

                    case "viewitems":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int tId))
                        {
                            Console.WriteLine("Usage: orderitem viewitems <tableId>");
                            return;
                        }
                        OrderItemService.ListItemsForTable(tId);
                        break;

                    case "remove":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int removeId))
                        {
                            Console.WriteLine("Usage: orderitem remove <orderItemId>");
                            return;
                        }
                        OrderItemService.RemoveOrderItem(removeId);
                        break;

                    case "update":
                        if (parts.Length < 3 ||
                            !int.TryParse(parts[1], out int updateId) ||
                            !int.TryParse(parts[2], out int newQty))
                        {
                            Console.WriteLine("Usage: orderitem update <orderItemId> <newQuantity>");
                            return;
                        }
                        OrderItemService.UpdateOrderItemQuantity(updateId, newQty);
                        break;

                    default:
                        Console.WriteLine($"Unknown orderitem command: '{subcommand}'");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  orderitem add <orderId>");
            Console.WriteLine("  orderitem list <orderId>");
            Console.WriteLine("  orderitem viewitems <tableId>");
            Console.WriteLine("  orderitem update <orderItemId> <newQuantity>");
            Console.WriteLine("  orderitem remove <orderItemId>");
        }
    }
}
