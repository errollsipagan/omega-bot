using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
namespace BotTest
{
    /// <summary>
    /// Sample bot that implements the bot actions interface without any changes to it
    /// </summary>
    public class Actions : IBotActions
    {
        /// <summary>
        /// Name of the deck to be used
        /// </summary>
        public string? DeckName { get; set; }
        /// <summary>
        /// Deck code to be used
        /// </summary>
        public string? DeckHash { get; set; }

        /// <summary>
        /// Wrapper instance which this instructions are linked to
        /// </summary>
        public DuelBotWrapper? BotWrapper { get; set; }
    }
}