using OmegaBotWrapper.Decks;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Game.Base;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper.Utils;
using System.Collections.Generic;
using System.Linq;

namespace OmegaBotWrapper.Game.AI.Bots.BlueEyes
{
    /// <summary>
    /// Defines constant Card IDs with descriptive names.
    /// </summary>
    /// <summary>
    /// Defines constant Card IDs with descriptive names.
    /// </summary>
    public class CardId
    {
        public int ShinyBlack = 04148264;
        public int CardDestruction = 72892473;
        public int Jinzo = 77585513;
        public int BlueEyes = 89631139;
    }

    public class BlueEyesTestActions : BaseBotActions
    {
        public BlueEyesTestActions(DeckId deckId = DeckId.BlueEyes)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
        }
    }
}