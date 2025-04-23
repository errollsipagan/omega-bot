using OmegaBotWrapper;
using BotTest;
using BotTest.Game.Field;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
string blueEyeDeckTest = "M2cwO76VaXuDL/PiPRNZl2tYM77c7c00/7c1S1KpEyuIXrwynHUpEC8DYpAcCN/svcRYwWvLzJgoAFFbs4MJhpsfHGSBYcM7gozue2oZYHgH1J6EDF7GrWV8jGsrtRiuSR9kTO+zZemaxcCclLWZGaQHhEH2g/CzOgWGs07hrKYXJjDDcLBZARMMM8U0wfHk3mCmvCJdhuMLrzJnRSsygtwOAA==";
string superDuckDeckTest = "k2UrsbZkv3n/IqNv3AGG901iDPwskxlBGCTm93YHKwyfiuEB453hP5l7NziypLn/ZgSpB6lb8F+Vpf//EgYQNtHfxgwyC4S7p4sz3NnQzQLCc67HMhRaazKZFMaDzQDpB9lzePJNMAbpA9kDAA==";

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
Console.ReadKey(); // Waits for user input before finishing the console execution