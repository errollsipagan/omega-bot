using BotTest.Game.Field;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

#pragma warning disable 1998
#pragma warning disable 8602

namespace BotTest.Game.Base
{
    /// <summary>
    /// Field-aware base class for BotTest decks.
    /// Generic fallback behavior lives in OmegaBotWrapper.BotActionsBase;
    /// this class keeps only the extra field-aware logic that deck bots rely on.
    /// </summary>
    public abstract class FieldBotActions : BotActionsBase
    {
        /// <summary>
        /// GameField instance to access field information
        /// </summary>
        protected GameField _gameField = null!;

        // Store the selected defender identity for ATTACKTARGET selection.
        private DynamicCard? _selectedDefenderCard;

        public override void RegisterActions(DuelBotWrapper bot)
        {
            _gameField = new GameField(bot);
            base.RegisterActions(bot);
        }

        public override async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            ConsoleUtils.PrintColored("OnBattleCmd - Generic attack evaluation");

            var attackableCards = state.AttackableCards
                .Where(card => !card.Attacked && !card.IsDisabled())
                .OrderByDescending(card => card.Attack)
                .ToList();

            if (!attackableCards.Any())
            {
                ConsoleUtils.PrintColored("No attackable cards available.");
                _selectedDefenderCard = null;
                return state.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
            }

            var opponentMonsters = (await _gameField.GetPlayerCardsInZone(CardLocation.MonsterZone))
                .Where(card => !card.IsDisabled())
                .ToList();

            if (!opponentMonsters.Any())
            {
                var directAttacker = attackableCards
                    .Where(card => card.CanDirectAttack)
                    .OrderByDescending(card => card.Attack)
                    .FirstOrDefault();

                if (directAttacker.ID > 0 && state.AttackableCards.TryGetByID(directAttacker.ID, out _, out int index))
                {
                    ConsoleUtils.PrintColored($"Direct Attack with Card ID: {directAttacker.ID}, Attack: {directAttacker.Attack}, Index: {index}");
                    _selectedDefenderCard = null;
                    return state.AttackableCards[index].DeclareAttack();
                }

                ConsoleUtils.PrintColored("No direct attack possible.");
                _selectedDefenderCard = null;
                return state.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
            }

            var bestBattle = attackableCards
                .SelectMany(attacker => opponentMonsters.Select(target => new
                {
                    Attacker = attacker,
                    Target = target,
                    AttackerPower = attacker.GetRealPower(),
                    Score = GetAttackScore(attacker, target)
                }))
                .Where(option => option.Score > 0)
                .OrderByDescending(option => option.Score)
                .ThenByDescending(option => option.AttackerPower)
                .FirstOrDefault();

            if (bestBattle != null &&
                state.AttackableCards.TryGetByID(bestBattle.Attacker.ID, out _, out int attackerIndex))
            {
                ConsoleUtils.PrintColored($"Attacking target ID {bestBattle.Target.ID} with attacker ID {bestBattle.Attacker.ID}");
                _selectedDefenderCard = bestBattle.Target;
                return state.AttackableCards[attackerIndex].DeclareAttack();
            }

            var facedownDefenseTarget = opponentMonsters
                .Where(target => !target.IsFaceup() && target.IsDefense())
                .OrderBy(target => target.CurrentCoordinates.Sequence)
                .FirstOrDefault();

            if (facedownDefenseTarget.ID > 0)
            {
                var opportunisticAttacker = attackableCards
                    .OrderByDescending(card => card.Attack)
                    .FirstOrDefault();

                if (opportunisticAttacker.ID > 0 &&
                    state.AttackableCards.TryGetByID(opportunisticAttacker.ID, out _, out int opportunisticAttackerIndex))
                {
                    ConsoleUtils.PrintColored($"Taking the opportunity to attack facedown defense target ID {facedownDefenseTarget.ID} with attacker ID {opportunisticAttacker.ID}");
                    _selectedDefenderCard = facedownDefenseTarget;
                    return state.AttackableCards[opportunisticAttackerIndex].DeclareAttack();
                }
            }

            ConsoleUtils.PrintColored("No advantageous attack targets found.");
            _selectedDefenderCard = null;
            return state.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
        }

        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            ConsoleUtils.PrintColored("[FIELD BOT ACTIONS] OnIdleCmd");

            var opponentMonsters = (await _gameField.GetPlayerCardsInZone(CardLocation.MonsterZone))
                .Where(card => !card.IsDisabled())
                .ToList();

            var repositionToDefense = state.ReposableCards
                .Where(card => card.IsFaceup() && card.IsAttack() && ShouldRepositionToDefense(card, opponentMonsters))
                .OrderBy(card => card.Attack)
                .FirstOrDefault();

            if (repositionToDefense.ID > 0)
            {
                ConsoleUtils.PrintColored($"Repositioning to defense: {repositionToDefense.ID}");
                return repositionToDefense.Reposition();
            }

            var repositionToAttack = state.ReposableCards
                .Where(card => card.IsDefense() && ShouldRepositionToAttack(card, opponentMonsters))
                .OrderByDescending(card => card.Attack)
                .FirstOrDefault();

