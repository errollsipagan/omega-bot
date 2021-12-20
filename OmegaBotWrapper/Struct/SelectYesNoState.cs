namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to select yes or no
    /// </summary>
    public struct SelectYesNoState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectYesNoStateData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectYesNoStateData
    {
        /// <summary>
        /// Description of what is being asked about
        /// </summary>
        public int Description;
    }
}
