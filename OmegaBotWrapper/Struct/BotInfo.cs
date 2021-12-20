namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Information about a bot you own
    /// </summary>
    public struct BotInfo
    {
        /// <summary>
        /// ID of the bot on the room
        /// </summary>
        public byte BotID;
        /// <summary>
        /// Name of the bot's deck
        /// </summary>
        public string DeckName;
        /// <summary>
        /// Index of the selected bot deck from game (-1 for custom)
        /// </summary>
        public short DeckIndex;
        /// <summary>
        /// Deck code (hash) of the bot current deck
        /// </summary>
        public string DeckHash;
    }
}
