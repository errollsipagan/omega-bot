using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to select counters
    /// </summary>
    public struct SelectCounterState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectCounterData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectCounterData
    {
        /// <summary>
        /// Required type of the counter to be selected
        /// </summary>
        public short RequiredType;
        /// <summary>
        /// Required amount of counters
        /// </summary>
        public short RequiredQuantity;
        /// <summary>
        /// Cards from which cards can be selected
        /// </summary>
        public DynamicCard[] SelectableCards;
        /// <summary>
        /// Counters available for selection
        /// </summary>
        public short[] Counters;
    }
}
