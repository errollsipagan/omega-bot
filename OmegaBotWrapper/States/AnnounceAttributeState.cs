namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to announce attribute(s)
    /// </summary>
    public struct AnnounceAttributeState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public AnnounceAttributeData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct AnnounceAttributeData
    {
        /// <summary>
        /// Amount of required attributes to be selected
        /// </summary>
        public int RequiredCount;
        /// <summary>
        /// Attributes that can be selected
        /// </summary>
        public byte[] SelectableOptions;
    }
}
