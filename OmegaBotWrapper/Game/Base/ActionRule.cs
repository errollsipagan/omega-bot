using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaBotWrapper.Game.Base
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
            // Usa o seletor padrão baseado na ação, ou lança exceção se não for encontrado
            if (!DefaultSelectors.TryGetValue(action, out var selector))
            {
                throw new ArgumentException($"No default card selector defined for action {action}.");
            }
            CardSelector = selector;
            Condition = condition ?? ((_, _) => true); // Default condition always returns true
        }
    }
}
