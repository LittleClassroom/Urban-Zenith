using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class MenuCommand : ICommand
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
                case "view":
                    if (int.TryParse(subArgs, out int id))
                        MenuService.ViewMenuItem(id);
                    else
                        Console.WriteLine("Usage: menu view <id>");
                    break;

                case "add":
                    MenuService.AddMenuItem();
                    break;

                case "list":
                    MenuService.ListMenuItems();
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

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  menu add");
            Console.WriteLine("  menu list");
            Console.WriteLine("  menu view <id>");
            Console.WriteLine("  menu update <id>");
            Console.WriteLine("  menu remove <id>");

        }
    }
}
