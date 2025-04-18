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
}
