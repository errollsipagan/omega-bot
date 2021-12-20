using OmegaBotWrapper;
using BotTest;

var playerDeck = await DuelBotWrapper.GetPlayerDeck(); // Gets the deck being used by the player
DuelBotWrapper bot = new(playerDeck.Name, playerDeck.Hash); // Makes a bot that uses same deck as the player
bot.RegisterActions(); // Register actions for the bot to take during the duel
if (await bot.Connect()) // Connects the bot to the bot instance on the game
    await bot.Start(); // Starts the bot procedures
Console.WriteLine("Bot finished!"); // Bot has finished dueling
Console.ReadKey();