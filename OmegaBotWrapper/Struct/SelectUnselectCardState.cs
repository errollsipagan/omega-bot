namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to select or unselect card(s)
    /// </summary>
    public struct SelectUnselectCardState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectUnselectCardData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectUnselectCardData
    {
        /// <summary>
        /// Cards that can be selected
        /// </summary>
        public DynamicCard[] SelectableCards;
        /// <summary>
        /// Minimum amount of cards required to be selected
        /// </summary>
        public int Minimum;
        /// <summary>
        /// Maximum amount of cards that can be selected
        /// </summary>
        public int Maximum;
        /// <summary>
        /// Hint description for this selection
        /// </summary>
        public int SelectionHint;
        /// <summary>
        /// Whether this selection is cancelable or not (response with -1 for cancel)
        /// </summary>
        public bool Cancelable;
        /// <summary>
        /// Amount of cards that are selectable
        /// </summary>
        public byte SelectableCount;
    }
}
