using BotTest.Decks;
using BotTest.Game.Base;

namespace BotTest.Game.AI.Bots.MasterDuel.SwordSoul
{
    public class SwordSoulActions : RuleBasedBotActions
    {
        public SwordSoulActions(MasterDuelDeck deckId = MasterDuelDeck.SwordSoul)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
        }
    }
}
