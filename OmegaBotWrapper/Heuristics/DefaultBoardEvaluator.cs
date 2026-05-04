using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Lightweight board evaluator using only the data currently exposed by the wrapper.
    /// </summary>
    public class DefaultBoardEvaluator : IBoardEvaluator
    {
        public virtual int EvaluateBoardValue(FieldInfo? field)
        {
            if (!field.HasValue)
                return 0;

            int ownMonsterCount = field.Value.OwnSide.MonsterZones?.Count(zone => zone.Cards?.Length > 0) ?? 0;
            int enemyMonsterCount = field.Value.EnemySide.MonsterZones?.Count(zone => zone.Cards?.Length > 0) ?? 0;
            int lifeDelta = field.Value.OwnSide.LifePoints - field.Value.EnemySide.LifePoints;
            return lifeDelta + ((ownMonsterCount - enemyMonsterCount) * 200);
        }

        public virtual bool CanDetectLethal(FieldInfo? field, DynamicCard attacker)
        {
            if (!field.HasValue || !attacker.CanDirectAttack)
                return false;

            return HasKnownDirectPath(field) && attacker.Attack >= field.Value.EnemySide.LifePoints;
        }

        public virtual bool HasKnownDirectPath(FieldInfo? field)
        {
            if (!field.HasValue)
                return false;

            return (field.Value.EnemySide.MonsterZones?.All(zone => zone.Cards == null || zone.Cards.Length == 0) ?? true);
        }

        public virtual bool IsSafeAttack(DynamicCard attacker, DynamicCard defender) =>
            attacker.Attack >= defender.GetRealPower();

        public virtual int EvaluateBattleTarget(DynamicCard attacker, DynamicCard defender, ICardEvaluator cardEvaluator)
        {
            if (defender.ID <= 0)
                return int.MinValue / 2;

            if (cardEvaluator.ShouldAvoidTargeting(defender))
                return int.MinValue / 4;

            int defenderPower = defender.GetRealPower();
            int attackDelta = attacker.Attack - defenderPower;

            if (cardEvaluator.IsInvincibleMonster(defender))
                return -20_000;

            int score = attackDelta * 10;

            if (cardEvaluator.IsFloodgate(defender))
                score += 8_000;

            if (cardEvaluator.IsDangerousMonster(defender))
                score -= 2_000;

            if (defender.IsFaceup() && defender.IsAttack())
                score += 1_500;
            else if (defender.IsDefense())
                score += 800;
            else
                score += 250;

            if (attackDelta > 0)
                score += 5_000;
            else if (attackDelta == 0)
                score -= 500;
            else
                score -= 10_000;

            return score;
        }

        public virtual int EvaluateRemovalTarget(DynamicCard target, ICardEvaluator cardEvaluator)
        {
            if (target.ID <= 0)
                return int.MinValue / 2;

            int score = target.GetRealPower();

            if (cardEvaluator.IsFloodgate(target))
                score += 10_000;

            if (cardEvaluator.IsDangerousMonster(target))
                score += 4_000;

            if (cardEvaluator.IsInvincibleMonster(target))
                score += 2_000;

            if (cardEvaluator.ShouldAvoidTargeting(target))
                score -= 12_000;

            return score;
        }
    }
}
