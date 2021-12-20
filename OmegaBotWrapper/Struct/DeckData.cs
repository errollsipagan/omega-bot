namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Information about a specific deck
    /// </summary>
    public struct DeckData
    {
        /// <summary>
        /// Name of the deck
        /// </summary>
        public string Name;
        /// <summary>
        /// Deck code (hash) of the deck
        /// </summary>
        public string Hash;
        /// <summary>
        /// IDs of the cards on the main deck
        /// </summary>
        public int[] Main;
        /// <summary>
        /// IDs of the cards on the extra deck
        /// </summary>
        public int[] Extra;
        /// <summary>
        /// IDs of the cards on the side deck
        /// </summary>
        public int[] Side;
    }
}
