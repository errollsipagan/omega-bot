using BotTest.Decks;
using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.BlueEyes
{
    public sealed class BlueEyesBotBrain : DeckBotBrain
    {
        public BlueEyesBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }

        public override async Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (TryGetSummonable(context.StateData, BlueEyesCardId.DragonSpiritOfWhite, out var dragonSpirit) &&
                await DragonSpiritOfWhiteSummon(context.StateData))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Dragon Spirit Of White");
                return dragonSpirit.NormalSummon();
            }

            if (TryGetSummonable(context.StateData, BlueEyesCardId.MaidenOfEyesOfBlue, out var maidenOfEyes))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Maiden Of Eyes Of Blue");
                return maidenOfEyes.NormalSummon();
            }

            if (TryGetSummonable(context.StateData, BlueEyesCardId.MaidenOfWhite, out var maidenOfWhite))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Maiden Of White");
                return maidenOfWhite.NormalSummon();
            }

            if (TryGetSummonable(context.StateData, BlueEyesCardId.BlueEyesWhiteDragon, out var blueEyes))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Blue Eyes White Dragon");
                return blueEyes.NormalSummon();
            }

            if (TryGetMonsterSettable(context.StateData, BlueEyesCardId.TheWhiteStoneOfAncients, out var whiteStoneSet))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: The White Stone Of Ancients");
                return whiteStoneSet.SetMonster();
            }

            if (TryGetReposable(context.StateData, BlueEyesCardId.TheWhiteStoneOfAncients, out var whiteStoneRepos))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: The White Stone Of Ancients");
                return whiteStoneRepos.Reposition();
            }

            if (TryGetSpecialSummon(context.StateData, BlueEyesCardId.BlueEyesAlternativeWhiteDragon, out var alternative))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Blue Eyes Alternative White Dragon");
                return alternative.SpecialSummon();
            }

            if (TryGetSpecialSummon(context.StateData, BlueEyesCardId.BlueEyesSpiritDragon, out var spiritDragon))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Blue Eyes Spirit Dragon");
                return spiritDragon.SpecialSummon();
            }

            if (TryGetSpecialSummon(context.StateData, BlueEyesCardId.HieraticSunDragonOverlordOfHeliopolis, out var heliopolis))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Hieratic Sun Dragon Overlord Of Heliopolis");
                return heliopolis.SpecialSummon();
            }

            if (TryGetSpecialSummon(context.StateData, BlueEyesCardId.Number46Dragluon, out var dragluon))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Number 46 Dragluon");
                return dragluon.SpecialSummon();
            }

            if (TryGetActivable(context.StateData, BlueEyesCardId.TheWhiteStoneOfAncients, out var whiteStoneActivate))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: The White Stone Of Ancients");
                return whiteStoneActivate.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, BlueEyesCardId.MaidenOfWhite, out maidenOfWhite))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Maiden Of White");
                return maidenOfWhite.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, BlueEyesCardId.MaidenOfEyesOfBlue, out maidenOfEyes))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Maiden Of Eyes Of Blue");
                return maidenOfEyes.MainActivateFromIndex();
            }

            if (TryGetActivable(context.StateData, BlueEyesCardId.BingoMachineGo, out var bingoMachineGo))
            {
                ConsoleUtils.PrintColored("Bot Brain => ExecuteAction. Card: Bingo Machine Go");
                return bingoMachineGo.MainActivateFromIndex();
            }

            return await base.OnIdleCmdAsync(context, ct);
        }

        public override Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default)
        {
            if (context.StateData.EffectCard.IsCode(BlueEyesCardId.DragonSpiritOfWhite))
            {
                ConsoleUtils.PrintColored("Activating effect for Dragon Spirit of White");
                return Task.FromResult(true);
            }

            return base.OnSelectEffectYnAsync(context, ct);
        }

        private async Task<bool> DragonSpiritOfWhiteSummon(IdleCmdData state)
        {
            int playerSpellZoneCount = await GetPlayerSpellZoneCountAsync();
            bool hasSpellCards = playerSpellZoneCount > 0;
            bool canSummon = (state.SummonableCards ?? Array.Empty<DynamicCard>()).Any(c => c.ID == BlueEyesCardId.DragonSpiritOfWhite);

            if (hasSpellCards && canSummon)
                ConsoleUtils.PrintColored($"[[[[BINGO!]]]] DragonSpiritOfWhiteSummon: Can summon (SpellZoneCount: {playerSpellZoneCount})");

            return hasSpellCards && canSummon;
        }
    }
}
