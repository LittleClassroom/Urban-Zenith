using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class TableCommand : ICommand
    {
        public string Name => "table";
        public string Description => "Manage tables (list, available, add, remove, reset, status, assign, unassign, update)";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowHelp();
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
