using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Heuristic-ready decision model. Existing bots can keep using IBotActions,
    /// while new bots can implement this directly.
    /// </summary>
    public interface IBotBrain
    {
        Task<BattleCmdResponse> OnBattleCmdAsync(DecisionContext<BattleCmdData> context, CancellationToken ct = default);
        Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default);
        Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default);
        Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default);
        Task<IList<byte>?> OnSelectTributeAsync(DecisionContext<SelectTributeData> context, CancellationToken ct = default);
        Task<IList<byte>?> OnSelectSumAsync(DecisionContext<SelectSumData> context, CancellationToken ct = default);
        Task<IList<CardAttribute>> OnAnnounceAttributeAsync(DecisionContext<AnnounceAttributeData> context, CancellationToken ct = default);
        Task<IList<CardRace>> OnAnnounceRaceAsync(DecisionContext<AnnounceRaceData> context, CancellationToken ct = default);
        Task<int> OnAnnounceNumberAsync(DecisionContext<AnnounceNumberData> context, CancellationToken ct = default);
        Task<int> OnAnnounceCardAsync(DecisionContext<AnnounceCardData> context, CancellationToken ct = default);
        Task<int> OnAnnounceCardFilterAsync(DecisionContext<AnnounceCardFilterData> context, CancellationToken ct = default);
        Task<IList<byte>?> OnSelectUnselectCardAsync(DecisionContext<SelectUnselectCardData> context, CancellationToken ct = default);
        Task<IList<short>> OnSelectCounterAsync(DecisionContext<SelectCounterData> context, CancellationToken ct = default);
        Task<int> OnSelectDisfieldAsync(DecisionContext<SelectDisfieldData> context, CancellationToken ct = default);
        Task<int> OnSelectPlaceAsync(DecisionContext<SelectPlaceData> context, CancellationToken ct = default);
        Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default);
        Task<bool> OnSelectYesNoAsync(DecisionContext<SelectYesNoData> context, CancellationToken ct = default);
        Task<int> OnSelectOptionAsync(DecisionContext<SelectOptionData> context, CancellationToken ct = default);
        Task<byte> OnSelectRockPaperScissorsAsync(CancellationToken ct = default);
        Task<CardPosition> OnSelectPositionAsync(DecisionContext<SelectPositionData> context, CancellationToken ct = default);
        Task<IList<byte>> OnSortCardAsync(DecisionContext<SortCardData> context, CancellationToken ct = default);
    }
}
