namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to announce race(s)
    /// </summary>
    public struct AnnounceRaceState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public AnnounceRaceData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct AnnounceRaceData
    {
        /// <summary>
        /// Required amount of races to be selected
        /// </summary>
        public int RequiredCount;
        /// <summary>
        /// Races that can be selected from
        /// </summary>
        public int[] SelectableOptions;
    }
}
