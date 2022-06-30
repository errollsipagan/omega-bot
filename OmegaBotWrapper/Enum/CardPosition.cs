namespace OmegaBotWrapper.Enum
{
    /// <summary>
    /// Hexdecimal mask for the position of an card
    /// </summary>
    public enum CardPosition
    {
        None = 0x0,
        FaceUpAttack = 0x1,
        FaceDownAttack = 0x2,
        FaceUpDefence = 0x4,
        FaceDownDefence = 0x8,
        FaceUp = 0x5,
        FaceDown = 0xA,
        Attack = 0x3,
        Defence = 0xC
    }
}
