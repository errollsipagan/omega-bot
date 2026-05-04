using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.SpeedDuel.SuperDuck
{
    public sealed class SuperDuckBotBrain : DeckBotBrain
    {
        private readonly Dictionary<int, bool> _selectChainMap = new();

        public SuperDuckBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }

        public override async Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (_selectChainMap.Any())
                _selectChainMap.Clear();

            foreach (var card in context.StateData.ActivableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[Activable cards] => {card.ID}");
            foreach (var card in context.StateData.SummonableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[Summonable cards] => {card.ID}");
            foreach (var card in context.StateData.ReposableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[Reposable cards] => {card.ID}");
            foreach (var card in context.StateData.SpecialSummonableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[SpecialSummonable cards] => {card.ID}");

            if (TryGetActivable(context.StateData, SuperDuckCardId.HammerShot, out var hammerShot) &&
                await HammerShotCondition(hammerShot))
            {
                return hammerShot.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, SuperDuckCardId.EnchantingFittingRoom, out var enchantingFittingRoom) &&
                await EnchantingFittingRoomCondition(enchantingFittingRoom))
            {
                return enchantingFittingRoom.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, SuperDuckCardId.AllureOfDarkness, out var allureOfDarkness) &&
                await AllureOfDarknessCondition(allureOfDarkness))
            {
                return allureOfDarkness.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, SuperDuckCardId.RoomForGrowth, out var roomForGrowth))
                return roomForGrowth.MainActivateFromIndex();

            if (TryGetSpellSettable(context.StateData, SuperDuckCardId.Waboku, out var waboku))
                return waboku.SetSpellTrap();

            if (TryGetSpellSettable(context.StateData, SuperDuckCardId.WakingTheDragon, out var wakingTheDragon))
                return wakingTheDragon.SetSpellTrap();

            if (TryGetSpellSettable(context.StateData, SuperDuckCardId.JustiBreak, out var justiBreak))
                return justiBreak.SetSpellTrap();

            if (TryGetSpellSettable(context.StateData, SuperDuckCardId.FloodgateTrapHole, out var floodgateTrapHole))
                return floodgateTrapHole.SetSpellTrap();

            if (TryGetActivable(context.StateData, SuperDuckCardId.BreakerTheMagicalWarrior, out var breaker) &&
                await BreakerTheMagicalWarriorCondition(breaker))
            {
                return breaker.MainActivateFromIndex();
            }

            foreach (var cardId in new[]
            {
                SuperDuckCardId.HunterDragon,
                SuperDuckCardId.SonicDuck,
                SuperDuckCardId.DragonZombie,
                SuperDuckCardId.CyberEndDragon,
                SuperDuckCardId.DestinyEndDragon,
                SuperDuckCardId.ArcanaKnightJoker,
                SuperDuckCardId.SuperVehicroidJumboDrill
            })
            {
                if (TryGetReposable(context.StateData, cardId, out var card) && ReposableCondition(card))
                    return card.Reposition();
            }

            if (TryGetReposable(context.StateData, SuperDuckCardId.OjamaKing, out var ojamaKing) &&
                ReposableConditionForOjamaKing(ojamaKing))
            {
                return ojamaKing.Reposition();
            }

            var filteredState = CloneStateWithoutSphereKuriboh(context.StateData);
            return await base.OnIdleCmdAsync(context with { StateData = filteredState }, ct);
        }

        public override async Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            foreach (var card in context.StateData.SelectableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[OnSelectChain] [Selectable cards] => {card.ID}");

            bool battleStartPhase = (DuelPhase)context.Header.CurrentPhase == DuelPhase.BattleStart;
            bool currentBotPlayer = context.Header.CurrentPlayer == 1;
            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(SuperDuckCardId.Waboku, out var waboku, out var index) &&
                !_selectChainMap.ContainsKey(waboku.ID) &&
                battleStartPhase &&
                currentBotPlayer)
            {
                _selectChainMap.Add(SuperDuckCardId.Waboku, true);
                return index;
            }

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(SuperDuckCardId.FloodgateTrapHole, out _, out index))
            {
                bool isUniqueTrap = await CurrentField.IsUniqueTrapChain(SuperDuckCardId.FloodgateTrapHole);
                if (!isUniqueTrap)
                    return index;
            }

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(SuperDuckCardId.JustiBreak, out _, out index))
                return index;

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(SuperDuckCardId.WakingTheDragon, out _, out index))
                return index;

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(SuperDuckCardId.SphereKuriboh, out _, out index))
                return index;

            return await base.OnSelectChainAsync(context, ct);
        }

        public override Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default)
        {
            ConsoleUtils.PrintColored("[override] [OnSelectEffectYn]");

            if (context.StateData.EffectCard.IsCode(SuperDuckCardId.SphereKuriboh) &&
                !_selectChainMap.ContainsKey(SuperDuckCardId.Waboku))
            {
                return Task.FromResult(true);
            }

            if (context.StateData.EffectCard.IsCode(SuperDuckCardId.WakingTheDragon))
                return Task.FromResult(true);

            return base.OnSelectEffectYnAsync(context, ct);
        }

        public override async Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (context.StateData.SelectionHint == (int)DHint.SPSUMMON)
            {
                ConsoleUtils.PrintColored("[override][OnSelectCard] - Selecting Card to SPSUMMON");
                byte[] botCards = context.StateData.SelectableCards
                    .Select((card, index) => new { Card = card, Index = index })
                    .OrderByDescending(x => x.Card.Attack)
                    .Take(context.StateData.Minimum)
                    .Select(x => (byte)x.Index)
                    .ToArray();

                if (botCards.Any())
                {
                    ConsoleUtils.PrintColored($"Selected {botCards.Length} bot-owned cards for SPSUMMON.");
                    return botCards;
                }
            }

            return await base.OnSelectCardAsync(context, ct);
        }

        public override Task<CardPosition> OnSelectPositionAsync(DecisionContext<SelectPositionData> context, CancellationToken ct = default)
        {
            ConsoleUtils.PrintColored("[override][OnSelectPosition]");
            var positions = context.StateData.Positions ?? Array.Empty<byte>();
            if (context.StateData.CardID == SuperDuckCardId.OjamaKing && positions.Length > 1)
            {
                return Task.FromResult((CardPosition)positions[1]);
            }

            return base.OnSelectPositionAsync(context, ct);
        }

        private static IdleCmdData CloneStateWithoutSphereKuriboh(IdleCmdData state)
        {
            state.SummonableCards = (state.SummonableCards ?? Array.Empty<DynamicCard>())
                .Where(card => card.ID != SuperDuckCardId.SphereKuriboh)
                .ToArray();
            return state;
        }

        private async Task<bool> HammerShotCondition(DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.HammerShot);
            bool opponentHasStrongest = await CurrentField.IsFaceUpAttackStrongestMonsterPlayer();
            if (canActivate && opponentHasStrongest)
                ConsoleUtils.PrintColored("HammerShotCondition: Can activate Hammer Shot (Opponent has strongest monster)");
            return canActivate && opponentHasStrongest;
        }

        private async Task<bool> EnchantingFittingRoomCondition(DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.EnchantingFittingRoom);
            int botMonstersCount = await CurrentField.GetMonstersCount();
            ConsoleUtils.PrintColored($"[EnchantingFittingRoomCondition] botMonstersCount {botMonstersCount}");
            bool botMonstersCondition = botMonstersCount <= 1;
            if (canActivate && botMonstersCondition)
                ConsoleUtils.PrintColored("EnchantingFittingRoom: Can activate Enchanting Fitting Room");
            return canActivate && botMonstersCondition;
        }

        private async Task<bool> AllureOfDarknessCondition(DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.AllureOfDarkness);
            bool botHasDarkAttribute = await CurrentField.HasCardWithAttributeInHand(CardAttribute.Dark);
            if (canActivate && botHasDarkAttribute)
                ConsoleUtils.PrintColored("AllureOfDarknessCondition: Can activate Allure of Darkness");
            return canActivate && botHasDarkAttribute;
        }

        private async Task<bool> BreakerTheMagicalWarriorCondition(DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.BreakerTheMagicalWarrior);
            int playerSpellsCount = await CurrentField.GetPlayerSpellsCount();
            ConsoleUtils.PrintColored($"BreakerTheMagicalWarriorCondition : playerSpellsCount {playerSpellsCount}");
            bool playerHasSpells = playerSpellsCount > 0;
            if (canActivate && playerHasSpells)
                ConsoleUtils.PrintColored("BreakerTheMagicalWarrior: Can activate Breaker The Magical Warrior");
            return canActivate && playerHasSpells;
        }

        private static bool ReposableCondition(DynamicCard card) => card.IsFaceup() && card.IsDefense();

        private static bool ReposableConditionForOjamaKing(DynamicCard card) => card.IsFaceup() && card.IsAttack();
    }
}
