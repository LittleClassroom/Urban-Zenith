using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class TableCommand : ICommand, IMenuCommand
    {
        public string Name => "table";
        public string Description => "Manage tables (list, available, add, remove, reset, status, assign, unassign, update)";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowMenu();
                return;
            }

            var parts = args.Split(' ', 3);
            string subcommand = parts[0].ToLower();
            string subArgs = parts.Length > 1 ? args.Substring(args.IndexOf(' ') + 1).Trim() : "";

            try
            {
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
                            Console.WriteLine("Usage: table remove <id>");
                            return;
                        }
                        TableService.RemoveTable(removeId);
                        break;
                    case "reset":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int resetId))
                        {
                            Console.WriteLine("Usage: table reset <id>");
                            return;
                        }
                        TableService.ResetTable(resetId);
                        break;
                    case "status":
                        if (parts.Length < 3 || !int.TryParse(parts[1], out int statusId))
                        {
                            Console.WriteLine("Usage: table status <id> <new_status>");
                            return;
                        }
                        string newStatus = parts[2];
                        TableService.UpdateTableStatus(statusId, newStatus);
                        break;
                    case "assign":
                        if (parts.Length < 3 || !int.TryParse(parts[1], out int tableId) || !int.TryParse(parts[2], out int staffId))
                        {
                            Console.WriteLine("Usage: table assign <tableId> <staffId>");
                            return;
                        }
                        TableService.AssignStaff(tableId, staffId);
                        break;
                    case "unassign":
                        if (int.TryParse(subArgs, out int unassignTableId))
                            TableService.UnassignStaff(unassignTableId);
                        else
                            Console.WriteLine("Usage: table unassign <tableId>");
                        break;
                    case "update":
                        if (parts.Length < 2 || !int.TryParse(parts[1], out int updateId))
                        {
                            Console.WriteLine("Usage: table update <id>");
                            return;
                        }
                        TableService.UpdateTable(updateId);
                        break;
                    default:
                        Console.WriteLine($"Unknown table command: '{subcommand}'");
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
                Console.WriteLine("\n=== Table Management Menu ===");
                Console.WriteLine("1. List all tables");
                Console.WriteLine("2. List available tables");
                Console.WriteLine("3. Add a new table");
                Console.WriteLine("4. Remove a table");
                Console.WriteLine("5. Reset table status");
                Console.WriteLine("6. Update table status");
                Console.WriteLine("7. Assign staff to table");
                Console.WriteLine("8. Unassign staff from table");
                Console.WriteLine("9. Update table info");
                Console.WriteLine("0. Back to main menu");
                Console.WriteLine("===============================\n");
                Console.Write("Table > ");

                string input = Console.ReadLine()?.Trim();
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
                            Console.Write("Enter table ID to remove: ");
                            if (int.TryParse(Console.ReadLine(), out int removeId))
                                TableService.RemoveTable(removeId);
                            else Console.WriteLine("Invalid ID.");
                            break;
                        case "5":
                            Console.Write("Enter table ID to reset: ");
                            if (int.TryParse(Console.ReadLine(), out int resetId))
                                TableService.ResetTable(resetId);
                            else Console.WriteLine("Invalid ID.");
                            break;
                        case "6":
                            Console.Write("Enter table ID: ");
                            if (!int.TryParse(Console.ReadLine(), out int statusId)) { Console.WriteLine("Invalid ID."); break; }
                            Console.Write("Enter new status: ");
                            string newStatus = Console.ReadLine();
                            TableService.UpdateTableStatus(statusId, newStatus);
                            break;
                        case "7":
                            Console.Write("Enter table ID: ");
                            if (!int.TryParse(Console.ReadLine(), out int tableId)) { Console.WriteLine("Invalid ID."); break; }
                            Console.Write("Enter staff ID: ");
                            if (!int.TryParse(Console.ReadLine(), out int staffId)) { Console.WriteLine("Invalid staff ID."); break; }
                            TableService.AssignStaff(tableId, staffId);
                            break;
                        case "8":
                            Console.Write("Enter table ID to unassign: ");
                            if (int.TryParse(Console.ReadLine(), out int unassignId))
                                TableService.UnassignStaff(unassignId);
                            else Console.WriteLine("Invalid ID.");
                            break;
                        case "9":
                            Console.Write("Enter table ID to update: ");
                            if (int.TryParse(Console.ReadLine(), out int updateId))
                                TableService.UpdateTable(updateId);
                            else Console.WriteLine("Invalid ID.");
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
            Console.WriteLine("  table list");
            Console.WriteLine("  table available");
            Console.WriteLine("  table add");
            Console.WriteLine("  table remove <id>");
            Console.WriteLine("  table reset <id>");
            Console.WriteLine("  table status <id> <new_status>");
            Console.WriteLine("  table assign <tableId> <staffId>");
            Console.WriteLine("  table unassign <tableId>");
            Console.WriteLine("  table update <id>");
        }
    }
}
