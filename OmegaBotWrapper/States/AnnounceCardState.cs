namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to announce a card
    /// </summary>
    public struct AnnounceCardState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public AnnounceCardData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct AnnounceCardData
    {
        /// <summary>
        /// Amount of filters
        /// </summary>
        public byte FilterCount;
        /// <summary>
        /// Filters that apply to whether you can or not select a card
        /// </summary>
        public int[] AppliableFilters;
    }
}
