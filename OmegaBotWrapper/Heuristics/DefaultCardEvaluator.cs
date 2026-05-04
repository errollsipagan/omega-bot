using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Conservative default card evaluator. It intentionally knows very little,
    /// but exposes clear override points for richer card knowledge later.
    /// </summary>
    public class DefaultCardEvaluator : ICardEvaluator
    {
        public virtual int EvaluateOffense(DynamicCard card) => card.Attack;

        public virtual int EvaluateDefense(DynamicCard card) => card.GetRealPower();

        public virtual bool IsActionable(DynamicCard card) => card.ID > 0 && !card.IsDisabled();

        public virtual bool IsLikelyHandTrap(DynamicCard card) => false;

        public virtual IReadOnlyList<string> GetArchetypeTags(DynamicCard card) => Array.Empty<string>();

        public virtual IReadOnlyList<string> GetComboTags(DynamicCard card) => Array.Empty<string>();

        public virtual IReadOnlyList<string> GetKnowledgeTags(DynamicCard card) => Array.Empty<string>();
    }
}
