namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to announce a number
    /// </summary>
    public struct AnnounceNumberState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public AnnounceNumberData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct AnnounceNumberData
    {
        /// <summary>
        /// Selectable numbers
        /// </summary>
        public int[] SelectableOptions;
    }
}
