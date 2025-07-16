using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

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
                        Console.WriteLine("Usage: menu info <id>");
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
                        Console.WriteLine("Usage: menu update <id>");
                    break;

                case "remove":
                    if (int.TryParse(subArgs, out int removeId))
                        MenuService.RemoveMenuItem(removeId);
                    else
                        Console.WriteLine("Usage: menu remove <id>");
                    break;

                default:
                    Console.WriteLine($"Unknown menu command: '{subcommand}'");
                    ShowHelp();
                    break;
            }
        }

        public void ShowMenu()
        {
            Console.Clear();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Menu Management ===");
                Console.WriteLine("1. List menu items");
                Console.WriteLine("2. View menu item");
                Console.WriteLine("3. Add menu item");
                Console.WriteLine("4. Update menu item");
                Console.WriteLine("5. Remove menu item");
                Console.WriteLine("0. Back to main menu");
                Console.Write("Choose an option: ");

                var input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        MenuService.ListMenuItems();
                        break;

                    case "2":
                        Console.Write("Enter Menu Item ID: ");
                        if (int.TryParse(Console.ReadLine(), out int viewId))
                            MenuService.InfoMenuItem(viewId);
                        else
                            Console.WriteLine("Invalid ID.");
                        break;

                    case "3":
                        MenuService.AddMenuItem();
                        break;

                    case "4":
                        Console.Write("Enter Menu Item ID to update: ");
                        if (int.TryParse(Console.ReadLine(), out int updateId))
                            MenuService.UpdateMenuItem(updateId);
                        else
                            Console.WriteLine("Invalid ID.");
                        break;

                    case "5":
                        Console.Write("Enter Menu Item ID to remove: ");
                        if (int.TryParse(Console.ReadLine(), out int removeId))
                            MenuService.RemoveMenuItem(removeId);
                        else
                            Console.WriteLine("Invalid ID.");
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  menu add");
            Console.WriteLine("  menu list");
            Console.WriteLine("  meneu list <page num>");
            Console.WriteLine("  menu info <id>");
            Console.WriteLine("  menu update <id>");
            Console.WriteLine("  menu remove <id>");
        }
    }
}
