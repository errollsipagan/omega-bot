namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to select an option from a list of options
    /// </summary>
    public struct SelectOptionState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectOptionData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectOptionData
    {
        /// <summary>
        /// Descriptions of the options that can be selected
        /// </summary>
        public int[] Options;
    }
}
