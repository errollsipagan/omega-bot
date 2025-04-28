using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.Base
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

        private static readonly Dictionary<MainAction, Func<IdleCmdData, IEnumerable<DynamicCard>>> DefaultSelectors = new()
        {
            { MainAction.Summon, s => s.SummonableCards },
            { MainAction.SpSummon, s => s.SpecialSummonableCards },
            { MainAction.Activate, s => s.ActivableCards },
            { MainAction.SetMonster, s => s.MonsterSetableCards },
            { MainAction.SetSpell, s => s.SpellSetableCards },
            { MainAction.Repos, s => s.ReposableCards }
        };

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

        /// <summary>
        /// Constructor that automatically selects the card selector based on the action.
        /// </summary>
        public ActionRule(
            MainAction action,
            int cardId,
            Func<IdleCmdData, DynamicCard, bool>? condition = null)
        {
            Action = action;
            CardId = cardId;
            // Use the default selector based on the action, or throw an exception if not found
            if (!DefaultSelectors.TryGetValue(action, out var selector))
            {
                throw new ArgumentException($"No default card selector defined for action {action}.");
            }
            CardSelector = selector;
            Condition = condition ?? ((_, _) => true); // Default condition always returns true
        }
    }
}
