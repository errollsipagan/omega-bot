using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using System;
using System.Collections;
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
        /// <summary>
        /// Try getting a card with given id from a collection of cards
        /// </summary>
        /// <param name="cards">Collection of cards</param>
        /// <param name="id">ID of the card being searched</param>
        /// <param name="card">Card returned if found (default if not)</param>
        /// <returns>True if a card with given ID was found or False otherwise</returns>
        public static bool TryGetByID(this IEnumerable<DynamicCard> cards, int id, out DynamicCard card)
        {
            card = default;
            if (id <= 0) return false;
            cards = cards.Where(c => c.ID == id || c.Alias == id);
            if (!cards.Any()) return false;
            card = cards.First();
            return true;
        }
        /// <summary>
        /// Extension methods for card collections.
        /// </summary>
        public static bool TryGetByID(this IEnumerable<DynamicCard> cards, int id, out DynamicCard card, out int index)
        {
            var cardList = cards.ToList();
            index = cardList.FindIndex(c => c.ID == id);
            card = index >= 0 ? cardList[index] : default;
            return index >= 0;
        }
        /// <summary>
        /// value representing the status this card will use on the battle on its current state
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>The power of the card on battle at its current state</returns>
        public static int GetRealPower(this DynamicCard card) => card.IsAttack() ? card.Attack : card.Defense;
        /// <summary>
        /// Checks whether card has or not the desired position
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="position">Position to be checked</param>
        /// <returns>True if card has given position, false otherwise</returns>
        public static bool HasPosition(this DynamicCard card, CardPosition position) => (card.Position & (int)position) != 0;
        /// <summary>
        /// Checks if the card is in attack position
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if card is in attack position, false otherwise</returns>
        public static bool IsAttack(this DynamicCard card) => card.HasPosition(CardPosition.Attack);
        /// <summary>
        /// Checks if the card is in defense position
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if card is in defense position, false otherwise</returns>
        public static bool IsDefense(this DynamicCard card) => card.HasPosition(CardPosition.Defence);
        /// <summary>
        /// Checks if the card is in face up position
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if the card is face up, false otherwise</returns>
        public static bool IsFaceup(this DynamicCard card) => card.HasPosition(CardPosition.FaceUp);
        /// <summary>
        /// Checks if the card is in face down position
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if the card is face down, false otherwise</returns>
        public static bool IsFacedown(this DynamicCard card) => card.HasPosition(CardPosition.FaceDown);
        /// <summary>
        /// Checks whether card has or not the desired status
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="stat">Card status being checked</param>
        /// <returns>True if card has desired status, false otherwise</returns>
        public static bool HasStatus(this DynamicCard card, Status stat) => (card.Status & (int)stat) != 0;
        /// <summary>
        /// Checks if the card is disabled
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if the card is disabled, false otherwise</returns>
        public static bool IsDisabled(this DynamicCard card) => card.HasStatus(Status.Disabled);
        /// <summary>
        /// Checks whether the card can be revived or not
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if the card can undergo the revival procedure, false otherwise</returns>
        public static bool CanRevive(this DynamicCard card) => card.HasStatus(Status.ProcComplete) || !(card.IsExtraCard() || card.HasType(CardType.Ritual) || card.HasType(CardType.SpSummon));
        /// <summary>
        /// Checks whether card contains given card type
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="type">Card type to check</param>
        /// <returns>True if card has given type, false otherwise</returns>
        public static bool HasType(this DynamicCard card, CardType type) => (card.Type & (int)type) != 0;
        /// <summary>
        /// Checks if the card belongs to the extra deck
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if the card is an extra deck card, false otherwise</returns>
        public static bool IsExtraCard(this DynamicCard card) => card.HasType(CardType.Fusion) || card.HasType(CardType.Synchro) || card.HasType(CardType.Xyz) || card.HasType(CardType.Link);
        /// <summary>
        /// Checks if the card is the one with given card ID
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="id">Card ID to be checked</param>
        /// <returns>True if the card ID is the desired one, false otherwise</returns>
        public static bool IsCode(this DynamicCard card, int id) => card.ID == id || card.Alias != 0 && card.Alias == id;
        /// <summary>
        /// Checks whether the card ID is contained on the given ID list or not
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="id">Card ID list to be checked</param>
        /// <returns>True if the card ID is contained on the ID list, false otherwise</returns>
        public static bool IsCode(this DynamicCard card, IEnumerable<int> ids) => ids.Contains(card.ID) || card.Alias != 0 && ids.Contains(card.Alias);
        /// <summary>
        /// Checks whether the card ID is contained on the given ID list or not
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="id">Card ID list to be checked</param>
        /// <returns>True if the card ID is contained on the ID list, false otherwise</returns>
        public static bool IsCode(this DynamicCard card, params int[] ids) => ids.Contains(card.ID) || card.Alias != 0 && ids.Contains(card.Alias);
        /// <summary>
        /// Checks if the card has any xyz material
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <returns>True if card has any xyz material, false otherwise</returns>
        public static bool HasXyzMaterial(this DynamicCard card) => card.Overlays.Any();
        /// <summary>
        /// Checks if the card has at least the given amount of xyz materials
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="count">Minimum amount of xyz materials desired</param>
        /// <returns>True if the card xyz material count is equal or bigger than the desired count, false otherwise</returns>
        public static bool HasXyzMaterial(this DynamicCard card, int count) => card.Overlays.Length >= count;
        /// <summary>
        /// Checks if the card has at least the given amount of xyz materials and at least one of them has the desired card ID
        /// </summary>
        /// <param name="card">Card reference</param>
        /// <param name="count">Minimum amount of xyz materials desired</param>
        /// <param name="cardid">Card ID being searched for</param>
        /// <returns>True if the card xyz material count is equal or bigger than the desired count and at least one of the desired card ID is found, false otherwise</returns>
        public static bool HasXyzMaterial(this DynamicCard card, int count, int cardid) => card.HasXyzMaterial(count) && card.Overlays.Contains(cardid);
    }
}
