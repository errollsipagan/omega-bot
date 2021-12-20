namespace OmegaBotWrapper.Enum
{
    /// <summary>
    /// Contains all possible states the bot may be
    /// </summary>
    public enum BotState : byte
    {
        None,
        SelectBattleCmd,
        SelectCard,
        SelectUnselectCard,
        SelectChain,
        SelectCounter,
        SelectDisfield,
        SelectEffectYn,
        SelectIdleCmd,
        SelectOption,
        SelectPlace,
        SelectPosition,
        SelectSum,
        SelectTribute,
        AnnounceAttribute,
        AnnounceCard,
        AnnounceNumber,
        AnnounceRace,
        AnnounceCardFilter,
        WaitingForDeck,
        SelectYesNo,
        RockPaperScissors,
        SortCard
    }
}