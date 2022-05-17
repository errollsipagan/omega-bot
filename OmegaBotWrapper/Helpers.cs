using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Helper functionality to extend the default bot functionality for easier usage
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Declare an attack with this given card
        /// </summary>
        /// <param name="card">Card to attack</param>
        /// <returns>Response to declare attack with this card</returns>
        public static BattleCmdResponse DeclareAttack(this DynamicCard card) => (BattleAction.Attack, (ushort)card.ActionIndex[(int)BattleAction.Attack]);
        /// <summary>
        /// Activates card effect of given index during battle phase open state
        /// </summary>
        /// <param name="card">Card to be activated</param>
        /// <param name="index">Effect index to be activated</param>
        /// <returns></returns>
        public static BattleCmdResponse BattleActivateFromIndex(this DynamicCard card, int index = 0) => (BattleAction.Activate, (ushort)card.ActionActivateIndex[index].ActionIndex);
        /// <summary>
        /// Activates card effect of given description during battle phase open state
        /// </summary>
        /// <param name="card">Card to be activated</param>
        /// <param name="description">Description of the effect to be activated</param>
        /// <returns></returns>
        public static BattleCmdResponse BattleActivateFromDescription(this DynamicCard card, int description = 0) => card.BattleActivateFromIndex(card.GetActivationIndexForDescription(description));
        /// <summary>
        /// Activates card effect of given index during main phase open state
        /// </summary>
        /// <param name="card">Card to be activated</param>
        /// <param name="index">Effect index to be activated</param>
        /// <returns></returns>
        public static IdleCmdResponse MainActivateFromIndex(this DynamicCard card, int index = 0) => (MainAction.Activate, (ushort)card.ActionActivateIndex[index].ActionIndex);
        /// <summary>
        /// Activates card effect of given description during main phase open state
        /// </summary>
        /// <param name="card">Card to be activated</param>
        /// <param name="description">Description of the effect to be activated</param>
        /// <returns></returns>
        public static IdleCmdResponse MainActivateFromDescription(this DynamicCard card, int description = 0) => card.MainActivateFromIndex(card.GetActivationIndexForDescription(description));
        /// <summary>
        /// Normal summon this given card
        /// </summary>
        /// <param name="card">Card to be normal summoned</param>
        /// <returns>Response to normal summon this card</returns>
        public static IdleCmdResponse NormalSummon(this DynamicCard card) => (MainAction.Summon, (ushort)card.ActionIndex[(int)MainAction.Summon]);
        /// <summary>
        /// Special summon this given card
        /// </summary>
        /// <param name="card">Card to be special summoned</param>
        /// <returns>Response to special summon this card</returns>
        public static IdleCmdResponse SpecialSummon(this DynamicCard card) => (MainAction.SpSummon, (ushort)card.ActionIndex[(int)MainAction.SpSummon]);
        /// <summary>
        /// Change battle position of this given card
        /// </summary>
        /// <param name="card">Card to have battle position changed</param>
        /// <returns>Response to change battle position of this card</returns>
        public static IdleCmdResponse Reposition(this DynamicCard card) => (MainAction.Repos, (ushort)card.ActionIndex[(int)MainAction.Repos]);
        /// <summary>
        /// Normal SET this monster card
        /// </summary>
        /// <param name="card">Monster card to be normal SET</param>
        /// <returns>Response to normal SET this monster card</returns>
        public static IdleCmdResponse SetMonster(this DynamicCard card) => (MainAction.SetMonster, (ushort)card.ActionIndex[(int)MainAction.SetMonster]);
        /// <summary>
        /// Set this spell/trap card
        /// </summary>
        /// <param name="card">Spell or Trap card to be set</param>
        /// <returns>Response to set this spell or trap card</returns>
        public static IdleCmdResponse SetSpellTrap(this DynamicCard card) => (MainAction.SetSpell, (ushort)card.ActionIndex[(int)MainAction.SetSpell]);
    }
}
