using BotTest.Decks;
using BotTest.Game.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
