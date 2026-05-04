using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.SwordSoul
{
    public sealed class SwordSoulBotBrain : DeckBotBrain
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
        [
            SwordSoulCardId.SwordsoulStrategistLongYuan,
            SwordSoulCardId.SwordsoulOfTaia,
            SwordSoulCardId.TenyiAshuna,
            SwordSoulCardId.TenyiVishuda,
            SwordSoulCardId.TenyiAdhara,
            SwordSoulCardId.SwordsoulBlackout,
            SwordSoulCardId.SwordsoulEmergence,
            SwordSoulCardId.SwordsoulSummit
        ];

        private static readonly int[] LongyuanDiscardPriority =
        [
            SwordSoulCardId.SwordsoulBlackout,
            SwordSoulCardId.SwordsoulEmergence,
            SwordSoulCardId.SwordsoulSummit,
            SwordSoulCardId.TenyiAdhara,
            SwordSoulCardId.TenyiVishuda,
            SwordSoulCardId.TenyiAshuna,
            SwordSoulCardId.SwordsoulOfTaia,
            SwordSoulCardId.SwordsoulOfMoYe
        ];

        private static readonly int[] TaiaSendPriority =
        [
            SwordSoulCardId.SwordsoulOfMoYe,
            SwordSoulCardId.TenyiAdhara,
            SwordSoulCardId.TenyiVishuda,
            SwordSoulCardId.TenyiAshuna
        ];

        public SwordSoulBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }

        public override async Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            bool hasChixiaoOnField = await HasCardInBotMonsterZoneAsync(SwordSoulCardId.SwordsoulGrandmasterChixiao);
            bool hasMoYeCombo = await HasMoYeStarter();
            bool hasTaiaCombo = await HasTaiaStarter();
            bool hasLongyuanLine = await HasLongyuanLine();

            if (TryGetActivable(context.StateData, SwordSoulCardId.SwordsoulOfMoYe, out var moYe) && hasMoYeCombo)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Mo Ye.");
                SetPendingAction(PendingActionKind.Reveal, moYe.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.RevealForMoYe);
                MarkCardActivated(moYe.ID);
                return moYe.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(context.StateData, SwordSoulCardId.SwordsoulGrandmasterChixiao, out var chixiao) && !hasChixiaoOnField)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Chixiao.");
                SetPendingAction(PendingActionKind.MaterialSelection, chixiao.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel8);
                return chixiao.SpecialSummon();
            }

            if (TryGetActivable(context.StateData, SwordSoulCardId.SwordsoulGrandmasterChixiao, out var chixiaoEffect) &&
                !WasCardActivatedThisTurn(chixiaoEffect.ID))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Chixiao.");
                SetPendingAction(PendingActionKind.Search, chixiaoEffect.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.SearchFollowUp);
                MarkCardActivated(chixiaoEffect.ID);
                return chixiaoEffect.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, SwordSoulCardId.SwordsoulStrategistLongYuan, out var longyuan) &&
                hasLongyuanLine)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Longyuan.");
                SetPendingAction(PendingActionKind.Discard, longyuan.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.DiscardForLongyuan);
                MarkCardActivated(longyuan.ID);
                return longyuan.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, SwordSoulCardId.SwordsoulSummit, out var summit) &&
                await ShouldUseSummit())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Summit.");
                SetPendingAction(PendingActionKind.Revive, summit.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.ReviveWithSummit);
                MarkCardActivated(summit.ID);
                return summit.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(context.StateData, SwordSoulCardId.BaronneDeFluer, out var baronne) && await ShouldPreferBaronne())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Baronne.");
                SetPendingAction(PendingActionKind.MaterialSelection, baronne.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel10);
                return baronne.SpecialSummon();
            }

            if (TryGetSpecialSummon(context.StateData, SwordSoulCardId.SwordsoulChengying, out var chengying))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Chengying.");
                SetPendingAction(PendingActionKind.MaterialSelection, chengying.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel10);
                return chengying.SpecialSummon();
            }

            if (TryGetSummonable(context.StateData, SwordSoulCardId.SwordsoulOfMoYe, out var moYeSummon) && hasMoYeCombo)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Mo Ye.");
                return moYeSummon.NormalSummon();
            }

            if (TryGetSummonable(context.StateData, SwordSoulCardId.SwordsoulOfTaia, out var taiaSummon) && hasTaiaCombo)
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Taia.");
                return taiaSummon.NormalSummon();
            }

            if (TryGetActivable(context.StateData, SwordSoulCardId.SwordsoulOfTaia, out var taiaActivate) &&
                hasTaiaCombo &&
                !WasCardActivatedThisTurn(taiaActivate.ID))
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Taia.");
                SetPendingAction(PendingActionKind.SendToGrave, taiaActivate.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.SendToGraveForTaia);
                MarkCardActivated(taiaActivate.ID);
                return taiaActivate.MainActivateFromIndex();
            }

            if (TryGetSpecialSummon(context.StateData, SwordSoulCardId.YangZingBaxia, out var baxia) && await HasWorthwhileEnemyTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Summoning Baxia.");
                SetPendingAction(PendingActionKind.MaterialSelection, baxia.ID, MainAction.SpSummon, contextCode: (int)SwordSoulContext.SummonLevel8);
                return baxia.SpecialSummon();
            }

            if (TryGetActivable(context.StateData, SwordSoulCardId.YangZingBaxia, out var baxiaEffect) &&
                !WasCardActivatedThisTurn(baxiaEffect.ID) &&
                await HasWorthwhileEnemyTarget())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Baxia.");
                SetPendingAction(PendingActionKind.TargetSelection, baxiaEffect.ID, MainAction.Activate, contextCode: (int)SwordSoulContext.BaxiaTarget);
                MarkCardActivated(baxiaEffect.ID);
                return baxiaEffect.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, SwordSoulCardId.SwordsoulEmergence, out var emergence) &&
                await ShouldUseEmergence())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Activating Emergence.");
                SetPendingAction(PendingActionKind.Search, emergence.ID, MainAction.Activate, contextCode: (int)GetEmergenceContext());
                MarkCardActivated(emergence.ID);
                return emergence.MainActivateFromIndex();
            }

            if (TryGetSpellSettable(context.StateData, SwordSoulCardId.SwordsoulBlackout, out var blackoutSet) && await ShouldSetBlackout())
            {
                ConsoleUtils.PrintColored("[SWORDSOUL] Setting Blackout.");
                return blackoutSet.SetSpellTrap();
            }

            return await base.OnIdleCmdAsync(context, ct);
        }

        public override async Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);
            RegisterChainDescriptions(context.StateData.Descriptions ?? Array.Empty<int>());

            if (context.StateData.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulGrandmasterChixiao, out _, out var index))
            {
                SetPendingAction(PendingActionKind.Search, SwordSoulCardId.SwordsoulGrandmasterChixiao, MainAction.Activate, contextCode: (int)SwordSoulContext.SearchFollowUp);
                MarkCardActivated(SwordSoulCardId.SwordsoulGrandmasterChixiao);
                return index;
            }

            if (context.StateData.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulBlackout, out _, out index) && await HasWorthwhileEnemyTarget())
            {
                SetPendingAction(PendingActionKind.TargetSelection, SwordSoulCardId.SwordsoulBlackout, MainAction.Activate, contextCode: (int)SwordSoulContext.BlackoutTarget);
                MarkCardActivated(SwordSoulCardId.SwordsoulBlackout);
                return index;
            }

            if (context.StateData.SelectableCards.TryGetByID(SwordSoulCardId.YangZingBaxia, out _, out index) && await HasWorthwhileEnemyTarget())
            {
                SetPendingAction(PendingActionKind.TargetSelection, SwordSoulCardId.YangZingBaxia, MainAction.Activate, contextCode: (int)SwordSoulContext.BaxiaTarget);
                MarkCardActivated(SwordSoulCardId.YangZingBaxia);
                return index;
            }

            if (context.StateData.SelectableCards.TryGetByID(SwordSoulCardId.SwordsoulOfTaia, out _, out index))
            {
                SetPendingAction(PendingActionKind.SendToGrave, SwordSoulCardId.SwordsoulOfTaia, MainAction.Activate, contextCode: (int)SwordSoulContext.SendToGraveForTaia);
                MarkCardActivated(SwordSoulCardId.SwordsoulOfTaia);
                return index;
            }

            return await base.OnSelectChainAsync(context, ct);
        }

        public override async Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (context.StateData.EffectCard.IsCode(SwordSoulCardId.SwordsoulOfMoYe))
                return await HasRevealForMoYe();

            if (context.StateData.EffectCard.IsCode(SwordSoulCardId.SwordsoulOfTaia))
                return await HasTaiaBanishTarget();

            if (context.StateData.EffectCard.IsCode(
                SwordSoulCardId.SwordsoulGrandmasterChixiao,
                SwordSoulCardId.SwordsoulStrategistLongYuan,
                SwordSoulCardId.YangZingBaxia,
                SwordSoulCardId.SwordsoulBlackout))
            {
                return true;
            }

            return await base.OnSelectEffectYnAsync(context, ct);
        }

        public override async Task<CardPosition> OnSelectPositionAsync(DecisionContext<SelectPositionData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (context.StateData.CardID == SwordSoulCardId.SwordsoulToken ||
                context.StateData.CardID == SwordSoulCardId.YangZingBaxia ||
                context.StateData.CardID == SwordSoulCardId.SwordsoulOfMoYe)
            {
                return PickPosition(context.StateData, CardPosition.FaceUpDefence);
            }

            if (context.StateData.CardID == SwordSoulCardId.BaronneDeFluer ||
                context.StateData.CardID == SwordSoulCardId.SwordsoulChengying ||
                context.StateData.CardID == SwordSoulCardId.SwordsoulGrandmasterChixiao)
            {
                if (IsFirstTurnOrMain2(context.Header))
                    return PickPosition(context.StateData, CardPosition.FaceUpDefence);

                var bestEnemy = await GetBestEnemyMonster(true);
                if (bestEnemy.ID > 0 && bestEnemy.Attack > GetExpectedAttack(context.StateData.CardID))
                    return PickPosition(context.StateData, CardPosition.FaceUpDefence);

                return PickPosition(context.StateData, CardPosition.FaceUpAttack);
            }

            return await base.OnSelectPositionAsync(context, ct);
        }

        public override async Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);
            ConsoleUtils.PrintColored($"[SWORDSOUL] OnSelectCard => {(DHint)context.StateData.SelectionHint} / {(SwordSoulContext)PendingAction.ContextCode}");

            if (context.StateData.SelectionHint == (int)DHint.ATOHAND)
            {
                var preferredSearch = await GetSearchPriority((SwordSoulContext)PendingAction.ContextCode);
                var searchSelection = PreferCardSelection(context.StateData, preferredSearch);
                if (searchSelection != null)
                {
                    ClearPendingAction();
                    return searchSelection;
                }
            }

            if ((context.StateData.SelectionHint == (int)DHint.SPSUMMON || context.StateData.SelectionHint == (int)DHint.TOFIELD) &&
                PendingContextIs((int)SwordSoulContext.ReviveWithSummit))
            {
                var reviveSelection = await GetSummitReviveSelection(context.StateData);
                if (reviveSelection != null)
                {
                    ClearPendingAction();
                    return reviveSelection;
                }
            }

            if (context.StateData.SelectionHint == (int)DHint.CONFIRM && PendingContextIs((int)SwordSoulContext.RevealForMoYe))
            {
                var revealSelection = PreferCardSelection(context.StateData, MoYeRevealPriority);
                if (revealSelection != null)
                {
                    ClearPendingAction();
                    return revealSelection;
                }
            }

            if (context.StateData.SelectionHint == (int)DHint.DISCARD && PendingContextIs((int)SwordSoulContext.DiscardForLongyuan))
            {
                var discardSelection = PreferCardSelection(context.StateData, LongyuanDiscardPriority);
                if (discardSelection != null)
                {
                    ClearPendingAction();
                    return discardSelection;
                }
            }

            if (context.StateData.SelectionHint == (int)DHint.TOGRAVE && PendingContextIs((int)SwordSoulContext.SendToGraveForTaia))
            {
                var graveSelection = PreferCardSelection(context.StateData, TaiaSendPriority);
                if (graveSelection != null)
                {
                    ClearPendingAction();
                    return graveSelection;
                }
            }

            if ((context.StateData.SelectionHint == (int)DHint.TARGET || context.StateData.SelectionHint == (int)DHint.DESTROY) &&
                PendingAction.Kind == PendingActionKind.TargetSelection)
            {
                IList<byte>? targetSelection = PendingContextIs((int)SwordSoulContext.BlackoutTarget)
                    ? await SelectBlackoutTargets(context.StateData)
                    : PreferCardSelection(context.StateData, await BuildEnemyTargetPriority());

                if (targetSelection != null)
                {
                    ClearPendingAction();
                    return targetSelection;
                }
            }

            return await base.OnSelectCardAsync(context, ct);
        }

        public override async Task<IList<byte>?> OnSelectSumAsync(DecisionContext<SelectSumData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (context.StateData.SelectionHint == (int)DHint.SMATERIAL)
            {
                if (PendingContextIs((int)SwordSoulContext.SummonLevel8))
                {
                    var level8Selection = TrySelectExactMaterials(
                        context.StateData,
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
                        context.StateData,
                        SwordSoulCardId.SwordsoulToken,
                        SwordSoulCardId.SwordsoulStrategistLongYuan);

                    if (level10Selection != null)
                    {
                        ClearPendingAction();
                        return level10Selection;
                    }
                }
            }

            return await base.OnSelectSumAsync(context, ct);
        }

        private async Task<bool> HasRevealForMoYe()
        {
            var hand = await GetBotHandCardsAsync();
            return hand.Any(IsMoYeRevealCandidate);
        }

        private async Task<bool> HasMoYeStarter() => await HasRevealForMoYe();

        private async Task<bool> HasTaiaBanishTarget()
        {
            var graveyard = await GetBotGraveyardCardsAsync();
            return graveyard.Any(IsSwordSoulOrWyrm);
        }

        private async Task<bool> HasTaiaStarter() => await HasTaiaBanishTarget();

        private async Task<bool> HasLongyuanLine()
        {
            var hand = await GetBotHandCardsAsync();
            return hand.Any(IsLongyuanDiscardCandidate);
        }

        private async Task<bool> ShouldUseEmergence()
        {
            var hand = await GetBotHandCardsAsync();
            bool hasMoYe = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe));
            bool hasTaia = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia));
            bool hasLongyuan = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulStrategistLongYuan));
            bool hasBlackout = hand.Any(card => card.IsCode(SwordSoulCardId.SwordsoulBlackout));
            bool hasMoYeStarter = hasMoYe && await HasRevealForMoYe();
            bool hasTaiaStarter = hasTaia && await HasTaiaBanishTarget();
            bool hasStarter = hasMoYeStarter || hasTaiaStarter;
            bool hasChixiaoOnField = await HasCardInBotMonsterZoneAsync(SwordSoulCardId.SwordsoulGrandmasterChixiao);

            if (!hasStarter)
                return true;

            if (hasChixiaoOnField && (!hasLongyuan || !hasBlackout))
                return true;

            if (hasMoYeStarter && !hasLongyuan)
                return true;

            if (!hasTaia && await HasTaiaBanishTarget())
                return true;

            return !hasBlackout;
        }

        private async Task<bool> ShouldUseSummit()
        {
            var graveyard = await GetBotGraveyardCardsAsync();
            var monsters = await GetBotMonsterCardsAsync();

            bool hasMoYeInGrave = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe));
            bool hasTaiaInGrave = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia));
            bool hasFaceUpSynchro = monsters.Any(card => card.IsFaceup() && card.HasType(CardType.Synchro));

            if (hasMoYeInGrave && await HasRevealForMoYe())
                return true;

            if (hasTaiaInGrave && await HasTaiaBanishTarget())
                return true;

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
            if (await HasCardInBotSpellZoneAsync(SwordSoulCardId.SwordsoulBlackout))
                return false;

            var botMonsters = await GetBotMonsterCardsAsync();
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
            var hand = await GetBotHandCardsAsync();
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
                    return
                    [
                        SwordSoulCardId.SwordsoulStrategistLongYuan,
                        SwordSoulCardId.SwordsoulBlackout,
                        SwordSoulCardId.SwordsoulSummit,
                        SwordSoulCardId.SwordsoulEmergence,
                        SwordSoulCardId.SwordsoulOfTaia
                    ];
                }

                if (!hasBlackout)
                {
                    return
                    [
                        SwordSoulCardId.SwordsoulBlackout,
                        SwordSoulCardId.SwordsoulSummit,
                        SwordSoulCardId.SwordsoulStrategistLongYuan,
                        SwordSoulCardId.SwordsoulEmergence,
                        SwordSoulCardId.SwordsoulOfTaia
                    ];
                }

                return
                [
                    SwordSoulCardId.SwordsoulSummit,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulEmergence,
                    SwordSoulCardId.SwordsoulOfTaia
                ];
            }

            if (!moYeStarterReady)
            {
                return
                [
                    SwordSoulCardId.SwordsoulOfMoYe,
                    SwordSoulCardId.SwordsoulOfTaia,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence
                ];
            }

            if (!taiaStarterReady)
            {
                return
                [
                    SwordSoulCardId.SwordsoulOfTaia,
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence
                ];
            }

            if (!hasLongyuan)
            {
                return
                [
                    SwordSoulCardId.SwordsoulStrategistLongYuan,
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence
                ];
            }

            if (!hasBlackout)
            {
                return
                [
                    SwordSoulCardId.SwordsoulBlackout,
                    SwordSoulCardId.SwordsoulEmergence,
                    SwordSoulCardId.SwordsoulSummit
                ];
            }

            return
            [
                SwordSoulCardId.SwordsoulStrategistLongYuan,
                SwordSoulCardId.SwordsoulBlackout,
                SwordSoulCardId.SwordsoulEmergence
            ];
        }

        private async Task<int[]> BuildEnemyTargetPriority()
        {
            var priorities = new List<int>();

            var bestEnemyMonster = await GetBestEnemyMonster();
            if (bestEnemyMonster.ID > 0)
                priorities.Add(bestEnemyMonster.ID);

            var bestEnemySpell = await GetBestEnemySpell();
            if (bestEnemySpell.ID > 0 && !priorities.Contains(bestEnemySpell.ID))
                priorities.Add(bestEnemySpell.ID);

            var enemyMonsters = await GetEnemyMonsterCardsAsync();
            priorities.AddRange(enemyMonsters
                .OrderByDescending(card => card.GetRealPower())
                .Select(card => card.ID)
                .Where(id => !priorities.Contains(id)));

            var enemySpells = await GetEnemySpellCardsAsync();
            priorities.AddRange(enemySpells
                .OrderByDescending(card => card.IsFaceup())
                .Select(card => card.ID)
                .Where(id => !priorities.Contains(id)));

            return priorities.ToArray();
        }

        private async Task<IList<byte>?> GetSummitReviveSelection(SelectCardData state)
        {
            var graveyard = await GetBotGraveyardCardsAsync();
            bool canUseMoYe = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfMoYe)) && await HasRevealForMoYe();
            bool canUseTaia = graveyard.Any(card => card.IsCode(SwordSoulCardId.SwordsoulOfTaia)) && await HasTaiaBanishTarget();

            if (canUseMoYe)
            {
                var selection = PreferCardSelection(state, SwordSoulCardId.SwordsoulOfMoYe);
                if (selection != null)
                    return selection;
            }

            if (canUseTaia)
            {
                var selection = PreferCardSelection(state, SwordSoulCardId.SwordsoulOfTaia);
                if (selection != null)
                    return selection;
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
                return null;

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
                return selection.Take(state.Minimum).ToArray();

            return null;
        }

        private static IList<byte>? TrySelectExactMaterials(SelectSumData state, params int[] preferredIds)
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
                        continue;

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
            var positions = state.Positions ?? Array.Empty<byte>();
            if (positions.Contains((byte)preferred))
                return preferred;

            return positions.Length > 0 ? (CardPosition)positions[0] : CardPosition.FaceUpAttack;
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
