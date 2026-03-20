using BotTest.Decks;
using BotTest.Game.Base;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.SwordSoul
{
    public class SwordSoulActions : ContextualBotActions
    {
        private static readonly int[] MoYeRevealPriority =
        {
            SwordSoulCardId.SwordsoulStrategistLongYuan,
            SwordSoulCardId.SwordsoulOfTaia,
            SwordSoulCardId.TenyiAshuna,
            SwordSoulCardId.TenyiVishuda,
            SwordSoulCardId.TenyiAdhara,
            SwordSoulCardId.SwordsoulBlackout,
            SwordSoulCardId.SwordsoulEmergence,
            SwordSoulCardId.SwordsoulSummit
        };

        private static readonly int[] LongyuanDiscardPriority =
        {
            SwordSoulCardId.SwordsoulBlackout,
            SwordSoulCardId.SwordsoulEmergence,
            SwordSoulCardId.SwordsoulSummit,
            SwordSoulCardId.TenyiAdhara,
            SwordSoulCardId.TenyiVishuda,
            SwordSoulCardId.TenyiAshuna,
            SwordSoulCardId.SwordsoulOfTaia,
            SwordSoulCardId.SwordsoulOfMoYe
        };

        private static readonly int[] TaiaSendPriority =
        {
            SwordSoulCardId.SwordsoulOfMoYe,
            SwordSoulCardId.TenyiAdhara,
            SwordSoulCardId.TenyiVishuda,
            SwordSoulCardId.TenyiAshuna
        };

        public SwordSoulActions(MasterDuelDeck deckId = MasterDuelDeck.SwordSoul)
        {
            DeckName = DeckManager.GetDeckName(deckId);
            DeckHash = DeckManager.GetDeckHash(deckId);
            InitializeCardInfo<SwordSoulCardId>();
        }

        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            await PrepareContextAsync();

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulOfMoYe, out var moYe) && await HasRevealForMoYe())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Mo Ye.");
                SetPendingAction(PendingSelectionReason.RevealForMoYe, moYe.ID, MainAction.Activate);
                MarkCardActivated(moYe.ID);
                return moYe.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.SwordsoulGrandmasterChixiao, out var chixiao))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Chixiao.");
                SetPendingAction(PendingSelectionReason.SummonLevel8, chixiao.ID, MainAction.SpSummon);
                return chixiao.SpecialSummon();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulGrandmasterChixiao, out var chixiaoEffect) &&
                !WasCardActivatedThisTurn(chixiaoEffect.ID))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Chixiao.");
                SetPendingAction(PendingSelectionReason.SearchFollowUp, chixiaoEffect.ID, MainAction.Activate);
                MarkCardActivated(chixiaoEffect.ID);
                return chixiaoEffect.MainActivateFromIndex();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulStrategistLongYuan, out var longyuan) &&
                await TryGetLongyuanDiscardCandidate())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Longyuan.");
                SetPendingAction(PendingSelectionReason.DiscardForLongyuan, longyuan.ID, MainAction.Activate);
                MarkCardActivated(longyuan.ID);
                return longyuan.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.BaronneDeFluer, out var baronne) && await ShouldPreferBaronne())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Baronne.");
                SetPendingAction(PendingSelectionReason.SummonLevel10, baronne.ID, MainAction.SpSummon);
                return baronne.SpecialSummon();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.SwordsoulChengying, out var chengying))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Chengying.");
                SetPendingAction(PendingSelectionReason.SummonLevel10, chengying.ID, MainAction.SpSummon);
                return chengying.SpecialSummon();
            }

            if (TryGetSummonable(state, SwordSoulCardId.SwordsoulOfMoYe, out var moYeSummon) && await HasRevealForMoYe())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Mo Ye.");
                return moYeSummon.NormalSummon();
            }

            if (TryGetSummonable(state, SwordSoulCardId.SwordsoulOfTaia, out var taiaSummon) && await HasTaiaBanishTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Taia.");
                return taiaSummon.NormalSummon();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulOfTaia, out var taiaActivate) &&
                await HasTaiaBanishTarget() &&
                !WasCardActivatedThisTurn(taiaActivate.ID))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Taia.");
                SetPendingAction(PendingSelectionReason.SendToGraveForTaia, taiaActivate.ID, MainAction.Activate);
                MarkCardActivated(taiaActivate.ID);
                return taiaActivate.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.YangZingBaxia, out var baxia) && await HasWorthwhileEnemyTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Baxia.");
                SetPendingAction(PendingSelectionReason.SummonLevel8, baxia.ID, MainAction.SpSummon);
                return baxia.SpecialSummon();
            }

            if (TryGetActivable(state, SwordSoulCardId.YangZingBaxia, out var baxiaEffect) &&
                !WasCardActivatedThisTurn(baxiaEffect.ID) &&
                await HasWorthwhileEnemyTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Baxia.");
                SetPendingAction(PendingSelectionReason.BaxiaTarget, baxiaEffect.ID, MainAction.Activate);
                MarkCardActivated(baxiaEffect.ID);
                return baxiaEffect.MainActivateFromIndex();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulEmergence, out var emergence) &&
                await ShouldUseEmergence())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Emergence.");
                SetPendingAction(GetEmergenceReason(), emergence.ID, MainAction.Activate);
                MarkCardActivated(emergence.ID);
                return emergence.MainActivateFromIndex();
            }

            if (TryGetSpellSettable(state, SwordSoulCardId.SwordsoulBlackout, out var blackoutSet) && await ShouldSetBlackout())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Setting Blackout.");
                return blackoutSet.SetSpellTrap();
            }

            return await base.OnIdleCmd(state);
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            await PrepareContextAsync();
            RegisterChainDescriptions(state.Descriptions);

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulGrandmasterChixiao, out _, out var index))
            {
                SetPendingAction(PendingSelectionReason.SearchFollowUp, SwordSoulCardId.SwordsoulGrandmasterChixiao, MainAction.Activate);
                MarkCardActivated(SwordSoulCardId.SwordsoulGrandmasterChixiao);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulBlackout, out _, out index) && await HasWorthwhileEnemyTarget())
            {
                SetPendingAction(PendingSelectionReason.BlackoutTarget, SwordSoulCardId.SwordsoulBlackout, MainAction.Activate);
                MarkCardActivated(SwordSoulCardId.SwordsoulBlackout);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.YangZingBaxia, out _, out index) && await HasWorthwhileEnemyTarget())
            {
                SetPendingAction(PendingSelectionReason.BaxiaTarget, SwordSoulCardId.YangZingBaxia, MainAction.Activate);
                MarkCardActivated(SwordSoulCardId.YangZingBaxia);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulOfTaia, out _, out index))
            {
                SetPendingAction(PendingSelectionReason.SendToGraveForTaia, SwordSoulCardId.SwordsoulOfTaia, MainAction.Activate);
                MarkCardActivated(SwordSoulCardId.SwordsoulOfTaia);
                return index;
            }

            return await base.OnSelectChain(state);
        }

        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            await PrepareContextAsync();

            if (state.EffectCard.IsCode(SwordSoulCardId.SwordsoulOfMoYe))
            {
                return await HasRevealForMoYe();
            }

            if (state.EffectCard.IsCode(SwordSoulCardId.SwordsoulOfTaia))
            {
                return await HasTaiaBanishTarget();
            }

            if (state.EffectCard.IsCode(
                SwordSoulCardId.SwordsoulGrandmasterChixiao,
                SwordSoulCardId.SwordsoulStrategistLongYuan,
                SwordSoulCardId.YangZingBaxia,
                SwordSoulCardId.SwordsoulBlackout))
            {
                return true;
            }

            return await base.OnSelectEffectYn(state);
        }

        public override async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            await PrepareContextAsync();

            if (state.CardID == SwordSoulCardId.SwordsoulToken ||
                state.CardID == SwordSoulCardId.YangZingBaxia ||
                state.CardID == SwordSoulCardId.SwordsoulOfMoYe)
            {
                return PickPosition(state, CardPosition.FaceUpDefence);
            }

            if (state.CardID == SwordSoulCardId.BaronneDeFluer ||
                state.CardID == SwordSoulCardId.SwordsoulChengying ||
                state.CardID == SwordSoulCardId.SwordsoulGrandmasterChixiao)
            {
                if (IsFirstTurnOrMain2())
                {
                    return PickPosition(state, CardPosition.FaceUpDefence);
                }

                var bestEnemy = await GetBestEnemyMonster(true);
                if (bestEnemy.ID > 0 && bestEnemy.Attack > GetExpectedAttack(state.CardID))
                {
                    return PickPosition(state, CardPosition.FaceUpDefence);
                }

                return PickPosition(state, CardPosition.FaceUpAttack);
            }

            return await base.OnSelectPosition(state);
        }

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            await PrepareContextAsync();
            ConsoleUtils.PrintColored($"[SWORDSOUL] OnSelectCard => {(DHint)state.SelectionHint} / {PendingAction.Reason}");

            if (state.SelectionHint == (int)DHint.ATOHAND)
            {
                var preferredSearch = await GetSearchPriority(PendingAction.Reason);
                var searchSelection = PreferCardSelection(state, preferredSearch);
                if (searchSelection != null)
                {
                    ClearPendingAction();
                    return searchSelection;
                }
            }

            if (state.SelectionHint == (int)DHint.CONFIRM && PendingAction.Reason == PendingSelectionReason.RevealForMoYe)
            {
                var revealSelection = PreferCardSelection(state, MoYeRevealPriority);
                if (revealSelection != null)
                {
                    ClearPendingAction();
                    return revealSelection;
                }
            }

            if (state.SelectionHint == (int)DHint.DISCARD && PendingAction.Reason == PendingSelectionReason.DiscardForLongyuan)
            {
                var discardSelection = PreferCardSelection(state, LongyuanDiscardPriority);
                if (discardSelection != null)
                {
                    ClearPendingAction();
                    return discardSelection;
                }
            }

            if (state.SelectionHint == (int)DHint.TOGRAVE && PendingAction.Reason == PendingSelectionReason.SendToGraveForTaia)
            {
                var graveSelection = PreferCardSelection(state, TaiaSendPriority);
                if (graveSelection != null)
                {
                    ClearPendingAction();
                    return graveSelection;
                }
            }

            if ((state.SelectionHint == (int)DHint.TARGET || state.SelectionHint == (int)DHint.DESTROY) &&
                PendingAction.Reason is PendingSelectionReason.BaxiaTarget or PendingSelectionReason.BlackoutTarget)
            {
                var targets = await BuildTargetPriority();
                var targetSelection = PreferCardSelection(state, targets);
                if (targetSelection != null)
                {
                    ClearPendingAction();
                    return targetSelection;
                }
            }

            return await base.OnSelectCard(state);
        }

        public override async Task<IList<byte>?> OnSelectSum(SelectSumData state)
        {
            await PrepareContextAsync();

            if (state.SelectionHint == (int)DHint.SMATERIAL)
            {
                if (PendingAction.Reason == PendingSelectionReason.SummonLevel8)
                {
                    var level8Selection = TrySelectExactMaterials(
                        state,
                        SwordSoulCardId.SwordsoulToken,
                        SwordSoulCardId.SwordsoulOfMoYe,
                        SwordSoulCardId.SwordsoulOfTaia);

                    if (level8Selection != null)
                    {
                        ClearPendingAction();
                        return level8Selection;
                    }
                }

                if (PendingAction.Reason == PendingSelectionReason.SummonLevel10)
                {
                    var level10Selection = TrySelectExactMaterials(
                        state,
                        SwordSoulCardId.SwordsoulToken,
                        SwordSoulCardId.SwordsoulStrategistLongYuan);

                    if (level10Selection != null)
                    {
                        ClearPendingAction();
                        return level10Selection;
                    }
                }
            }

            return await base.OnSelectSum(state);
        }

        private static bool TryGetSummonable(IdleCmdData state, int cardId, out DynamicCard card)
        {
            return state.SummonableCards.TryGetByID(cardId, out card);
        }

        private static bool TryGetSpecialSummon(IdleCmdData state, int cardId, out DynamicCard card)
        {
            return state.SpecialSummonableCards.TryGetByID(cardId, out card);
        }

        private static bool TryGetSpellSettable(IdleCmdData state, int cardId, out DynamicCard card)
        {
            return state.SpellSetableCards.TryGetByID(cardId, out card);
        }

        private static bool TryGetActivable(IdleCmdData state, int cardId, out DynamicCard card)
        {
            return state.ActivableCards.TryGetByID(cardId, out card);
        }

        private async Task<bool> HasRevealForMoYe()
        {
            var hand = await _gameField.GetBotHandCards();
            return hand.Any(IsMoYeRevealCandidate);
        }

        private async Task<bool> HasTaiaBanishTarget()
        {
            var graveyard = await _gameField.GetBotGraveyardCards();
            return graveyard.Any(IsSwordSoulOrWyrm);
        }

        private async Task<bool> TryGetLongyuanDiscardCandidate()
        {
            var hand = await _gameField.GetBotHandCards();
            return hand.Any(IsLongyuanDiscardCandidate);
        }

        private async Task<bool> ShouldUseEmergence()
        {
            var hand = await _gameField.GetBotHandCards();
            bool hasMoYe = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe));
            bool hasTaia = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia));
            bool hasLongyuan = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan));
            bool hasBlackout = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulBlackout));
            bool hasStarter = hasMoYe && await HasRevealForMoYe() || hasTaia && await HasTaiaBanishTarget();
            bool hasChixiaoOnField = await _gameField.HasCardInBotMonsterZone(SwordSoulCardId.SwordsoulGrandmasterChixiao);

            if (!hasStarter)
            {
                return true;
            }

            if (hasChixiaoOnField && (!hasLongyuan || !hasBlackout))
            {
                return true;
            }

            return !hasBlackout;
        }

        private PendingSelectionReason GetEmergenceReason()
        {
            return PendingAction.Reason == PendingSelectionReason.SearchFollowUp
                ? PendingSelectionReason.SearchFollowUp
                : PendingSelectionReason.SearchStarter;
        }

        private async Task<bool> ShouldSetBlackout()
        {
            if (await _gameField.HasCardInBotSpellZone(SwordSoulCardId.SwordsoulBlackout))
            {
                return false;
            }

            var botMonsters = await _gameField.GetBotMonsterCards();
            return botMonsters.Any(card => card.IsFaceup() && IsSwordSoulOrWyrm(card));
        }

        private async Task<bool> HasWorthwhileEnemyTarget()
        {
            var bestEnemy = await GetBestEnemyCard();
            return bestEnemy.ID > 0;
        }

        private async Task<bool> ShouldPreferBaronne()
        {
            var bestEnemyMonster = await GetBestEnemyMonster(true);
            return bestEnemyMonster.ID > 0 && bestEnemyMonster.GetRealPower() >= 2500;
        }

        private async Task<int[]> GetSearchPriority(PendingSelectionReason reason)
        {
            var hand = await _gameField.GetBotHandCards();
            bool hasMoYe = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe));
            bool hasTaia = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia));
            bool hasLongyuan = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan));
            bool hasBlackout = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulBlackout));

            if (reason == PendingSelectionReason.SearchFollowUp)
            {
                return new[]
                {
                    hasLongyuan ? SwordSoulCardId.SwordsoulBlackout : SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulEmergence,
                    SwordSoulCardId.SwordsoulOfTaia
                };
            }

            if (!hasMoYe)
            {
                return new[]
                {
                    SwordSoulCardId.SwordsoulOfMoYe,
                    SwordSoulCardId.SwordsoulOfTaia,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence
                };
            }

            if (!hasTaia)
            {
                return new[]
                {
                    SwordSoulCardId.SwordsoulOfTaia,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence
                };
            }

            if (!hasLongyuan)
            {
                return new[]
                {
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence
                };
            }

            if (!hasBlackout)
            {
                return new[]
                {
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence,
                    SwordSoulCardId.SwordsoulSummit
                };
            }

            return new[]
            {
                SwordSoulCardId.SwordsoulStrategistLongYuan,
                SwordSoulCardId.SwordsoulBlackout,
                SwordSoulCardId.SwordsoulEmergence
            };
        }

        private async Task<int[]> BuildTargetPriority()
        {
            var priorities = new List<int>();

            var bestEnemyMonster = await GetBestEnemyMonster();
            if (bestEnemyMonster.ID > 0)
            {
                priorities.Add(bestEnemyMonster.ID);
            }

            var bestEnemySpell = await GetBestEnemySpell();
            if (bestEnemySpell.ID > 0 && !priorities.Contains(bestEnemySpell.ID))
            {
                priorities.Add(bestEnemySpell.ID);
            }

            var enemyMonsters = await _gameField.GetEnemyMonsterCards();
            priorities.AddRange(enemyMonsters
                .OrderByDescending(card => card.GetRealPower())
                .Select(card => card.ID)
                .Where(id => !priorities.Contains(id)));

            var enemySpells = await _gameField.GetEnemySpellCards();
            priorities.AddRange(enemySpells
                .OrderByDescending(card => card.IsFaceup())
                .Select(card => card.ID)
                .Where(id => !priorities.Contains(id)));

            priorities.Add(SwordSoulCardId.SwordsoulToken);
            priorities.Add(SwordSoulCardId.SwordsoulOfTaia);
            priorities.Add(SwordSoulCardId.SwordsoulOfMoYe);
            priorities.Add(SwordSoulCardId.SwordsoulGrandmasterChixiao);

            return priorities.ToArray();
        }

        private IList<byte>? TrySelectExactMaterials(SelectSumData state, params int[] preferredIds)
        {
            var selected = state.SelectableCards
                .Select((card, index) => new { Card = card, Index = index })
                .Where(x => x.Card.IsCode(preferredIds))
                .ToList();

            if (selected.Count >= state.Minimum)
            {
                var exact = selected
                    .Take(state.Minimum)
                    .Select(x => (byte)x.Index)
                    .ToArray();

                int levelSum = selected.Take(state.Minimum).Sum(x => x.Card.Level);
                if (!state.MustBeExactValue || levelSum == state.Sum)
                {
                    return exact;
                }
            }

            return null;
        }

        private static bool IsMoYeRevealCandidate(DynamicCard card)
        {
            return (IsSwordSoulOrWyrm(card) || card.IsCode(SwordSoulCardId.SwordsoulBlackout, SwordSoulCardId.SwordsoulEmergence, SwordSoulCardId.SwordsoulSummit))
                && !card.IsCode(SwordSoulCardId.SwordsoulOfMoYe);
        }

        private static bool IsLongyuanDiscardCandidate(DynamicCard card)
        {
            return (IsSwordSoulOrWyrm(card) || card.IsCode(SwordSoulCardId.SwordsoulBlackout, SwordSoulCardId.SwordsoulEmergence, SwordSoulCardId.SwordsoulSummit))
                && !card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan);
        }

        private static bool IsSwordSoulOrWyrm(DynamicCard card)
        {
            return card.HasRace(CardRace.Wyrm) ||
                   card.IsCode(
                       SwordSoulCardId.SwordsoulBlackout,
                       SwordSoulCardId.SwordsoulEmergence,
                       SwordSoulCardId.SwordsoulSummit);
        }

        private static CardPosition PickPosition(SelectPositionData state, CardPosition preferred)
        {
            if (state.Positions.Contains((byte)preferred))
            {
                return preferred;
            }

            return (CardPosition)state.Positions[0];
        }

        private static int GetExpectedAttack(int cardId)
        {
            return cardId switch
            {
                SwordSoulCardId.BaronneDeFluer => 3000,
                SwordSoulCardId.SwordsoulChengying => 3000,
                SwordSoulCardId.SwordsoulGrandmasterChixiao => 2800,
                SwordSoulCardId.YangZingBaxia => 2300,
                SwordSoulCardId.SwordsoulOfMoYe => 1700,
                _ => 0
            };
        }
    }
}
