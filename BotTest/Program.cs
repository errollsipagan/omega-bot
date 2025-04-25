using OmegaBotWrapper;
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
SevensRoadActions botActions = new() // This class has a copy of the interface implementation, the default behaviour occurs even without proper implementation of each case
{
    DeckName = playerDeck.Name,
    DeckHash = sevensRoadDeckTest//playerDeck.Hash
}; // Instantiates the bot instruction set with the same deck as the player
DuelBotWrapper bot = new(botActions); // Creates a new bot wrapper instance with the given set of instructions
if (await bot.Connect()) // Connects the bot to the bot instance on the game
    await bot.Start(); // Starts the bot procedures
Console.WriteLine("Bot finished!"); // Bot has finished dueling
Console.ReadKey(); // Waits for user input before finishing the console execution