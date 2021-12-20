namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Information about where a card is located on the field
    /// </summary>
    public struct CardCoordinate
    {
        /// <summary>
        /// Player who controls the card
        /// </summary>
        public byte Controller;
        /// <summary>
        /// Location where the card is (eg. monster zone)
        /// </summary>
        public byte Location;
        /// <summary>
        /// Slot sequence in which the card is located at
        /// </summary>
        public byte Sequence;
    }
}
