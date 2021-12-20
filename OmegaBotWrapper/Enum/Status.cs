namespace OmegaBotWrapper.Enum
{
    /// <summary>
    /// Hexdecimal mask for card status
    /// </summary>
    [Flags]
    public enum Status
    {
        None = 0x0,
        Disabled = 0x0001,
        ToEnable = 0x0002,
        ToDisable = 0x0004,
        ProcComplete = 0x0008,
        SetTurn = 0x0010,
        NoLevel = 0x0020,
        BattleResult = 0x0040,
        SPSummonStep = 0x0080,
        FormChanged = 0x0100,
        Summoning = 0x0200,
        EffectEnabled = 0x0400,
        SummonTurn = 0x0800,
        DestroyConfirmed = 0x1000,
        LeaveConfirmed = 0x2000,
        BattleDestroyed = 0x4000,
        CopyingEffect = 0x8000,
        Chaining = 0x10000,
        SummonDisabled = 0x20000,
        ActivateDisabled = 0x40000,
        EffectReplaced = 0x80000,
        FutureFusion = 0x100000,
        AttackCanceled = 0x200000,
        Initializing = 0x400000,
        JustPos = 0x1000000,
        ContinuousPos = 0x2000000,
        Forbidden = 0x4000000,
        ActFromHand = 0x8000000,
        OppoBattle = 0x10000000,
        FlipSummonTurn = 0x20000000,
        SpSummonTurn = 0x40000000
    }
}
