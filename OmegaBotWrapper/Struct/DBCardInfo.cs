namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Information about a specific card on client database
    /// </summary>
    public struct DBCardInfo
    {
        /// <summary>
        /// Card id (code)
        /// </summary>
        public int ID;
        /// <summary>
        /// Card OT (tcg,ocg,wcs,dl)
        /// </summary>
        public byte OT;
        /// <summary>
        /// Card alias pointing to original ID in case of alt arts and other special cases
        /// </summary>
        public int Alias;
        /// <summary>
        /// Card set codes (8 bytes, every 2 bytes is a setcode)
        /// </summary>
        public long SetCode;
        /// <summary>
        /// Card type bitmask, defines what kind of card it is
        /// </summary>
        public int Type;
        /// <summary>
        /// Card level (or rank)
        /// </summary>
        public sbyte Level;
        /// <summary>
        /// Pendulum Left Scale
        /// </summary>
        public byte LScale;
        /// <summary>
        /// Pendulum Right Scale
        /// </summary>
        public byte RScale;
        /// <summary>
        /// Link markers bitmask of this card
        /// </summary>
        public ushort LinkMarker;
        /// <summary>
        /// Card special category flags
        /// </summary>
        public int Category;
        /// <summary>
        /// When was released on TCG
        /// </summary>
        public DateTime TCGReleaseDate;
        /// <summary>
        /// When was released on OCG
        /// </summary>
        public DateTime OCGReleaseDate;
        /// <summary>
        /// Card attribute bitmask
        /// </summary>
        public byte Attribute;
        /// <summary>
        /// Card race bitmask
        /// </summary>
        public int Race;
        /// <summary>
        /// Card original attack printed on card
        /// </summary>
        public int Attack;
        /// <summary>
        /// Card original defense printed on card
        /// </summary>
        public int Defense;
        /// <summary>
        /// Sets this card belong to
        /// </summary>
        public string SetID;
        /// <summary>
        /// Genre bitmask this card belongs to
        /// </summary>
        public long Genre;
        /// <summary>
        /// Support setcodes this card has relations to (8 bytes, 2 bytes each setcode)
        /// </summary>
        public long Support;
        /// <summary>
        /// Card's name on current client language
        /// </summary>
        public string Name;
        /// <summary>
        /// Card's text on current client language
        /// </summary>
        public string CardText;
        /// <summary>
        /// Card's pendulum text on current client language
        /// </summary>
        public string PendulumText;
        /// <summary>
        /// ID of the cover (sleeve) of this card
        /// </summary>
        public int Cover;
    }
}
