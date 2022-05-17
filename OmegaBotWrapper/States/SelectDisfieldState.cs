namespace OmegaBotWrapper.States
{
    /// <summary>
    /// State where the bot has to select field zone(s) for being disabled
    /// </summary>
    public struct SelectDisfieldState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectDisfieldData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectDisfieldData
    {
        /// <summary>
        /// ID of the card that triggered this effect
        /// </summary>
        public int CardID;
        /// <summary>
        /// Player of which the field is selected
        /// </summary>
        public byte Player;
        /// <summary>
        /// Location of the zones to be selected (eg. monster zones)
        /// </summary>
        public short Location;
        /// <summary>
        /// Filter mask for the zones to define which of the available zones can be selected
        /// </summary>
        public int Filter;
    }
}
