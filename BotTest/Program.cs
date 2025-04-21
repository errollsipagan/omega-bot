using OmegaBotWrapper;
using OmegaBotWrapper.Decks;
using OmegaBotWrapper.Game.AI.Bots;
using OmegaBotWrapper.Game.AI.Bots.BlueEyes;
using OmegaBotWrapper.Game.AI.Bots.BotTest;
using OmegaBotWrapper.Game.AI.Bots.DarkMagician;
using OmegaBotWrapper.Game.AI.Bots.SpeedDuel.SuperDuck;
using OmegaBotWrapper.Game.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotTest
{
    class Program
    {
        enum DuelFormat { MasterDuel, SpeedDuel }

        static async Task Main(string[] args)
        {
            while (true)
            {
                // Prompt for duel format (Master Duel or Speed Duel)
                var formatOptions = new List<string> { "Master Duel", "Speed Duel" };
                int formatChoice = SelectMenuOption(formatOptions, "Select a duel format:");
                DuelFormat selectedFormat = (DuelFormat)formatChoice;

                // Get deck options based on the selected format
                List<DeckOption> deckOptions = selectedFormat == DuelFormat.MasterDuel
                    ? DeckManager.GetDeckOptions().ToList()
                    : DeckManager.GetSpeedDuelDeckOptions().ToList();

                if (!deckOptions.Any())
                {
                    Console.WriteLine($"No decks available for {selectedFormat}. Exiting...");
                    break;
                }

                // Select deck using arrow keys with colored highlight
                int selectedIndex = SelectMenuOption(deckOptions.Select(d => d.Name).ToList(), $"Select a {selectedFormat} deck to duel with:");
                var selectedDeck = deckOptions[selectedIndex];

                // Instantiate the appropriate bot actions based on DeckId and format
                BaseBotActions botActions = selectedFormat switch
                {
                    DuelFormat.MasterDuel => selectedDeck.Id switch
                    {
                        MasterDuelDeck.BotTest => new BotTestActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.BlueEyes => new BlueEyesTestActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.DarkMagician => new DarkMagicianTestActions((MasterDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Master Duel DeckId: {selectedDeck.Id}")
                    },
                    DuelFormat.SpeedDuel => selectedDeck.Id switch
                    {
                        SpeedDuelDeck.SuperDuck => new SuperDuckActions((SpeedDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Speed Duel DeckId: {selectedDeck.Id}")
                    },
                    _ => throw new InvalidOperationException("Invalid duel format.")
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