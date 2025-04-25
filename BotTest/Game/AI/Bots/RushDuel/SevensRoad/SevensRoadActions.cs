using BotTest.Decks;
using BotTest.Game.Base;

namespace BotTest.Game.AI.Bots.RushDuel.SevensRoad
{
    public class SevensRoadActions : BaseBotActions
    {
        public SevensRoadActions(RushDuelDeck deckId = RushDuelDeck.SevensRoad)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
        }
    }
}
