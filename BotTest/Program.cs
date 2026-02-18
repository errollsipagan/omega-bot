using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.MasterDuel.DarkMagician;
using BotTest.Game.AI.Bots.MasterDuel.HandTraps;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.RushDuel.SevensRoad;
using BotTest.Game.AI.Bots.SpeedDuel.Amazoness;
using BotTest.Game.AI.Bots.SpeedDuel.MachineAngelAscension;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;
using OmegaBotWrapper;

#pragma warning disable CS8625
namespace BotTest
{
    class Program
    {
        enum DuelFormat { MasterDuel, SpeedDuel, RushDuel }

        static bool DEBUG_MODE = false;

        static async Task Main(string[] args)
        {
            if (DEBUG_MODE)
            {
                string blueEyeDeckTest = "M2cwO76VaXuDL/PiPRNZl2tYM77c7c00/7c1S1KpEyuIXrwynHUpEC8DYpAcCN/svcRYwWvLzJgoAFFbs4MJhpsfHGSBYcM7gozue2oZYHgH1J6EDF7GrWV8jGsrtRiuSR9kTO+zZemaxcCclLWZGaQHhEH2g/CzOgWGs07hrKYXJjDDcLBZARMMM8U0wfHk3mCmvCJdhuMLrzJnRSsygtwOAA==";
                string superDuckDeckTest = "k2M4PPEmHAdvq2WB4a5pMxhg+AISnoKEUwy6GBuANAyHV7UywrDeyfdwnL56N8v0Yi2WS8p5jN3TxRn2H/rMCDILAA==";
                string sevensRoadDeckTest = "s2KY+0STHYZ1V2jAcaaLJhwbAfkgvJVbC46fbNeA44uPEFj3uBYcb9ytwa65AoEtz0Ow8TJNOL4ENAuGw45D8FluBDYF6oPhjUD+eiS8CQm/0EPgpVYI7H0cgkHqAQ==";

                var playerDeck = await DuelBotWrapper.GetPlayerDeck(); // Gets the deck being used by the player
                AmazonessActions botActions = new()
                {
                    DeckName = playerDeck.Name,
                    DeckHash = "k2E7bW3Jvm/DD5Y1aZ8ZGn5wsvq2ZDHDMEj8vXogA0gOJg/CX2qjWZfN/sIIwi2qdQxtPDdYQZhN6iMDCIP0dU8XZ7izoZtlzvVYhudvfzCZFMazetucZPb7XsoIwhukfjOD9IKwb9wBBpA7AA=="
                };

                DuelBotWrapper bot = new(botActions);
                if (await bot.Connect())
                    await bot.Start();

                Console.WriteLine("Bot finished!");
                Console.ReadKey();
            }
            else
            {
                // Normal mode with menus and rematch
                await RunNormalMode();
            }
        }

        static async Task RunNormalMode()
        {
            while (true)
            {
                // --- Format and deck selection ---
                var formatOptions = new List<string> { "Master Duel", "Speed Duel", "Rush Duel" };
                int formatChoice = SelectMenuOption(formatOptions, "Select a duel format:");
                DuelFormat selectedFormat = (DuelFormat)formatChoice;

                var deckOptions = selectedFormat switch
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

                int selectedIndex = SelectMenuOption(deckOptions.Select(d => d.Name).ToList(), $"Select a {selectedFormat} deck to duel with:");
                var selectedDeck = deckOptions[selectedIndex];

                BaseBotActions botActions = selectedFormat switch
                {
                    DuelFormat.MasterDuel => selectedDeck.Id switch
                    {
                        MasterDuelDeck.BlueEyes => new BlueEyesActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.DarkMagician => new DarkMagicianActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.SwordSoul => new SwordSoulActions((MasterDuelDeck)selectedDeck.Id),
                        MasterDuelDeck.HandTraps => new HandTrapsActions((MasterDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Master Duel DeckId: {selectedDeck.Id}")
                    },
                    DuelFormat.SpeedDuel => selectedDeck.Id switch
                    {
                        SpeedDuelDeck.SuperDuck => new SuperDuckActions((SpeedDuelDeck)selectedDeck.Id),
                        SpeedDuelDeck.MachineAngelAscension => new MachineAngelAscensionActions((SpeedDuelDeck)selectedDeck.Id),
                        SpeedDuelDeck.Amazoness => new AmazonessActions((SpeedDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Speed Duel DeckId: {selectedDeck.Id}")
                    },
                    DuelFormat.RushDuel => selectedDeck.Id switch
                    {
                        RushDuelDeck.SevensRoad => new SevensRoadActions((RushDuelDeck)selectedDeck.Id),
                        _ => throw new InvalidOperationException($"No bot actions defined for Rush Duel DeckId: {selectedDeck.Id}")
                    },
                    _ => throw new InvalidOperationException("Invalid duel format.")
                };

                // --- Run duel ---
                while (true)
                {
                    DuelBotWrapper bot = new(botActions);
                    if (await bot.Connect())
                    {
                        Console.WriteLine("Bot is dueling!");
                        await bot.Start();
                        Console.WriteLine("Bot finished dueling!");
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect the bot.");
                        break;
                    }

                    // --- Rematch menu ---
                    var rematchOptions = new List<string> { "Yes", "No (choose another deck)", "Exit" };
                    var rematchColors = new Dictionary<int, ConsoleColor>
                    {
                        { 0, ConsoleColor.Green },    // Yes
                        { 1, ConsoleColor.Yellow },   // No (choose deck)
                        { 2, ConsoleColor.Red }       // Exit
                    };

                    int rematchChoice = SelectMenuOption(rematchOptions, "Would you like to rematch?", rematchColors);

                    if (rematchChoice == 0)
                    {
                        await DuelBotWrapper.RematchDuel();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Rematch requested. Reusing same bot setup...");
                        Console.ResetColor();
                        continue; // repeat the duel
                    }
                    else if (rematchChoice == 1)
                    {
                        Console.WriteLine("Returning to duel format selection...");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(">>>> Please click 'Rematch' in the game UI before continuing <<<<\n");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break; // back to format and deck selection
                    }
                    else
                    {
                        Console.WriteLine("Exiting program.");
                        Environment.Exit(0);
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu and allows selection using arrow keys and Enter.
        /// </summary>
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
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = optionColors != null && optionColors.TryGetValue(i, out var color)
                            ? color
                            : ConsoleColor.White;
                    }

                    Console.WriteLine(i == selectedIndex ? $"> {options[i]}" : $"  {options[i]}");
                    Console.ResetColor();
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                    selectedIndex = (selectedIndex - 1 + options.Count) % options.Count;
                else if (key == ConsoleKey.DownArrow)
                    selectedIndex = (selectedIndex + 1) % options.Count;

            } while (key != ConsoleKey.Enter);

            Console.Clear();
            return selectedIndex;
        }
    }
}
