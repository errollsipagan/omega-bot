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

        public virtual bool IsLikelyHandTrap(DynamicCard card) =>
            DefaultCardKnowledge.HandTrapMonsters.Contains(DefaultCardKnowledge.GetStableId(card));

        public virtual bool ShouldAvoidNormalSummon(DynamicCard card) => IsLikelyHandTrap(card);

        public virtual bool IsFloodgate(DynamicCard card) =>
            DefaultCardKnowledge.Floodgates.Contains(DefaultCardKnowledge.GetStableId(card));

        public virtual bool IsDangerousMonster(DynamicCard card) =>
            DefaultCardKnowledge.DangerousMonsters.Contains(DefaultCardKnowledge.GetStableId(card));

        public virtual bool IsInvincibleMonster(DynamicCard card) =>
            DefaultCardKnowledge.InvincibleMonsters.Contains(DefaultCardKnowledge.GetStableId(card));

        public virtual bool ShouldAvoidTargeting(DynamicCard card) =>
            DefaultCardKnowledge.AvoidTargeting.Contains(DefaultCardKnowledge.GetStableId(card));

        public virtual IReadOnlyList<string> GetArchetypeTags(DynamicCard card) => Array.Empty<string>();

        public virtual IReadOnlyList<string> GetComboTags(DynamicCard card) => Array.Empty<string>();

        public virtual IReadOnlyList<string> GetKnowledgeTags(DynamicCard card)
        {
            List<string> tags = new();

            if (IsLikelyHandTrap(card))
                tags.Add("hand-trap");
            if (IsFloodgate(card))
                tags.Add("floodgate");
            if (IsDangerousMonster(card))
                tags.Add("dangerous");
            if (IsInvincibleMonster(card))
                tags.Add("invincible");
            if (ShouldAvoidTargeting(card))
                tags.Add("avoid-target");

            return tags;
        }
    }
}
