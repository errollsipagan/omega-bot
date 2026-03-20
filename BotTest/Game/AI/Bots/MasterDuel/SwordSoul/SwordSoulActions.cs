using BotTest.Decks;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.SwordSoul
{
    public class SwordSoulActions : RuleBasedBotActions
    {
        public SwordSoulActions(MasterDuelDeck deckId = MasterDuelDeck.SwordSoul)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
            InitializeCardInfo<SwordSoulCardId>();

            //Activable cards
            BotAction(
                action: MainAction.Activate,
                cardId: SwordSoulCardId.SwordsoulEmergence,
                condition: EmergenceAndTaiaCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SwordSoulCardId.SwordsoulOfTaia
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SwordSoulCardId.YangZingBaxia
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SwordSoulCardId.SwordsoulStrategistLongYuan,
                condition: SwordsoulStrategistLongYuanCondition
            );

            //Summonable cards
            BotAction(
                action: MainAction.Summon,
                cardId: SwordSoulCardId.SwordsoulOfTaia
            );

            //Special Summonable cards
            BotAction(
                action: MainAction.SpSummon,
                cardId: SwordSoulCardId.YangZingBaxia
            );

            BotAction(
                action: MainAction.SpSummon,
                cardId: SwordSoulCardId.SwordsoulGrandmasterChixiao
            );

            //Spell/Trap Settable
            BotAction(
                action: MainAction.SetSpell,
                cardId: SwordSoulCardId.SwordsoulBlackout
            );
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            GetCurrentDuelPhase();

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulOfTaia, out var card, out var index))
                return index;

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulGrandmasterChixiao, out card, out index))
                return index;

            //Fallback to base implementation for other cards
            return await base.OnSelectChain(state);
        }

        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            if (state.EffectCard.IsCode(SwordSoulCardId.SwordsoulOfMoYe))
                return true;

            return await base.OnSelectEffectYn(state);
        }

        public override async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            if (state.CardID.Equals(56495148) || //Token
                state.CardID.Equals(20001444) || //Token
                state.CardID.Equals(SwordSoulCardId.YangZingBaxia) ||
                state.CardID.Equals(SwordSoulCardId.SwordsoulOfMoYe))
                return (CardPosition)state.Positions[1]; //Face-Up Defence

            return await base.OnSelectPosition(state);
        }

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            ConsoleUtils.PrintColored("[override][OnSelectCard] " + (DHint)state.SelectionHint);

            if (state.SelectionHint.Equals((int)DHint.ATOHAND))
            {
                var card = _gameField.FilterSelectableCardsById(state, SwordSoulCardId.SwordsoulStrategistLongYuan);
                return card;
            }

            if (state.SelectionHint.Equals((int)DHint.TOGRAVE))
            {
                var card = _gameField.FilterSelectableCardsById(state, SwordSoulCardId.SwordsoulOfMoYe);
                return card;
            }

            return await base.OnSelectCard(state);
        }

        private async Task<bool> EmergenceAndTaiaCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = card.IsCode(SwordSoulCardId.SwordsoulEmergence);
            bool hasSwordsoulEmergence = await _gameField.HasCardInHand(SwordSoulCardId.SwordsoulEmergence);
            bool hasSwordsoulOfTaia = await _gameField.HasCardInHand(SwordSoulCardId.SwordsoulOfTaia);

            return canActivate && hasSwordsoulEmergence && hasSwordsoulOfTaia;
        }

        private async Task<bool> SwordsoulStrategistLongYuanCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan);
            bool hasSwordsoulOfMoYe = await _gameField.HasCardInBotMonsterZone(SwordSoulCardId.SwordsoulOfMoYe);

            return canActivate && hasSwordsoulOfMoYe;
        }
    }
}
