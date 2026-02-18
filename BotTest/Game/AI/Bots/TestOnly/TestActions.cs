using BotTest.Decks;
using BotTest.Game.Base;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.TestOnly
{
    public class TestActions : RuleBasedBotActions
    {
        public TestActions(SpeedDuelDeck deck = SpeedDuelDeck.SuperDuck)
        {
            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);
            InitializeCardInfo<TestActionsCardId>();

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.MysticalSpaceTyphoon
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.CosmicCyclone
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.DoubleCyclone
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: TestActionsCardId.NegateAttack
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.NegateAttack
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: TestActionsCardId.MalevolentCatastrophe
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.DarkMagicianCurtain
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.DarkMagicianAttack
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: TestActionsCardId.Waboku
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.SphereKuriboh
            );

            BotAction(
                action: MainAction.Activate,
                cardId: TestActionsCardId.BlastSphere
            );
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            if (state.SelectableCards.TryGetByID(TestActionsCardId.NegateAttack, out _, out var index))
                return index;

            if (state.SelectableCards.TryGetByID(TestActionsCardId.MalevolentCatastrophe, out _, out index))
                return index;

            if (state.SelectableCards.TryGetByID(TestActionsCardId.SphereKuriboh, out _, out index))
                return index;


            bool playerWillBeDestroyAllSpellsAndTraps = _gameField.PlayerDestroyAllSpellAndTraps();

            if (playerWillBeDestroyAllSpellsAndTraps)
            {
                if (state.SelectableCards.TryGetByID(TestActionsCardId.Waboku, out _, out index))
                    return index;
            }

            return await base.OnSelectChain(state);
        }

            
    }

}
