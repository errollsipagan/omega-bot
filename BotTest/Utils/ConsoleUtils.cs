namespace BotTest.Utils
{
    /// <summary>
    /// Utility class for console-related operations.
    /// </summary>
    public static class ConsoleUtils
    {
        private static readonly Random _random = new Random();
        private static readonly ConsoleColor[] _availableColors = new[]
        {
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.White,
            ConsoleColor.Yellow,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed
        };

        private static readonly Dictionary<string, ConsoleColor> _fixedColors = new(StringComparer.OrdinalIgnoreCase)
        {
            { "OnSelectPlace", ConsoleColor.Cyan },
            { "OnIdleCmd", ConsoleColor.Green },
            { "OnSelectTribute", ConsoleColor.Magenta },
            { "OnBattleCmd", ConsoleColor.Yellow }
        };

        /// <summary>
        /// Prints the specified text to the console with a fixed color for specific texts or a random color otherwise.
        /// </summary>
        /// <param name="text">The text to print.</param>
        public static void PrintColored(string text)
        {
            // Store the original console color
            var originalColor = Console.ForegroundColor;

            // Determine the color to use
            ConsoleColor color;
            if (_fixedColors.TryGetValue(text, out var fixedColor))
            {
                color = fixedColor;
            }
            else
            {
                color = _availableColors[_random.Next(_availableColors.Length)];
            }

            // Set the console color
            //Console.ForegroundColor = color;

            // Print the text
            Console.WriteLine(text); //Turn ON or Turn OFF

            // Restore the original color
            Console.ForegroundColor = originalColor;
        }
    }
}
