using OmegaBotWrapper.Decks;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Game.Base;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper.Utils;
using System.Collections.Generic;
using System.Linq;

namespace OmegaBotWrapper.Game.AI.Bots.DarkMagician
{

    public class DarkMagicianTestActions : BaseBotActions
    {
        public DarkMagicianTestActions(MasterDuelDeck deckId = MasterDuelDeck.DarkMagician)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
        }
    }
}