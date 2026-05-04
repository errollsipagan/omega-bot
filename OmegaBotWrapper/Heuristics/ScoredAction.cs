namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Candidate action plus the score and trace produced by the scorer.
    /// </summary>
    public sealed record ScoredAction
    {
        public required CandidateAction Action { get; init; }

        public required int Score { get; init; }

        public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();
    }
}
