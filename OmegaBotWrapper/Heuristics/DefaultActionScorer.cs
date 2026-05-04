using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Conservative heuristic scorer. It prefers legal actions, avoids optional
    /// chains by default, and leaves rich card-specific logic to future overrides.
    /// </summary>
    public class DefaultActionScorer : IActionScorer
    {
        private readonly ICardEvaluator _cardEvaluator;
        private readonly IBoardEvaluator _boardEvaluator;

        public DefaultActionScorer(ICardEvaluator cardEvaluator, IBoardEvaluator boardEvaluator)
        {
            _cardEvaluator = cardEvaluator;
            _boardEvaluator = boardEvaluator;
        }

        public virtual ScoredAction Score<TState>(DecisionContext<TState> context, CandidateAction candidate)
        {
            var reasons = new List<string>();
            int score = 0;

            if (!candidate.IsLegal)
            {
                reasons.Add("illegal");
                return new ScoredAction { Action = candidate, Score = int.MinValue / 2, Reasons = reasons };
            }

            score += 1000;
            reasons.Add("legal");

            switch (candidate.State)
            {
                case BotState.SelectBattleCmd:
                    ScoreBattle(context, candidate, reasons, ref score);
                    break;

                case BotState.SelectIdleCmd:
                    ScoreIdle(context, candidate, reasons, ref score);
                    break;

                case BotState.SelectChain:
                    ScoreChain(candidate, reasons, ref score);
                    break;
            }

            if (candidate.SourceCard is DynamicCard source)
            {
                score += _cardEvaluator.EvaluateOffense(source) / 100;
                foreach (var tag in _cardEvaluator.GetArchetypeTags(source))
                    reasons.Add($"archetype:{tag}");
                foreach (var tag in _cardEvaluator.GetComboTags(source))
                    reasons.Add($"combo:{tag}");
                foreach (var tag in _cardEvaluator.GetKnowledgeTags(source))
                    reasons.Add($"knowledge:{tag}");
                if (_cardEvaluator.IsLikelyHandTrap(source))
                    score += ScoreHandTrapWindow(context, candidate, source, reasons);
            }

            score += _boardEvaluator.EvaluateBoardValue(context.Field) / 1000;
            score += GetOrderBias(candidate);

            return new ScoredAction
            {
                Action = candidate,
                Score = score,
                Reasons = reasons
            };
        }

        protected virtual int ScoreHandTrapWindow<TState>(
            DecisionContext<TState> context,
            CandidateAction candidate,
            DynamicCard source,
            IList<string> reasons)
        {
            reasons.Add("hand-trap-hook");
            return 0;
        }

        private void ScoreBattle<TState>(DecisionContext<TState> context, CandidateAction candidate, IList<string> reasons, ref int score)
        {
            if (candidate.Kind == CandidateActionKind.Attack)
            {
                score += 200;
                reasons.Add("prefer-attack");

                if (candidate.SourceCard is DynamicCard attacker)
                {
                    if (_boardEvaluator.CanDetectLethal(context.Field, attacker))
                    {
                        score += 5000;
                        reasons.Add("detectable-lethal");
                    }
                    else if (attacker.CanDirectAttack && _boardEvaluator.HasKnownDirectPath(context.Field))
                    {
                        score += 500;
                        reasons.Add("direct-path");
                    }
                }

                if (candidate.TargetCard is DynamicCard defender &&
                    candidate.SourceCard is DynamicCard source &&
                    _boardEvaluator.IsSafeAttack(source, defender))
                {
                    score += 300;
                    reasons.Add("safe-attack");
                }

                return;
            }

            if (candidate.Kind == CandidateActionKind.ToMainPhaseTwo)
            {
                score += 25;
                reasons.Add("preserve-main2");
                return;
            }

            if (candidate.Kind == CandidateActionKind.ToEndPhase)
            {
                score += 10;
                reasons.Add("fallback-end");
            }
        }

        private void ScoreIdle<TState>(DecisionContext<TState> context, CandidateAction candidate, IList<string> reasons, ref int score)
        {
            switch (candidate.Kind)
            {
                case CandidateActionKind.Summon:
                    score += 400;
                    reasons.Add("prefer-summon");
                    score += GetStableOrderBias(candidate, 100);
                    reasons.Add("preserve-default-summon-order");
                    break;

                case CandidateActionKind.ToBattlePhase:
                    score += 100;
                    reasons.Add("prefer-battle-when-idle");
                    break;

                case CandidateActionKind.ToEndPhase:
                    score += 10;
                    reasons.Add("idle-fallback-end");
                    break;
            }
        }

        private void ScoreChain(CandidateAction candidate, IList<string> reasons, ref int score)
        {
            if (candidate.IsForced)
            {
                score += 5000;
                reasons.Add("forced-chain");
            }

            if (candidate.Kind == CandidateActionKind.PassChain)
            {
                score += 200;
                reasons.Add("avoid-optional-chain");
                return;
            }

            if (candidate.Kind == CandidateActionKind.ActivateChain)
            {
                score -= candidate.IsForced ? 0 : 100;
                reasons.Add(candidate.IsForced ? "resolve-forced-chain" : "optional-chain-default-low");
                if (candidate.IsForced)
                {
                    score += GetStableOrderBias(candidate, 100);
                    reasons.Add("preserve-default-chain-order");
                }
            }
        }

        private static int GetOrderBias(CandidateAction candidate)
        {
            if (candidate.Order is not int order)
                return 0;
            return Math.Max(0, 20 - order);
        }

        private static int GetStableOrderBias(CandidateAction candidate, int multiplier)
        {
            if (candidate.Order is not int order)
                return 0;
            return Math.Max(0, 1000 - (order * multiplier));
        }
    }
}
