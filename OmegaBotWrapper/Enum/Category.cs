namespace OmegaBotWrapper.Enum
{
    /// <summary>
    /// Hexdecimal mask for the different card categories
    /// </summary>
    [Flags]
    public enum Category
    {
        SkillCard = 0x1,
        SpeedSpellCard = 0x2,
        BossCard = 0x4,
        BetaCard = 0x8,
        ActionCard = 0x10,
        CommandCard = 0x20,
        DoubleScript = 0x40,
        RushLegendary = 0x80,
        PreErrata = 0x100,
        DarkCard = 0x200,
        DuelLinks = 0x400,
        RushCard = 0x800,
        StartCard = 0x1000,
        OneCard = 0x2000,
        TwoCard = 0x4000,
        ThreeCard = 0x8000,
        LevelZero = 0x10000,
        TreatedAs = 0x20000,
        BlueGod = 0x40000,
        YellowGod = 0x80000,
        RedGod = 0x100000,
        RushMax = 0x200000,
        SC = 0x400000,
        PendulumEffect = 0x800000
    }
}
