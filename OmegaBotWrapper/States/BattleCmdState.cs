using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State defines that the bot is waiting for a command during battle phase
    /// </summary>
    public struct BattleCmdState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public BattleCmdData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct BattleCmdData
    {
        /// <summary>
        /// Which cards can attack
        /// </summary>
        public DynamicCard[] AttackableCards;
        /// <summary>
        /// Which cards can be activated
        /// </summary>
        public DynamicCard[] ActivableCards;
        /// <summary>
        /// Description index of the cards that can be activated
        /// </summary>
        public int[] ActivableDescriptions;
        /// <summary>
        /// Whether the bot can go to the main phase 2
        /// </summary>
        public bool CanMainPhaseTwo;
        /// <summary>
        /// Whether the bot can go to the end phase
        /// </summary>
        public bool CanEndPhase;
    }
}
