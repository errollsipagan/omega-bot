using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Evaluates the current board for generic strategic signals.
    /// </summary>
    public interface IBoardEvaluator
    {
        int EvaluateBoardValue(FieldInfo? field);

        bool CanDetectLethal(FieldInfo? field, DynamicCard attacker);

        bool HasKnownDirectPath(FieldInfo? field);

        bool IsSafeAttack(DynamicCard attacker, DynamicCard defender);
    }
}
