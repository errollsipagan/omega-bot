using OmegaBotWrapper.Decks;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Game.AI.Bots.BotTest;
using OmegaBotWrapper.Game.Base;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper.Utils;
using System.Collections.Generic;
using System.Linq;

namespace OmegaBotWrapper.Game.AI.Bots.BlueEyes
{
    public class BlueEyesTestActions : BaseBotActions
    {
        private readonly List<ActionRule> _actionRules;
        private readonly Dictionary<int, Base.CardInfo> _cardInfoMap;

        public BlueEyesTestActions(DeckId deckId = DeckId.BlueEyes)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);

            _actionRules = new List<ActionRule>();
            _cardInfoMap = new Dictionary<int, Base.CardInfo>();

            // Initialize card info mapping
            AddCardInfo(BlueEyesCardId.BlueEyesWhiteDragon, "Blue Eyes White Dragon");
            AddCardInfo(BlueEyesCardId.TheWhiteStoneOfAncients, "The White Stone Of Ancients");
            AddCardInfo(BlueEyesCardId.BlueEyesAlternativeWhiteDragon, "Blue Eyes Alternative White Dragon");
            AddCardInfo(BlueEyesCardId.MaidenOfWhite, "Maiden of White");
            AddCardInfo(BlueEyesCardId.MaidenOfEyesOfBlue, "Maiden of Eyes Of Blue");
            AddCardInfo(BlueEyesCardId.BlueEyesSpiritDragon, "Blue Eyes Spirit Dragon");
            AddCardInfo(BlueEyesCardId.DragonSpiritOfWhite, "Dragon Spirit Of White");

            // Normal Summons
            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.DragonSpiritOfWhite,
                condition: (s, c) => s.SummonableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.MaidenOfEyesOfBlue,
                condition: (s, c) => s.SummonableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.MaidenOfWhite,
                condition: (s, c) => s.SummonableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.BlueEyesWhiteDragon,
                condition: (s, c) => s.SummonableCards.Any()
            ));

            // Set Monsters
            AddBotAction(new ActionRule(
                action: MainAction.SetMonster,
                cardId: BlueEyesCardId.TheWhiteStoneOfAncients
            ));

            // Reposable Monsters
            AddBotAction(new ActionRule(
                action: MainAction.Repos,
                cardId: BlueEyesCardId.TheWhiteStoneOfAncients
            ));

            // Special Summons
            AddBotAction(new ActionRule(
                action: MainAction.SpSummon,
                cardId: BlueEyesCardId.BlueEyesAlternativeWhiteDragon
            ));

            AddBotAction(new ActionRule(
                action: MainAction.SpSummon,
                cardId: BlueEyesCardId.BlueEyesSpiritDragon,
                condition: (s, c) => s.SpecialSummonableCards.Any()
            ));

            // Activable cards
            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.TheWhiteStoneOfAncients,
                condition: (s, c) => s.ActivableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.MaidenOfWhite,
                condition: (s, c) => s.ActivableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.MaidenOfEyesOfBlue,
                condition: (s, c) => s.ActivableCards.Any()
            ));
        }

        /// <summary>
        /// Adds a card info entry to the mapping.
        /// </summary>
        private void AddCardInfo(int cardId, string cardName)
        {
            _cardInfoMap[cardId] = new Base.CardInfo(cardId, cardName);
        }

        /// <summary>
        /// Adds a bot action rule to the list.
        /// </summary>
        private void AddBotAction(ActionRule rule)
        {
            _actionRules.Add(rule);
        }

        /// <summary>
        /// Overrides the base OnIdleCmd to execute action rules in sequence.
        /// </summary>
        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            //Activable cards, for each
            foreach (var cards in state.ActivableCards) {
                Console.WriteLine($"[Activable cards] => {cards.ID}");
            }

            //Summonable cards, for each
            foreach (var cards in state.SummonableCards)
            {
                Console.WriteLine($"[Summonable cards] => {cards.ID}");
            }

            //Activable cards, for each
            foreach (var cards in state.ActivableCards)
            {
                Console.WriteLine($"[Activable cards] => {cards.ID}");
            }

            //Special Summonable cards, for each
            foreach (var cards in state.SpecialSummonableCards)
            {
                Console.WriteLine($"[SpecialSummonable cards] => {cards.ID}");
            }

            var response = ExecuteFirstValidAction(state, _actionRules);
            return response ?? await base.OnIdleCmd(state); // Fallback to base implementation
        }

        /// <summary>
        /// Overrides the base OnSelectEffectYn to handle specific card effects.
        /// </summary>
        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            // Check if the card is Dragon Spirit Of White
            if (state.EffectCard.ID == BlueEyesCardId.DragonSpiritOfWhite)
            {
                ConsoleUtils.PrintColored("Activating effect for Dragon Spirit of White");
                return true;
            }

            // Fallback to base implementation for other cards
            return await base.OnSelectEffectYn(state);
        }

        /// <summary>
        /// Executes the first valid action from the rules in list order.
        /// </summary>
        protected virtual IdleCmdResponse? ExecuteFirstValidAction(IdleCmdData state, IEnumerable<ActionRule> rules)
        {
            foreach (var rule in rules)
            {
                var cards = rule.CardSelector(state).ToList();
                if (cards.TryGetByID(rule.CardId, out var card, out var index) && rule.Condition(state, card))
                {
                    // Get card name from mapping, fallback to CardId if not found
                    string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
                        ? cardInfo.CardName
                        : rule.CardId.ToString();
                    ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
                    return ExecuteAction(rule.Action, (ushort)index);
                }
            }

            return null; // No valid action found
        }

        /// <summary>
        /// Executes the specified action with the given card index.
        /// </summary>
        protected virtual IdleCmdResponse ExecuteAction(MainAction action, ushort index)
        {
            return new IdleCmdResponse(action, index);
        }
    }
}