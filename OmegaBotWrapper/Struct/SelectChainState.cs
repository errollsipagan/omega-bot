namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to select something to chain
    /// </summary>
    public struct SelectChainState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectChainData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectChainData
    {
        /// <summary>
        /// Cards that can be selected for chaining
        /// </summary>
        public DynamicCard[] SelectableCards;
        /// <summary>
        /// Descriptions of the cards activations
        /// </summary>
        public int[] Descriptions;
        /// <summary>
        /// Whether you are forced to chain something or not (-1 to chain nothing if not forced)
        /// </summary>
        public bool Forced;
    }
}
