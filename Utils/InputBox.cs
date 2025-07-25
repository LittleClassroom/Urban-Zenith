using Spectre.Console;

namespace UrbanZenith.Utils
{
    internal static class InputBox
    {
        public static string GetStringBoxed(string placeholder = "Type your input...", string? hint = null)
        {
            string promptSymbol = "[bold green]>[/]";

            // Draw a fake box with top and bottom lines
            var inputLine = $"{promptSymbol}  [grey italic]{placeholder}[/]";

            var panel = new Panel(inputLine)
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Grey),
                Padding = new Padding(1, 0, 1, 0),
            };

            AnsiConsole.Write(panel);

            // Actual input prompt below box
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"{promptSymbol} ")
                    .PromptStyle("green")
                    .AllowEmpty()
                    .Validate(input => ValidationResult.Success())
            );
        }
    }
}
