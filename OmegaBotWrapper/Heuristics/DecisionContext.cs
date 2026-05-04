using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Snapshot of the current decision point passed into a bot brain.
    /// </summary>
    public sealed record DecisionContext<TState>
    {
        public required DuelBotWrapper Bot { get; init; }

        public required BotState State { get; init; }

        public required BotStateHeader Header { get; init; }

        public required TState StateData { get; init; }

        /// <summary>
        /// Optional board snapshot for heuristic evaluation.
        /// This can be left null when callers want zero extra API work.
        /// </summary>
        public FieldInfo? Field { get; init; }

        /// <summary>
        /// Extension point for richer context such as chain metadata, card knowledge,
        /// archetype profile hints, or precomputed board analysis.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Features { get; init; } =
            new Dictionary<string, object?>();
    }
}
