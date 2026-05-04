using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.SpeedDuel.Amazoness
{
    public sealed class AmazonessBotBrain : DeckBotBrain
    {
        private bool _sendToGrave;

        public AmazonessBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }

        public override async Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (TryGetActivable(context.StateData, AmazonessCardId.BattleCityBestie, out var bestie))
                return bestie.MainActivateFromIndex();

            if (TryGetActivable(context.StateData, AmazonessCardId.ReinforcementOfTheArmy, out var rota))
                return rota.MainActivateFromIndex();

            if (TryGetSummonable(context.StateData, AmazonessCardId.AmazonessPrincess, out var princess))
                return princess.NormalSummon();

            foreach (var cardId in new[]
            {
                AmazonessCardId.CosmicCyclone,
                AmazonessCardId.NightBeam,
                AmazonessCardId.MysticalSpaceTyphoon
            })
            {
                if (TryGetActivable(context.StateData, cardId, out var removal) &&
                    await DestroyOrRemoveSpellTrapCondition())
                {
                    return removal.MainActivateFromIndex();
                }
            }

            if (TryGetSpellSettable(context.StateData, AmazonessCardId.AmazonessCall, out var call))
                return call.SetSpellTrap();

            if (TryGetSpellSettable(context.StateData, AmazonessCardId.AmazonessOnslaught, out var onslaught) &&
                await SetAmazonessOnslaughtCondition())
            {
                return onslaught.SetSpellTrap();
            }

            if (TryGetSpellSettable(context.StateData, AmazonessCardId.MindCrush, out var mindCrush))
                return mindCrush.SetSpellTrap();

            if (TryGetActivable(context.StateData, AmazonessCardId.EnemyController, out var enemyController))
                return enemyController.MainActivateFromIndex();

            if (TryGetReposable(context.StateData, AmazonessCardId.AmazonessQueen, out var queen) && ReposableCondition(queen))
                return queen.Reposition();

            return await base.OnIdleCmdAsync(context, ct);
        }

        public override async Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            bool battleStartPhase = (DuelPhase)context.Header.CurrentPhase == DuelPhase.BattleStart;
            if (context.StateData.EffectCard.IsCode(AmazonessCardId.AmazonessPrincess) && !battleStartPhase)
                return true;

            IEnumerable<DynamicCard> botMonster = await CurrentField.GetMonsterInLocation(CardLocation.MonsterZone, controller: 0);
            IEnumerable<DynamicCard> botSpellTrap = await CurrentField.GetCardsInSpellTrapZone();
            IEnumerable<DynamicCard> cardHand = await CurrentField.GetCardsInHand();

            bool hasAmazonessPrincess = botMonster.Count(card => card.ID == AmazonessCardId.AmazonessPrincess) >= 2;
            bool hasOneAmazonessQueen = botMonster.Count(card => card.ID == AmazonessCardId.AmazonessQueen) == 1;
            bool hasTwoAmazonessQueen = botMonster.Count(card => card.ID == AmazonessCardId.AmazonessQueen) == 2;
            cardHand = cardHand.Where(card => !card.IsCode(AmazonessCardId.AmazonessQueen) && !card.IsCode(AmazonessCardId.AmazonessOnslaught));
            bool hasCardInHand = cardHand.Any();
            botSpellTrap = botSpellTrap.Where(card => !card.IsCode(AmazonessCardId.AmazonessOnslaught));
            bool hasSpellTrap = botSpellTrap.Any();

            if (context.StateData.EffectCard.IsCode(AmazonessCardId.AmazonessPrincess) &&
                (hasAmazonessPrincess || (hasCardInHand && hasOneAmazonessQueen) || hasSpellTrap || !hasTwoAmazonessQueen))
            {
                return true;
            }

            if (context.StateData.EffectCard.IsCode(AmazonessCardId.AmazonessOnslaught))
                return true;

            return await base.OnSelectEffectYnAsync(context, ct);
        }

        public override async Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            foreach (var card in context.StateData.SelectableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[OnSelectChain] [override] [Selectable cards] => {card.ID}");

            if (await CurrentField.PlayerDestroyAllSpellAndTraps())
                return 0;

            if (await CurrentField.PlayerDestroyAllCards())
                return 0;

            var spellTrapResponse = await TrySelectProtectedTrapChain(context.StateData, await CurrentField.GetPlayerSpellsCardsInSpellZone());
            if (spellTrapResponse.HasValue)
                return spellTrapResponse.Value;

            var monsterResponse = await TrySelectProtectedTrapChain(
                context.StateData,
                (await CurrentField.GetPlayerMonstersCardsInMonsterZone()).Where(card => card.IsFaceup() && card.HasType(CardType.Effect) && card.CurrentChainTargets.Any()));
            if (monsterResponse.HasValue)
                return monsterResponse.Value;

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(AmazonessCardId.AmazonessCall, out _, out var index) &&
                await AmazonessCallCondition())
            {
                return index;
            }

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(AmazonessCardId.AmazonessOnslaught, out _, out index) &&
                await AmazonessOnslaughtCondition())
            {
                return index;
            }

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(AmazonessCardId.MindCrush, out _, out index))
                return index;

            return await base.OnSelectChainAsync(context, ct);
        }

        public override async Task<BattleCmdResponse> OnBattleCmdAsync(DecisionContext<BattleCmdData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);
            var attackableCards = context.StateData.AttackableCards ?? Array.Empty<DynamicCard>();

            if (attackableCards.Any())
            {
                var amazonessOnslaught = (await CurrentField.GetCardsInSpellZone())
                    .Where(card => card.IsCode(AmazonessCardId.AmazonessOnslaught) && card.IsFaceup());

                if (amazonessOnslaught.Any())
                    return attackableCards.First().DeclareAttack();
            }

            return await base.OnBattleCmdAsync(context, ct);
        }

        public override async Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (context.StateData.SelectionHint == (int)DHint.OPERATECARD)
            {
                _sendToGrave = false;
                IEnumerable<DynamicCard> cardHand = await CurrentField.GetCardsInHand();
                IEnumerable<DynamicCard> cardGrave = await CurrentField.GetCardsInGrave();

                bool amazonessPrincessInMonsterZone = await CurrentField.HasCardInBotMonsterZone(AmazonessCardId.AmazonessPrincess);
                var amazonessPrincessInGrave = cardGrave.Where(card => card.IsCode(AmazonessCardId.AmazonessPrincess));
                var amazonessPrincessInHand = cardHand.Where(card => card.IsCode(AmazonessCardId.AmazonessPrincess));

                if (!amazonessPrincessInHand.Any() && !amazonessPrincessInMonsterZone && !amazonessPrincessInGrave.Any())
                    return CurrentField.FilterSelectableCardsById(context.StateData, AmazonessCardId.AmazonessPrincess);

                var amazonessCards = cardHand.Where(card => card.IsCode(
                    AmazonessCardId.AmazonessCall,
                    AmazonessCardId.AmazonessOnslaught,
                    AmazonessCardId.AmazonessPrincess,
                    AmazonessCardId.AmazonessQueen));

                if (!amazonessCards.Any())
                    return CurrentField.FilterSelectableCardsById(context.StateData, AmazonessCardId.AmazonessQueen);

                var amazonessQueenInHand = cardHand.Where(card => card.IsCode(AmazonessCardId.AmazonessQueen));
                var amazonessCardsInGrave = cardGrave.Where(card => card.IsCode(
                    AmazonessCardId.AmazonessCall,
                    AmazonessCardId.AmazonessOnslaught,
                    AmazonessCardId.AmazonessPrincess,
                    AmazonessCardId.AmazonessQueen));

                if (amazonessQueenInHand.Any() && !amazonessCardsInGrave.Any())
                {
                    _sendToGrave = true;
                    return CurrentField.FilterSelectableCardsById(context.StateData, AmazonessCardId.AmazonessQueen);
                }

                var amazonessQueenInGrave = cardGrave.Where(card => card.IsCode(AmazonessCardId.AmazonessQueen));

                if (amazonessQueenInHand.Any() && amazonessQueenInGrave.Any() && amazonessPrincessInHand.Any())
                    return CurrentField.FilterSelectableCardsById(context.StateData, AmazonessCardId.AmazonessOnslaught);

                if (amazonessQueenInHand.Any() && amazonessQueenInGrave.Any())
                    return CurrentField.FilterSelectableCardsById(context.StateData, AmazonessCardId.AmazonessPrincess);
            }

            if (context.StateData.SelectionHint == (int)DHint.TOGRAVE)
            {
                var filteredCards = context.StateData.SelectableCards
                    .Select((card, index) => new { Card = card, OriginalIndex = index })
                    .Where(x => x.Card.ID != AmazonessCardId.AmazonessQueen)
                    .Where(x => x.Card.ID != AmazonessCardId.AmazonessOnslaught)
                    .Take(context.StateData.Minimum)
                    .Select(x => (byte)x.OriginalIndex)
                    .ToArray();

                if (filteredCards.Any())
                    return filteredCards;
            }

            if (context.StateData.SelectionHint == (int)DHint.SPSUMMON)
            {
                byte[] botCards = context.StateData.SelectableCards
                    .Select((card, index) => new { Card = card, Index = index })
                    .OrderByDescending(x => x.Card.Attack)
                    .Take(context.StateData.Minimum)
                    .Select(x => (byte)x.Index)
                    .ToArray();

                if (botCards.Any())
                    return botCards;
            }

            return await base.OnSelectCardAsync(context, ct);
        }

        public override async Task<int> OnAnnounceCardAsync(DecisionContext<AnnounceCardData> context, CancellationToken ct = default)
        {
            IEnumerable<DynamicCard> playerCards = await CurrentField.GetPlayerCardsInHand();
            if (playerCards.Any())
            {
                Random rng = new();
                var announcedCard = playerCards.ElementAt(rng.Next(0, playerCards.Count()));
                return announcedCard.Alias != 0 ? announcedCard.Alias : announcedCard.ID;
            }

            return await base.OnAnnounceCardAsync(context, ct);
        }

        public override Task<int> OnSelectOptionAsync(DecisionContext<SelectOptionData> context, CancellationToken ct = default) =>
            Task.FromResult(_sendToGrave ? 1 : 0);

        private static bool ReposableCondition(DynamicCard card) => card.IsFaceup() && card.IsDefense();

        private async Task<bool> DestroyOrRemoveSpellTrapCondition() => await CurrentField.GetPlayerSpellsCount() > 0;

        private async Task<bool> AmazonessOnslaughtCondition()
        {
            var cardsActivated = (await CurrentField.GetPlayerSpellsCardsInSpellZone()).Where(card => card.IsFaceup());
            foreach (var card in cardsActivated)
            {
                if (card.CurrentChainTargets.Length > 0 && card.CurrentChainTargets[0].Controller != 0)
                {
                    var getBotCardTargets = await CurrentField.GetBotFieldCard(
                        controller: 0,
                        location: card.CurrentChainTargets[0].Location,
                        sequence: card.CurrentChainTargets[0].Sequence);

                    if (getBotCardTargets.IsCode(AmazonessCardId.AmazonessOnslaught))
                        return true;
                }
            }

            return (DuelPhase)CurrentHeader.CurrentPhase == DuelPhase.BattleStart;
        }

        private async Task<bool> AmazonessCallCondition()
        {
            var cardsActivated = (await CurrentField.GetPlayerSpellsCardsInSpellZone()).Where(card => card.IsFaceup());
            foreach (var card in cardsActivated)
            {
                if (card.CurrentChainTargets.Length > 0 && card.CurrentChainTargets[0].Controller != 0)
                {
                    var getBotCardTargets = await CurrentField.GetBotFieldCard(
                        controller: 0,
                        location: card.CurrentChainTargets[0].Location,
                        sequence: card.CurrentChainTargets[0].Sequence);

                    if (getBotCardTargets.IsCode(AmazonessCardId.AmazonessCall) && getBotCardTargets.IsFacedown())
                        return true;
                }
            }

            return (DuelPhase)CurrentHeader.CurrentPhase == DuelPhase.BattleStart;
        }

        private async Task<bool> SetAmazonessOnslaughtCondition()
        {
            var spellsTrapsCards = await CurrentField.GetCardsInSpellZone();
            return spellsTrapsCards.Count(card => card.IsCode(AmazonessCardId.AmazonessOnslaught)) < 2;
        }

        private async Task<int?> TrySelectProtectedTrapChain(SelectChainData state, IEnumerable<DynamicCard> chainSources)
        {
            foreach (var card in chainSources.Where(card => card.IsFaceup()))
            {
                if (card.CurrentChainTargets.Length == 0)
                    continue;

                int controllerTarget = card.CurrentChainTargets[0].Controller;
                if (controllerTarget == 0)
                    continue;

                int sequenceTarget = card.CurrentChainTargets[0].Sequence;
                if (state.SelectableCards.TryGetByIDWithZone(AmazonessCardId.AmazonessCall, sequenceTarget, out _, out var index))
                    return index;

                if (state.SelectableCards.TryGetByIDWithZone(AmazonessCardId.AmazonessOnslaught, sequenceTarget, out _, out index))
                    return index;
            }

            return null;
        }
    }
}
