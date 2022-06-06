namespace OmegaBotWrapper.States
{
    /// <summary>
    /// General header for bot state request information
    /// </summary>
    public struct BotStateHeader
    {
        /// <summary>
        /// Numeric representation of the bot current state
        /// </summary>
        public byte CurrentStateCode;
        /// <summary>
        /// String representation of the bot current state
        /// </summary>
        public string CurrentStateName;
        /// <summary>
        /// Current duel turn number
        /// </summary>
        public int CurrentTurn;
        /// <summary>
        /// Current duel turn phase
        /// </summary>
        public int CurrentPhase;
        /// <summary>
        /// Last player that chained
        /// </summary>
        public int LastChainPlayer;
        /// <summary>
        /// Last player that conducted a summon
        /// </summary>
        public int LastSummonPlayer;
        /// <summary>
        /// Current turn player
        /// </summary>
        public int CurrentPlayer;
        /// <summary>
        /// Forced to start duel first (to check for the need of CurrentPlayer inversion)
        /// </summary>
        public bool IsFirst;
    }
}
