using BotTest.Game.AI.Enums;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Heuristics;

/// <summary>
/// Lightweight card knowledge adapter that reuses broad utility lists already maintained
/// in BotTest and inspired by windbot's generic threat categories.
/// </summary>
public sealed class WindBotInspiredCardEvaluator : DefaultCardEvaluator
{
    private static readonly HashSet<int> Floodgates = Enum.GetValues<Floodgate>().Select(value => (int)value).ToHashSet();
    private static readonly HashSet<int> DangerousMonsters = Enum.GetValues<DangerousMonster>().Select(value => (int)value).ToHashSet();
    private static readonly HashSet<int> InvincibleMonsters = Enum.GetValues<InvincibleBotMonster>().Select(value => (int)value)
        .Concat(Enum.GetValues<InvincibleEnemyMonster>().Select(value => (int)value))
        .ToHashSet();
    private static readonly HashSet<int> AvoidTargeting = Enum.GetValues<ShouldNotBeTarget>().Select(value => (int)value)
        .Concat(Enum.GetValues<ShouldNotBeMonsterTarget>().Select(value => (int)value))
        .Concat(Enum.GetValues<ShouldNotBeSpellTrapTarget>().Select(value => (int)value))
        .ToHashSet();

    public override bool IsFloodgate(DynamicCard card) => Floodgates.Contains(GetStableId(card));

    public override bool IsDangerousMonster(DynamicCard card) => DangerousMonsters.Contains(GetStableId(card));

    public override bool IsInvincibleMonster(DynamicCard card) => InvincibleMonsters.Contains(GetStableId(card));

    public override bool ShouldAvoidTargeting(DynamicCard card) => AvoidTargeting.Contains(GetStableId(card));

    public override IReadOnlyList<string> GetKnowledgeTags(DynamicCard card)
    {
        List<string> tags = new();

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

    private static int GetStableId(DynamicCard card) => card.Alias != 0 ? card.Alias : card.ID;
}
