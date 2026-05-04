using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Evaluates individual cards for generic heuristics.
    /// </summary>
    public interface ICardEvaluator
    {
        int EvaluateOffense(DynamicCard card);

        int EvaluateDefense(DynamicCard card);

        bool IsActionable(DynamicCard card);

        bool IsLikelyHandTrap(DynamicCard card);

        bool ShouldAvoidNormalSummon(DynamicCard card);

        bool IsFloodgate(DynamicCard card);

        bool IsDangerousMonster(DynamicCard card);

        bool IsInvincibleMonster(DynamicCard card);

        bool ShouldAvoidTargeting(DynamicCard card);

        IReadOnlyList<string> GetArchetypeTags(DynamicCard card);

        IReadOnlyList<string> GetComboTags(DynamicCard card);

        IReadOnlyList<string> GetKnowledgeTags(DynamicCard card);
    }
}
