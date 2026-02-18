using BotTest.Decks;
using BotTest.Game.Base;
using BotTest.Utils;
using OmegaBotWrapper.States;

namespace BotTest.Game.AI.Bots.MasterDuel.HandTraps
{
    public class HandTrapsActions : RuleBasedBotActions
    {
        public HandTrapsActions(MasterDuelDeck deckId = MasterDuelDeck.HandTraps)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
            InitializeCardInfo<HandTrapsCardId>();
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            GetCurrentDuelPhase();

            foreach (var cards in state.SelectableCards)
            {
                ConsoleUtils.PrintColored($"[OnSelectChain] [override] [Selectable cards] => {cards.ID}");
            }

            foreach (var cards in state.Descriptions)
            {
                ConsoleUtils.PrintColored($"[Descriptions cards] => {cards}");
            }

            if (!state.Forced)
                return 0;

            //if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulOfTaia, out var card, out var index))
            //    return index;

            //if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulGrandmasterChixiao, out card, out index))
            //    return index;

            //Fallback to base implementation for other cards
            return await base.OnSelectChain(state);
        }
    }
}
