namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// State where the bot has to select one position for a card
    /// </summary>
    public struct SelectPositionState
    {
        /// <summary>
        /// Header representing the current state
        /// </summary>
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Data about the bot current state
        /// </summary>
        public SelectPositionData CurrentStateData;
    }
    /// <summary>
    /// Specific data about this state
    /// </summary>
    public struct SelectPositionData
    {
        /// <summary>
        /// ID of the card that is being selected position for
        /// </summary>
        public int CardID;
        /// <summary>
        /// Possible positions that you can assign to this card
        /// </summary>
        public byte[] Positions;
    }
}
