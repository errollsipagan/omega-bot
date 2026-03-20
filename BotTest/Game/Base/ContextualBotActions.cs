using BotTest.Game.Field;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.Base
{
    public abstract class ContextualBotActions : RuleBasedBotActions
    {
        protected enum PendingActionKind
        {
            None,
            Search,
            Reveal,
            Discard,
            SendToGrave,
            Revive,
            MaterialSelection,
            TargetSelection
        }

        protected struct PendingActionContext
        {
            public MainAction? Action;
            public int CardId;
            public int Description;
            public PendingActionKind Kind;
            public int ContextCode;
        }

        private int _trackedTurn = -1;
        private readonly HashSet<int> _activatedThisTurn = new();
        private readonly List<int> _chainDescriptions = new();

        protected PendingActionContext PendingAction { get; private set; }

        protected async Task PrepareContextAsync()
        {
            if (BotWrapper == null)
            {
                return;
            }

            var header = BotWrapper.CurrentStateHeader;
            if (header.CurrentTurn != _trackedTurn)
            {
                _trackedTurn = header.CurrentTurn;
                _activatedThisTurn.Clear();
                _chainDescriptions.Clear();
                PendingAction = default;
            }

            await _gameField.GetBotField();
        }

        protected void SetPendingAction(PendingActionKind kind, int cardId = 0, MainAction? action = null, int description = 0, int contextCode = 0)
        {
            PendingAction = new PendingActionContext
            {
                Kind = kind,
                CardId = cardId,
                Action = action,
                Description = description,
                ContextCode = contextCode
            };
        }

        protected void ClearPendingAction()
        {
            PendingAction = default;
        }

        protected void MarkCardActivated(int cardId)
        {
            if (cardId > 0)
            {
                _activatedThisTurn.Add(cardId);
            }
        }

        protected bool WasCardActivatedThisTurn(int cardId)
        {
            return _activatedThisTurn.Contains(cardId);
        }

        protected void RegisterChainDescriptions(IEnumerable<int> descriptions)
        {
            _chainDescriptions.Clear();
            _chainDescriptions.AddRange(descriptions);
        }

        protected bool IsFirstTurnOrMain2()
        {
            if (BotWrapper == null)
            {
                return false;
            }

            var header = BotWrapper.CurrentStateHeader;
            return header.CurrentTurn <= 1 || (DuelPhase)header.CurrentPhase == DuelPhase.Main2;
        }

        protected DynamicCard? FindSelectableByCardId(IEnumerable<DynamicCard> cards, params int[] ids)
        {
            var selectable = cards.FirstOrDefault(card => card.IsCode(ids));
            return selectable.ID > 0 ? selectable : null;
        }

        protected bool PendingContextIs(int contextCode) => PendingAction.ContextCode == contextCode;

        protected IList<byte>? PreferCardSelection(SelectCardData state, params int[] preferredIds)
        {
            var selected = state.SelectableCards
                .Select((card, index) => new { Card = card, Index = index })
                .Where(x => x.Card.IsCode(preferredIds))
                .Take(state.Minimum)
                .Select(x => (byte)x.Index)
                .ToArray();

            return selected.Length >= state.Minimum ? selected : null;
        }

        protected async Task<DynamicCard> GetBestEnemyMonster(bool onlyFaceUp = false)
        {
            return await _gameField.GetBestEnemyMonster(onlyFaceUp);
        }

        protected async Task<DynamicCard> GetBestEnemySpell(bool onlyFaceUp = false)
        {
            return await _gameField.GetBestEnemySpell(onlyFaceUp);
        }

        protected async Task<DynamicCard> GetBestEnemyCard(bool onlyFaceUp = false)
        {
            return await _gameField.GetBestEnemyCard(onlyFaceUp);
        }

        protected static bool CanBuildLevel8Synchro(IdleCmdData state)
        {
            return state.SpecialSummonableCards.Any(card => card.IsCode(69248256, 83755611, 43202238));
        }

        protected static bool CanBuildLevel10Synchro(IdleCmdData state)
        {
            return state.SpecialSummonableCards.Any(card => card.IsCode(96633955, 84815190, 47710198));
        }

        public override async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            await PrepareContextAsync();
            return await base.OnBattleCmd(state);
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            await PrepareContextAsync();
            RegisterChainDescriptions(state.Descriptions);
            return await base.OnSelectChain(state);
        }

        public override async Task<int> OnSelectOption(SelectOptionData state)
        {
            await PrepareContextAsync();
            return await base.OnSelectOption(state);
        }
    }
}
