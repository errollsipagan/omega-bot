using OmegaBotWrapper.Decks;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Game.Base;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Game.AI.Bots
{
    /// <summary>
    /// Class representing a bot action rule for sequential execution.
    /// </summary>
    public class ActionRule
    {
        public MainAction Action { get; }
        public int CardId { get; }
        public Func<IdleCmdData, IEnumerable<DynamicCard>> CardSelector { get; }
        public Func<IdleCmdData, DynamicCard, bool> Condition { get; }

        public ActionRule(
            MainAction action,
            int cardId,
            Func<IdleCmdData, IEnumerable<DynamicCard>> cardSelector,
            Func<IdleCmdData, DynamicCard, bool>? condition = null)
        {
            Action = action;
            CardId = cardId;
            CardSelector = cardSelector;
            Condition = condition ?? ((_, _) => true); // Default condition always returns true
        }
    }

    public class BotTestActions : BaseBotActions
    {
        private readonly List<ActionRule> _actionRules;

        public BotTestActions(DeckId deckId = DeckId.BotTest)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);

            _actionRules = new List<ActionRule>();

            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: 04148264, //Shiny Black
                cardSelector: s => s.SummonableCards,
                condition: (s, c) => s.SummonableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: 72892473, //Card Destruction
                cardSelector: s => s.ActivableCards,
                condition: (s, c) => s.ActivableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: 77585513, //Jinzo
                cardSelector: s => s.SummonableCards,
                condition: (s, c) => s.SummonableCards.Any()
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Summon,
                cardId: 89631139, //Blue-Eyes
                cardSelector: s => s.SummonableCards,
                condition: (s, c) => s.SummonableCards.Any()
            ));
        }

        /// <summary>
        /// Adds a bot action rule to the list.
        /// </summary>
        public void AddBotAction(ActionRule rule)
        {
            _actionRules.Add(rule);
        }

        /// <summary>
        /// Overrides the base OnIdleCmd to execute action rules in sequence.
        /// </summary>
        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            var response = ExecuteFirstValidAction(state, _actionRules);
            return response ?? await base.OnIdleCmd(state); // Fallback to base implementation
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