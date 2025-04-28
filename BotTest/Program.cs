/*using OmegaBotWrapper;
using BotTest;
using BotTest.Game.Field;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Decks;
using BotTest.Game.AI.Bots.RushDuel.SevensRoad;
string blueEyeDeckTest = "M2cwO76VaXuDL/PiPRNZl2tYM77c7c00/7c1S1KpEyuIXrwynHUpEC8DYpAcCN/svcRYwWvLzJgoAFFbs4MJhpsfHGSBYcM7gozue2oZYHgH1J6EDF7GrWV8jGsrtRiuSR9kTO+zZemaxcCclLWZGaQHhEH2g/CzOgWGs07hrKYXJjDDcLBZARMMM8U0wfHk3mCmvCJdhuMLrzJnRSsygtwOAA==";
string superDuckDeckTest = "k2Xr/7+EAYRLrC3Zezc4sqS5/2b0e7uDFYZPxfCA8c7wn8zvm8QYQPjm/YuMMMzPMhmMF/xXZTHR38bsG3eAAYa7p4sz3NnQzQLCc67HMhRaazKZFMaDzQLZc3jyTTAG6QfpBdkHAA==";
string sevensRoadDeckTest = "s2KY+0STHYZ1V2jAcaaLJhwbAfkgvJVbC46fbNeA44uPEFj3uBYcb9ytwa65AoEtz0Ow8TJNOL4ENAuGw45D8FluBDYF6oPhjUD+eiS8CQm/0EPgpVYI7H0cgkHqAQ==";
var playerDeck = await DuelBotWrapper.GetPlayerDeck(); // Gets the deck being used by the player
BlueEyesActions botActions = new() // This class has a copy of the interface implementation, the default behaviour occurs even without proper implementation of each case
{
    DeckName = playerDeck.Name,
    DeckHash = blueEyeDeckTest//playerDeck.Hash
}; // Instantiates the bot instruction set with the same deck as the player
DuelBotWrapper bot = new(botActions); // Creates a new bot wrapper instance with the given set of instructions
if (await bot.Connect()) // Connects the bot to the bot instance on the game
    await bot.Start(); // Starts the bot procedures
Console.WriteLine("Bot finished!"); // Bot has finished dueling
Console.ReadKey(); // Waits for user input before finishing the console execution*/

using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.MasterDuel.DarkMagician;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.RushDuel.SevensRoad;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;
using OmegaBotWrapper;
#pragma warning disable CS8625
namespace BotTest
{
    class Program
    {
        enum DuelFormat { MasterDuel, SpeedDuel, RushDuel }

        static async Task Main(string[] args)
        {
            while (true)
            {
                // Prompt for duel format (Master Duel or Speed Duel or Rush Duel)
                var formatOptions = new List<string> { "Master Duel", "Speed Duel", "Rush Duel" };
                int formatChoice = SelectMenuOption(formatOptions, "Select a duel format:");
                DuelFormat selectedFormat = (DuelFormat)formatChoice;

                // Get deck options based on the selected format
                List<DeckOption> deckOptions = selectedFormat switch
                {
                    DuelFormat.MasterDuel => DeckManager.GetMasterDuelDeckOptions().ToList(),
                    DuelFormat.SpeedDuel => DeckManager.GetSpeedDuelDeckOptions().ToList(),
                    DuelFormat.RushDuel => DeckManager.GetRushDuelDeckOptions().ToList(),
                    _ => throw new ArgumentException($"Duel format not supported: {selectedFormat}")
                };

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
                        MasterDuelDeck.BlueEyes => new BlueEyesActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.DarkMagician => new DarkMagicianActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.SwordSoul => new SwordSoulActions((MasterDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Master Duel DeckId: {selectedDeck.Id}")
                    },
                    DuelFormat.SpeedDuel => selectedDeck.Id switch
                    {
                        SpeedDuelDeck.SuperDuck => new SuperDuckActions((SpeedDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Speed Duel DeckId: {selectedDeck.Id}")
                    },
                    DuelFormat.RushDuel => selectedDeck.Id switch
                    {
                        RushDuelDeck.SevensRoad => new SevensRoadActions((RushDuelDeck)selectedDeck.Id),
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
                Console.Clear();
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

            Console.Clear();
            return selectedIndex;
        }
    }
}
