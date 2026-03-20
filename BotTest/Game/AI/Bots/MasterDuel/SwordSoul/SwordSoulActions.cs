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
        private enum SwordSoulContext
        {
            None = 0,
            SearchStarter,
            SearchFollowUp,
            RevealForMoYe,
            DiscardForLongyuan,
            SendToGraveForTaia,
            ReviveWithSummit,
            SummonLevel8,
            SummonLevel10,
            BaxiaTarget,
            BlackoutTarget
        }

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

            bool hasChixiaoOnField = await _gameField.HasCardInBotMonsterZone(SwordSoulCardId.SwordsoulGrandmasterChixiao);
            bool hasMoYeCombo = await HasMoYeStarter();
            bool hasTaiaCombo = await HasTaiaStarter();
            bool hasLongyuanLine = await HasLongyuanLine();

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulOfMoYe, out var moYe) && hasMoYeCombo)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Mo Ye.");
                SetPendingAction(PendingActionKind.Reveal, moYe.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.RevealForMoYe);
                MarkCardActivated(moYe.ID);
                return moYe.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.SwordsoulGrandmasterChixiao, out var chixiao) && !hasChixiaoOnField)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Chixiao.");
                SetPendingAction(PendingActionKind.MaterialSelection, chixiao.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel8);
                return chixiao.SpecialSummon();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulGrandmasterChixiao, out var chixiaoEffect) &&
                !WasCardActivatedThisTurn(chixiaoEffect.ID))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Chixiao.");
                SetPendingAction(PendingActionKind.Search, chixiaoEffect.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.SearchFollowUp);
                MarkCardActivated(chixiaoEffect.ID);
                return chixiaoEffect.MainActivateFromIndex();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulStrategistLongYuan, out var longyuan) &&
                hasLongyuanLine)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Longyuan.");
                SetPendingAction(PendingActionKind.Discard, longyuan.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.DiscardForLongyuan);
                MarkCardActivated(longyuan.ID);
                return longyuan.MainActivateFromIndex();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulSummit, out var summit) &&
                await ShouldUseSummit())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Summit.");
                SetPendingAction(PendingActionKind.Revive, summit.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.ReviveWithSummit);
                MarkCardActivated(summit.ID);
                return summit.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.BaronneDeFluer, out var baronne) && await ShouldPreferBaronne())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Baronne.");
                SetPendingAction(PendingActionKind.MaterialSelection, baronne.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel10);
                return baronne.SpecialSummon();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.SwordsoulChengying, out var chengying))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Chengying.");
                SetPendingAction(PendingActionKind.MaterialSelection, chengying.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel10);
                return chengying.SpecialSummon();
            }

            if (TryGetSummonable(state, SwordSoulCardId.SwordsoulOfMoYe, out var moYeSummon) && hasMoYeCombo)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Mo Ye.");
                return moYeSummon.NormalSummon();
            }

            if (TryGetSummonable(state, SwordSoulCardId.SwordsoulOfTaia, out var taiaSummon) && hasTaiaCombo)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Taia.");
                return taiaSummon.NormalSummon();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulOfTaia, out var taiaActivate) &&
                hasTaiaCombo &&
                !WasCardActivatedThisTurn(taiaActivate.ID))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Taia.");
                SetPendingAction(PendingActionKind.SendToGrave, taiaActivate.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.SendToGraveForTaia);
                MarkCardActivated(taiaActivate.ID);
                return taiaActivate.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(state, SwordSoulCardId.YangZingBaxia, out var baxia) && await HasWorthwhileEnemyTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Baxia.");
                SetPendingAction(PendingActionKind.MaterialSelection, baxia.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel8);
                return baxia.SpecialSummon();
            }

            if (TryGetActivable(state, SwordSoulCardId.YangZingBaxia, out var baxiaEffect) &&
                !WasCardActivatedThisTurn(baxiaEffect.ID) &&
                await HasWorthwhileEnemyTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Baxia.");
                SetPendingAction(PendingActionKind.TargetSelection, baxiaEffect.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.BaxiaTarget);
                MarkCardActivated(baxiaEffect.ID);
                return baxiaEffect.MainActivateFromIndex();
            }

            if (TryGetActivable(state, SwordSoulCardId.SwordsoulEmergence, out var emergence) &&
                await ShouldUseEmergence())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Emergence.");
                SetPendingAction(PendingActionKind.Search, emergence.ID, MainAction.Activate, contextCode: (int)GetEmergenceContext());
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
                SetPendingAction(PendingActionKind.Search, SwordSoulCardId.SwordsoulGrandmasterChixiao, MainAction.Activate, contextCode: (int)SwordSoulContext.SearchFollowUp);
                MarkCardActivated(SwordSoulCardId.SwordsoulGrandmasterChixiao);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulBlackout, out _, out index) && await HasWorthwhileEnemyTarget())
            {
                SetPendingAction(PendingActionKind.TargetSelection, SwordSoulCardId.SwordsoulBlackout, MainAction.Activate, contextCode: (int)SwordSoulContext.BlackoutTarget);
                MarkCardActivated(SwordSoulCardId.SwordsoulBlackout);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.YangZingBaxia, out _, out index) && await HasWorthwhileEnemyTarget())
            {
                SetPendingAction(PendingActionKind.TargetSelection, SwordSoulCardId.YangZingBaxia, MainAction.Activate, contextCode: (int)SwordSoulContext.BaxiaTarget);
                MarkCardActivated(SwordSoulCardId.YangZingBaxia);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulOfTaia, out _, out index))
            {
                SetPendingAction(PendingActionKind.SendToGrave, SwordSoulCardId.SwordsoulOfTaia, MainAction.Activate, contextCode: (int)SwordSoulContext.SendToGraveForTaia);
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
            ConsoleUtils.PrintColored($"[SWORDSOUL] OnSelectCard => {(DHint)state.SelectionHint} / {(SwordSoulContext)PendingAction.ContextCode}");

            if (state.SelectionHint == (int)DHint.ATOHAND)
            {
                var preferredSearch = await GetSearchPriority((SwordSoulContext)PendingAction.ContextCode);
                var searchSelection = PreferCardSelection(state, preferredSearch);
                if (searchSelection != null)
                {
                    ClearPendingAction();
                    return searchSelection;
                }
            }

            if ((state.SelectionHint == (int)DHint.SPSUMMON || state.SelectionHint == (int)DHint.TOFIELD) &&
                PendingContextIs((int)SwordSoulContext.ReviveWithSummit))
            {
                var reviveSelection = await GetSummitReviveSelection(state);
                if (reviveSelection != null)
                {
                    ClearPendingAction();
                    return reviveSelection;
                }
            }

            if (state.SelectionHint == (int)DHint.CONFIRM && PendingContextIs((int)SwordSoulContext.RevealForMoYe))
            {
                var revealSelection = PreferCardSelection(state, MoYeRevealPriority);
                if (revealSelection != null)
                {
                    ClearPendingAction();
                    return revealSelection;
                }
            }

            if (state.SelectionHint == (int)DHint.DISCARD && PendingContextIs((int)SwordSoulContext.DiscardForLongyuan))
            {
                var discardSelection = PreferCardSelection(state, LongyuanDiscardPriority);
                if (discardSelection != null)
                {
                    ClearPendingAction();
                    return discardSelection;
                }
            }

            if (state.SelectionHint == (int)DHint.TOGRAVE && PendingContextIs((int)SwordSoulContext.SendToGraveForTaia))
            {
                var graveSelection = PreferCardSelection(state, TaiaSendPriority);
                if (graveSelection != null)
                {
                    ClearPendingAction();
                    return graveSelection;
                }
            }

            if ((state.SelectionHint == (int)DHint.TARGET || state.SelectionHint == (int)DHint.DESTROY) &&
                PendingAction.Kind == PendingActionKind.TargetSelection)
            {
                IList<byte>? targetSelection = PendingContextIs((int)SwordSoulContext.BlackoutTarget)
                    ? await SelectBlackoutTargets(state)
                    : PreferCardSelection(state, await BuildEnemyTargetPriority());

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
                if (PendingContextIs((int)SwordSoulContext.SummonLevel8))
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

                if (PendingContextIs((int)SwordSoulContext.SummonLevel10))
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

        private async Task<bool> HasMoYeStarter()
        {
            return await HasRevealForMoYe();
        }

        private async Task<bool> HasTaiaBanishTarget()
        {
            var graveyard = await _gameField.GetBotGraveyardCards();
            return graveyard.Any(IsSwordSoulOrWyrm);
        }

        private async Task<bool> HasTaiaStarter()
        {
            return await HasTaiaBanishTarget();
        }

        private async Task<bool> HasLongyuanLine()
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
            bool hasMoYeStarter = hasMoYe && await HasRevealForMoYe();
            bool hasTaiaStarter = hasTaia && await HasTaiaBanishTarget();
            bool hasStarter = hasMoYeStarter || hasTaiaStarter;
            bool hasChixiaoOnField = await _gameField.HasCardInBotMonsterZone(SwordSoulCardId.SwordsoulGrandmasterChixiao);

            if (!hasStarter)
            {
                return true;
            }

            if (hasChixiaoOnField && (!hasLongyuan || !hasBlackout))
            {
                return true;
            }

            if (hasMoYeStarter && !hasLongyuan)
            {
                return true;
            }

            if (!hasTaia && await HasTaiaBanishTarget())
            {
                return true;
            }

            return !hasBlackout;
        }

        private async Task<bool> ShouldUseSummit()
        {
            var graveyard = await _gameField.GetBotGraveyardCards();
            var monsters = await _gameField.GetBotMonsterCards();

            bool hasMoYeInGrave = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe));
            bool hasTaiaInGrave = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia));
            bool hasFaceUpSynchro = monsters.Any(card => card.IsFaceup() && card.HasType(CardType.Synchro));

            if (hasMoYeInGrave && await HasRevealForMoYe())
            {
                return true;
            }

            if (hasTaiaInGrave && await HasTaiaBanishTarget())
            {
                return true;
            }

            return hasFaceUpSynchro && (hasMoYeInGrave || hasTaiaInGrave);
        }

        private SwordSoulContext GetEmergenceContext()
        {
            return PendingContextIs((int)SwordSoulContext.SearchFollowUp)
                ? SwordSoulContext.SearchFollowUp
                : SwordSoulContext.SearchStarter;
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

        private async Task<int[]> GetSearchPriority(SwordSoulContext searchContext)
        {
            var hand = await _gameField.GetBotHandCards();
            bool hasMoYe = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe));
            bool hasTaia = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia));
            bool hasLongyuan = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan));
            bool hasBlackout = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulBlackout));
            bool moYeStarterReady = hasMoYe && await HasRevealForMoYe();
            bool taiaStarterReady = hasTaia && await HasTaiaBanishTarget();

            if (searchContext == SwordSoulContext.SearchFollowUp)
            {
                if (!hasLongyuan)
                {
                    return new[]
                    {
                        SwordSoulCardId.SwordsoulStrategistLongYuan,
                        SwordSoulCardId.SwordsoulBlackout,
                        SwordSoulCardId.SwordsoulSummit,
                        SwordSoulCardId.SwordsoulEmergence,
                        SwordSoulCardId.SwordsoulOfTaia
                    };
                }

                if (!hasBlackout)
                {
                    return new[]
                    {
                        SwordSoulCardId.SwordsoulBlackout,
                        SwordSoulCardId.SwordsoulSummit,
                        SwordSoulCardId.SwordsoulStrategistLongYuan,
                        SwordSoulCardId.SwordsoulEmergence,
                        SwordSoulCardId.SwordsoulOfTaia
                    };
                }

                return new[]
                {
                    SwordSoulCardId.SwordsoulSummit,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulEmergence,
                    SwordSoulCardId.SwordsoulOfTaia
                };
            }

            if (!moYeStarterReady)
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

            if (!taiaStarterReady)
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

        private async Task<int[]> BuildEnemyTargetPriority()
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

            return priorities.ToArray();
        }

        private async Task<IList<byte>?> GetSummitReviveSelection(SelectCardData state)
        {
            var graveyard = await _gameField.GetBotGraveyardCards();
            bool canUseMoYe = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe)) && await HasRevealForMoYe();
            bool canUseTaia = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia)) && await HasTaiaBanishTarget();

            if (canUseMoYe)
            {
                var selection = PreferCardSelection(state, SwordSoulCardId.SwordsoulOfMoYe);
                if (selection != null)
                {
                    return selection;
                }
            }

            if (canUseTaia)
            {
                var selection = PreferCardSelection(state, SwordSoulCardId.SwordsoulOfTaia);
                if (selection != null)
                {
                    return selection;
                }
            }

            return PreferCardSelection(state, SwordSoulCardId.SwordsoulGrandmasterChixiao, SwordSoulCardId.YangZingBaxia);
        }

        private async Task<IList<byte>?> SelectBlackoutTargets(SelectCardData state)
        {
            var ownTarget = state.SelectableCards
                .Select((card, index) => new { Card = card, Index = index })
                .Where(x =>
                    x.Card.CurrentCoordinates.Location == (byte)CardLocation.MonsterZone &&
                    x.Card.IsFaceup() &&
                    IsSwordSoulOrWyrm(x.Card))
                .OrderBy(x => x.Card.HasType(CardType.Synchro))
                .ThenBy(x => x.Card.Attack)
                .FirstOrDefault();

            if (ownTarget == null)
            {
                return null;
            }

            var enemyTargetIds = await BuildEnemyTargetPriority();
            var enemyTargets = state.SelectableCards
                .Select((card, index) => new { Card = card, Index = index })
                .Where(x => x.Index != ownTarget.Index && x.Card.CurrentCoordinates.Location != (byte)CardLocation.Hand)
                .Where(x => enemyTargetIds.Contains(x.Card.ID))
                .OrderBy(x => Array.IndexOf(enemyTargetIds, x.Card.ID))
                .Take(Math.Max(0, state.Minimum - 1))
                .Select(x => (byte)x.Index)
                .ToList();

            var selection = new List<byte> { (byte)ownTarget.Index };
            selection.AddRange(enemyTargets);

            if (selection.Count >= state.Minimum)
            {
                return selection.Take(state.Minimum).ToArray();
            }

            return null;
        }

        private IList<byte>? TrySelectExactMaterials(SelectSumData state, params int[] preferredIds)
        {
            var selectable = state.SelectableCards
                .Select((card, index) => new { Card = card, Index = index })
                .Where(x => x.Card.IsCode(preferredIds) || state.MandatoryCards.Any(mandatory => mandatory.ID == x.Card.ID))
                .ToList();

            for (int count = state.Minimum; count <= Math.Min(state.Maximum, selectable.Count); count++)
            {
                foreach (var combo in selectable.GetCombinations(count))
                {
                    var materialList = combo.ToList();
                    bool includesMandatory = state.MandatoryCards.All(mandatory =>
                        materialList.Any(item => item.Card.ID == mandatory.ID));
                    if (!includesMandatory)
                    {
                        continue;
                    }

                    int sum1 = materialList.Sum(item => item.Card.OpParam1);
                    int sum2 = materialList.Sum(item => item.Card.OpParam2 != 0 ? item.Card.OpParam2 : item.Card.OpParam1);
                    if (sum1 == state.Sum || sum2 == state.Sum || (!state.MustBeExactValue && (sum1 >= state.Sum || sum2 >= state.Sum)))
                    {
                        return materialList.Select(item => (byte)item.Index).ToArray();
                    }
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
                && !card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan)
                && !card.IsCode(SwordSoulCardId.SwordsoulBlackout);
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