            if (repositionToAttack.ID > 0)
            {
                ConsoleUtils.PrintColored($"Repositioning to attack: {repositionToAttack.ID}");
                return repositionToAttack.Reposition();
            }

            return await base.OnIdleCmd(state);
        }

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            ConsoleUtils.PrintColored($"[FIELD BOT ACTIONS] [OnSelectCard] => {(DHint)state.SelectionHint}");

            if (state.SelectionHint == (int)DHint.TARGET)
            {
                byte[] botCards = state.SelectableCards
                    .Select((card, index) => new { Card = card, Index = index })
                    .OrderByDescending(x => x.Card.Attack)
                    .Take(state.Minimum)
                    .Select(x => (byte)x.Index)
                    .ToArray();

                if (botCards.Any())
                {
                    ConsoleUtils.PrintColored($"Selected {botCards.Length} cards for TARGET.");
                    return botCards;
                }
            }

            if (state.SelectionHint == (int)DHint.ATTACKTARGET)
            {
                ConsoleUtils.PrintColored("OnSelectCard - Selecting Defender");

                if (_selectedDefenderCard.HasValue)
                {
                    var selectedDefender = _selectedDefenderCard.Value;
                    if (state.SelectableCards.TryGetByIDWithZone(selectedDefender.ID, selectedDefender.CurrentCoordinates.Sequence, out _, out int selectedIndex))
                    {
                        ConsoleUtils.PrintColored($"Selecting defender by card code and sequence: {selectedDefender.ID} / {selectedDefender.CurrentCoordinates.Sequence}");
                        return new List<byte> { (byte)selectedIndex };
                    }
                }
            }

            if (state.SelectionHint is (int)DHint.DESTROY or (int)DHint.REMOVE)
            {
                string actionName = state.SelectionHint == (int)DHint.DESTROY ? "DESTROY" : "REMOVE";
                ConsoleUtils.PrintColored($"OnSelectCard - Selecting Card to {actionName}");

                byte[] playerCards = state.SelectableCards
                    .Select((card, index) => new { Card = card, Index = index })
                    .Where(x => x.Card.Owner == 1)
                    .Take(state.Minimum)
                    .Select(x => (byte)x.Index)
                    .ToArray();

                if (playerCards.Any())
                {
                    ConsoleUtils.PrintColored($"Selected {playerCards.Length} cards for {actionName}.");
                    return playerCards;
                }
            }

            ConsoleUtils.PrintColored("No specific defender index available, using default selection.");
            return await base.OnSelectCard(state);
        }

        public override async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            ConsoleUtils.PrintColored("[FIELD BOT ACTIONS] OnAnnounceCard");
            var cards = await DuelBotWrapper.GetMatchingCards(state.AppliableFilters);
            return BotActionUtilities.SelectRandomAnnouncedCard(cards, preferAlias: true);
        }

        protected static bool ShouldRepositionToAttack(DynamicCard card, IEnumerable<DynamicCard> opponentMonsters)
        {
            var opponents = opponentMonsters.ToList();
            if (!opponents.Any())
                return true;

            return opponents.Any(opponent => GetAttackScore(card, opponent) > 0);
        }

        protected static bool ShouldRepositionToDefense(DynamicCard card, IEnumerable<DynamicCard> opponentMonsters)
        {
            int strongestOpponentAttack = opponentMonsters
                .Where(opponent => opponent.IsFaceup() && opponent.IsAttack())
                .Select(opponent => opponent.Attack)
                .DefaultIfEmpty(0)
                .Max();

            return strongestOpponentAttack > card.Attack;
        }

        protected static int GetAttackScore(DynamicCard attacker, DynamicCard defender)
        {
            int attackerPower = attacker.Attack;
            int defenderPower = defender.GetRealPower();

            if (attackerPower <= defenderPower)
                return 0;

            int score = 10_000 + defenderPower;

            if (defender.IsFaceup() && defender.IsAttack())
                score += 2_000;
            else if (defender.IsDefense())
                score += 1_000;

            score += attackerPower - defenderPower;
            return score;
        }

        public DuelPhase GetCurrentDuelPhase()
        {
            ConsoleUtils.PrintColored($"Current Duel Phase: {(DuelPhase)BotWrapper!.CurrentStateHeader.CurrentPhase}");
            return (DuelPhase)BotWrapper.CurrentStateHeader.CurrentPhase;
        }

        public BotState GetCurrentBotState()
        {
            ConsoleUtils.PrintColored($"Current Bot State: {(BotState)BotWrapper!.CurrentStateHeader.CurrentStateCode}");
            return (BotState)BotWrapper.CurrentStateHeader.CurrentStateCode;
        }

        public string GetCurrentStateName()
        {
            ConsoleUtils.PrintColored($"Current State Name: {BotWrapper!.CurrentStateHeader.CurrentStateName}");
            return BotWrapper.CurrentStateHeader.CurrentStateName;
        }

        public int GetCurrentPlayer()
        {
            ConsoleUtils.PrintColored($"Current Player: {BotWrapper!.CurrentStateHeader.CurrentPlayer}");
            return BotWrapper.CurrentStateHeader.CurrentPlayer;
        }
    }
}
