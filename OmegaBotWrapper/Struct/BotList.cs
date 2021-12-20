namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// List of the bots you own on the duel room
    /// </summary>
    public struct BotList
    {
        /// <summary>
        /// Amount of bots you own
        /// </summary>
        public byte BotCount;
        /// <summary>
        /// Bots you own
        /// </summary>
        public BotInfo[] Bots;
    }
}
