namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// General state information of the bot when not into other specific states
    /// </summary>
    public struct BotGeneralState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public BotGeneralStateData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct BotGeneralStateData
    {
        /// <summary>
        /// Whether the bot is ready or not
        /// </summary>
        public bool IsReady;
    }
}
