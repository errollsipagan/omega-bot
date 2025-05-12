using BotTest.Decks;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BotTest.Game.AI.Bots.TestOnly
{
    public class Actions : RuleBasedBotActions
    {
        public Actions(SpeedDuelDeck deck = SpeedDuelDeck.SuperDuck)
        {
            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);
            InitializeCardInfo<ActionsCardId>();

            BotAction(
                action: MainAction.Activate,
                cardId: ActionsCardId.MysticalSpaceTyphoon
            );

            BotAction(
                action: MainAction.Activate,
                cardId: ActionsCardId.CosmicCyclone
            );

            BotAction(
                action: MainAction.Activate,
                cardId: ActionsCardId.DoubleCyclone
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: ActionsCardId.NegateAttack
            );

            BotAction(
                action: MainAction.Activate,
                cardId: ActionsCardId.NegateAttack
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: ActionsCardId.MalevolentCatastrophe
            );

            BotAction(
                action: MainAction.Activate,
                cardId: ActionsCardId.DarkMagicianCurtain
            );

            BotAction(
                action: MainAction.Activate,
                cardId: ActionsCardId.DarkMagicianAttack
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: ActionsCardId.Waboku
            );
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            if (state.SelectableCards.TryGetByID(ActionsCardId.NegateAttack, out _, out var index))
                return index;

            if (state.SelectableCards.TryGetByID(ActionsCardId.MalevolentCatastrophe, out _, out index))
                return index;

            bool playerWillBeDestroyAllSpellsAndTraps = _gameField.PlayerDestroyAllSpellAndTraps();

            if (playerWillBeDestroyAllSpellsAndTraps)
            {
                if (state.SelectableCards.TryGetByID(ActionsCardId.Waboku, out _, out index))
                    return index;
            }

            return await base.OnSelectChain(state);
        }

            
    }

}
