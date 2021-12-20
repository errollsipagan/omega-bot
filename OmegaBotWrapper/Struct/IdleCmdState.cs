namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State defines that the bot is waiting for a command during main phase
    /// </summary>
    public struct IdleCmdState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public IdleCmdStateData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct IdleCmdStateData
    {
        /// <summary>
        /// Cards that the bot can summon
        /// </summary>
        public DynamicCard[] SummonableCards;
        /// <summary>
        /// Cards that the bot can special summon
        /// </summary>
        public DynamicCard[] SpecialSummonableCards;
        /// <summary>
        /// Cards that the bot can change the position
        /// </summary>
        public DynamicCard[] ReposableCards;
        /// <summary>
        /// Monster cards that the bot can set
        /// </summary>
        public DynamicCard[] MonsterSetableCards;
        /// <summary>
        /// Spell/Trap cards that the bot can set
        /// </summary>
        public DynamicCard[] SpellSetableCards;
        /// <summary>
        /// Cards that the bot can activate
        /// </summary>
        public DynamicCard[] ActivableCards;
        /// <summary>
        /// Descriptions of the activations of the cards that the bot can realize
        /// </summary>
        public int[] ActivableDescriptions;
        /// <summary>
        /// Whether the bot can go to the battle phase
        /// </summary>
        public bool CanBattlePhase;
        /// <summary>
        /// Whether the bot can go to the end phase
        /// </summary>
        public bool CanEndPhase;
        /// <summary>
        /// Whether the bot can shuffle the hand
        /// </summary>
        public bool CanShuffleHand;
    }
}
