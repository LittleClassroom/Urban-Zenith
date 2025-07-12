using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class OrderCommand : ICommand
    {
        public string Name => "order";
        public string Description => "Manage orders (new, list, complete, additem, viewitems, removeitem, updateitem)";

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
                    case "new":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int newTableId))
                        {
                            Console.WriteLine("Usage: order new <tableId>");
                            return;
                        }
                        OrderService.CreateNewOrder(newTableId);
                        break;

                    case "list":
                        OrderService.ListOrders();
                        break;

                    case "complete":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int completeId))
                        {
                            Console.WriteLine("Usage: order complete <orderId>");
                            return;
                        }
                        OrderService.CompleteOrder(completeId);
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

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  order new <tableId>");
            Console.WriteLine("  order list");
            Console.WriteLine("  order complete <orderId>");
            Console.WriteLine("  order additem");
            Console.WriteLine("  order viewitems <tableId>");
            Console.WriteLine("  order removeitem <orderItemId>");
            Console.WriteLine("  order updateitem <orderItemId> <newQuantity>");
        }
    }
}
