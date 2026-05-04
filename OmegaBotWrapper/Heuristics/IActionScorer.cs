namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Scores candidate actions so a brain can rank and select them.
    /// </summary>
    public interface IActionScorer
    {
        ScoredAction Score<TState>(DecisionContext<TState> context, CandidateAction candidate);
    }
}
