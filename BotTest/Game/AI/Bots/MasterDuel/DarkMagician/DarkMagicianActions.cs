using BotTest.Decks;
using BotTest.Game.Base;

namespace BotTest.Game.AI.Bots.MasterDuel.DarkMagician
{

    public class DarkMagicianActions : RuleBasedBotActions
    {
        public DarkMagicianActions(MasterDuelDeck deckId = MasterDuelDeck.DarkMagician)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
        }
    }
}