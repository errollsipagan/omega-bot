//using OmegaBotWrapper.Struct;

//namespace OmegaBotWrapper.Game.AI.Actions
//{
//    public static class CardExtension
//    {
//        /// <summary>
//        /// Do this monster prevents activation of opponent's effect monsters in battle?
//        /// </summary>
//        public static bool IsMonsterHasPreventActivationEffectInBattle(this DynamicCard card)
//        {
//            return !card.IsDisabled() && System.Enum.IsDefined(typeof(PreventActivationEffectInBattle), card.ID);
//        }

//        /// <summary>
//        /// Is this monster is invincible to battle?
//        /// </summary>
//        public static bool IsMonsterInvincible(this DynamicCard card)
//        {
//            return !card.IsDisabled() &&
//                (card.Owner == 0 && System.Enum.IsDefined(typeof(InvincibleBotMonster), card.ID) ||
//                 card.Owner == 1 && System.Enum.IsDefined(typeof(InvincibleEnemyMonster), card.ID));
//        }

//        /// <summary>
//        /// Is this monster is dangerous to attack?
//        /// </summary>
//        public static bool IsMonsterDangerous(this DynamicCard card)
//        {
//            return !card.IsDisabled() && System.Enum.IsDefined(typeof(DangerousMonster), card.ID);
//        }
//    }
//}
