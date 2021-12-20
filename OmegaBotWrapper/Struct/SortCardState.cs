namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has sort a list of cards into desired order
    /// </summary>
    public struct SortCardState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SortCardData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SortCardData
    {
        /// <summary>
        /// Cards that can be sorted
        /// </summary>
        public DynamicCard[] SelectableCards;
    }
}
