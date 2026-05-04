namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Strongly-typed action categories used by heuristic candidates.
    /// </summary>
    public enum CandidateActionKind
    {
        Attack,
        ToMainPhaseTwo,
        ToEndPhase,
        Summon,
        ToBattlePhase,
        ActivateChain,
        PassChain
    }
}
