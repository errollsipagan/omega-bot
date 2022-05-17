using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to select tributes for a summon
    /// </summary>
    public struct SelectTributeState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectTributeData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectTributeData
    {
        /// <summary>
        /// Cards that can be selected
        /// </summary>
        public DynamicCard[] SelectableCards;
        /// <summary>
        /// Minimum amount of cards that needs to be selected
        /// </summary>
        public int Minimum;
        /// <summary>
        /// Maximum amount of cards that can be selected
        /// </summary>
        public int Maximum;
        /// <summary>
        /// Selection description hint of the state
        /// </summary>
        public int SelectionHint;
        /// <summary>
        /// Whether can be canceled or not (-1 to cancel if its possible)
        /// </summary>
        public bool Cancelable;
        /// <summary>
        /// Amount of selectable cards
        /// </summary>
        public byte SelectableCount;
    }
}
