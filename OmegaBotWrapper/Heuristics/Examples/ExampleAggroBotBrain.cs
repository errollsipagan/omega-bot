namespace OmegaBotWrapper.Heuristics.Examples
{
    /// <summary>
    /// Example of a future deck-specific brain.
    /// Consumers can instantiate DuelBotWrapper with this brain directly:
    /// new DuelBotWrapper(deckName, deckHash, new ExampleAggroBotBrain()).
    /// </summary>
    public sealed class ExampleAggroBotBrain : DefaultBotBrain
    {
        public ExampleAggroBotBrain()
            : this(new DefaultCardEvaluator(), new DefaultBoardEvaluator())
        {
        }

        public ExampleAggroBotBrain(ICardEvaluator cardEvaluator, IBoardEvaluator boardEvaluator)
            : base(cardEvaluator, boardEvaluator, new ExampleAggroActionScorer(cardEvaluator, boardEvaluator))
        {
        }
    }
}
