using OmegaBotWrapper;
using OmegaBotWrapper.Decks;
using OmegaBotWrapper.Game.AI.Bots;
using OmegaBotWrapper.Game.AI.Bots.BlueEyes;
using OmegaBotWrapper.Game.AI.Bots.BotTest;
using OmegaBotWrapper.Game.AI.Bots.DarkMagician;
using OmegaBotWrapper.Game.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                // Get deck options
                var deckOptions = DeckManager.GetDeckOptions().ToList();

                // Select deck using arrow keys with colored highlight
                int selectedIndex = SelectMenuOption(deckOptions.Select(d => d.Name).ToList(), "Select a deck to duel with:");
                DeckId selectedDeckId = deckOptions[selectedIndex].Id;

                // Instantiate the appropriate bot actions based on DeckId
                BaseBotActions botActions = selectedDeckId switch
                {
                    DeckId.BotTest => new BotTestActions(selectedDeckId),
                    DeckId.BlueEyes => new BlueEyesTestActions(selectedDeckId),
                    DeckId.DarkMagician => new DarkMagicianTestActions(selectedDeckId),
                    _ => throw new InvalidOperationException($"No bot actions defined for DeckId: {selectedDeckId}")
                };

                // Create and start the bot
                DuelBotWrapper bot = new(botActions);
                if (await bot.Connect())
                {
                    await bot.Start();
                    Console.WriteLine("Bot finished dueling!");
                }
                else
                {
                    Console.WriteLine("Failed to connect the bot.");
                }

                // Prompt for rematch using arrow keys with colored options (Yes: Green, No: Red)
                var rematchOptions = new List<string> { "Yes", "No" };
                var rematchColors = new Dictionary<int, ConsoleColor>
                {
                    { 0, ConsoleColor.Green }, // Yes
                    { 1, ConsoleColor.Red }   // No
                };
                int rematchChoice = SelectMenuOption(rematchOptions, "Would you like to rematch?", rematchColors);
                if (rematchOptions[rematchChoice] == "Yes")
                {
                    if (await bot.Connect())
                    {
                        await DuelBotWrapper.RematchDuel();
                        Console.WriteLine("Bot finished rematch!");
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect for rematch.");
                    }
                }
                else
                {
                    Console.WriteLine("Exiting program.");
                    break;
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Displays a menu and allows selection using arrow keys and Enter.
        /// </summary>
        /// <param name="options">List of menu options to display.</param>
        /// <param name="prompt">Prompt message to display above the menu.</param>
        /// <param name="optionColors">Optional dictionary mapping option indices to text colors.</param>
        /// <returns>The index of the selected option.</returns>
        static int SelectMenuOption(List<string> options, string prompt, Dictionary<int, ConsoleColor> optionColors = null)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                //Console.Clear();
                Console.WriteLine(prompt);
                Console.WriteLine();

                for (int i = 0; i < options.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan; // Highlight selected option
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black; // Reset background
                        // Use custom color if provided, otherwise default to white
                        Console.ForegroundColor = optionColors != null && optionColors.TryGetValue(i, out var color)
                            ? color
                            : ConsoleColor.White;
                    }

                    Console.WriteLine(i == selectedIndex ? $"> {options[i]}" : $"  {options[i]}");
                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex - 1 + options.Count) % options.Count; // Wrap around
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex + 1) % options.Count; // Wrap around
                }
            } while (key != ConsoleKey.Enter);

            //Console.Clear();
            return selectedIndex;
        }
    }
}