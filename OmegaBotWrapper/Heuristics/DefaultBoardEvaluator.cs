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
    }
}
