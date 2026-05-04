namespace OmegaBotWrapper.Heuristics.Examples
{
    /// <summary>
    /// Example scorer that nudges the default brain toward a simple aggressive plan.
    /// This is intentionally lightweight and is meant as a template for future deck brains.
    /// </summary>
    public sealed class ExampleAggroActionScorer : DefaultActionScorer
    {
        public ExampleAggroActionScorer(ICardEvaluator cardEvaluator, IBoardEvaluator boardEvaluator)
            : base(cardEvaluator, boardEvaluator)
        {
        }

        public override ScoredAction Score<TState>(DecisionContext<TState> context, CandidateAction candidate)
        {
            var scored = base.Score(context, candidate);
            int score = scored.Score;
            var reasons = scored.Reasons.ToList();

            if (candidate.Kind == CandidateActionKind.Summon && candidate.SourceCard is { } summon)
            {
                score += summon.Attack / 5;
                reasons.Add("example-aggro:prefer-strong-summon");
            }

            if (candidate.Kind == CandidateActionKind.Attack && candidate.SourceCard is { } attacker)
            {
                score += attacker.Attack / 40;
                reasons.Add("example-aggro:press-battle");
            }

            if (candidate.Kind == CandidateActionKind.ToEndPhase)
            {
                score -= 50;
                reasons.Add("example-aggro:delay-ending");
            }

            return scored with
            {
                Score = score,
                Reasons = reasons
            };
        }
    }
}
