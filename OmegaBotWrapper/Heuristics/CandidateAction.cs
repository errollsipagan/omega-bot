using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Represents one possible action the brain can choose from.
    /// </summary>
    public sealed record CandidateAction
    {
        public required BotState State { get; init; }

        public required CandidateActionKind Kind { get; init; }

        public required ICandidateResponse Response { get; init; }

        public DynamicCard? SourceCard { get; init; }

        public DynamicCard? TargetCard { get; init; }

        public int? ActionIndex { get; init; }

        public int? Description { get; init; }

        public int? Order { get; init; }

        public bool IsForced { get; init; }

        public bool IsLegal { get; init; } = true;

        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

        public IReadOnlyDictionary<string, object?> Metadata { get; init; } =
            new Dictionary<string, object?>();

        public TResponse GetResponse<TResponse>() =>
            Response is ICandidateResponse<TResponse> typedResponse
                ? typedResponse.Value
                : throw new InvalidOperationException(
                    $"Candidate response type mismatch. Expected {typeof(TResponse).Name}, actual {Response.ResponseType.Name}.");
    }
}
