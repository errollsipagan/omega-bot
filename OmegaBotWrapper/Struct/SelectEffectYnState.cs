namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to select yes or no for an effect
    /// </summary>
    public struct SelectEffectYnState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectEffectYnData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectEffectYnData
    {
        /// <summary>
        /// Card which triggered the given effect
        /// </summary>
        public DynamicCard EffectCard;
        /// <summary>
        /// Description of the effect that is being asked about
        /// </summary>
        public int Description;
    }
}
