using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to select sum for a summon (eg. synchro/ritual)
    /// </summary>
    public struct SelectSumState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectSumData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectSumData
    {
        /// <summary>
        /// Cards that you can select
        /// </summary>
        public DynamicCard[] SelectableCards;
        /// <summary>
        /// The sum that must be achieved for the summon
        /// </summary>
        public int Sum;
        /// <summary>
        /// Minimum amount of cards that must be selected
        /// </summary>
        public int Minimum;
        /// <summary>
        /// Maximum amount of cards that can be selected
        /// </summary>
        public int Maximum;
        /// <summary>
        /// Selection description hint for this state 
        /// </summary>
        public int SelectionHint;
        /// <summary>
        /// Whether the sum must be exact the sum value or if it can be greater than it
        /// </summary>
        public bool MustBeExactValue;
        /// <summary>
        /// Cards that are mandatory to be selected for this summon
        /// </summary>
        public DynamicCard[] MandatoryCards;
    }
}
