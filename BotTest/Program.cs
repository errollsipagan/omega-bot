using OmegaBotWrapper;
using BotTest;
using OmegaBotWrapper.Game.AI.Bots;

//var playerDeck = await DuelBotWrapper.GetPlayerDeck(); // Gets the deck being used by the player
//Actions botActions = new() // This class has a copy of the interface implementation, the default behaviour occurs even without proper implementation of each case
//{
//    DeckName = playerDeck.Name,
//    DeckHash = playerDeck.Hash
//}; // Instantiates the bot instruction set with the same deck as the player
DuelBotWrapper bot = new(new BotTestActions()); // Creates a new bot wrapper instance with the given set of instructions
if (await bot.Connect()) // Connects the bot to the bot instance on the game
    await bot.Start(); // Starts the bot procedures
Console.WriteLine("Bot finished!"); // Bot has finished dueling
Console.ReadKey(); // Waits for user input before finishing the console execution