namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Typed response wrapper for brain candidates.
    /// </summary>
    public interface ICandidateResponse
    {
        Type ResponseType { get; }
    }

    public interface ICandidateResponse<out TResponse> : ICandidateResponse
    {
        TResponse Value { get; }
    }

    public sealed record CandidateResponse<TResponse>(TResponse Value) : ICandidateResponse<TResponse>
    {
        public Type ResponseType => typeof(TResponse);
    }

    public static class CandidateResponse
    {
        public static CandidateResponse<TResponse> Create<TResponse>(TResponse value) => new(value);
    }
}
