using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.MasterDuel.DarkMagician;
using BotTest.Game.AI.Bots.MasterDuel.HandTraps;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.MasterDuel.KaibaPureDragon;
using BotTest.Game.AI.Bots.MasterDuel.KaibaHeavyFusion;
using BotTest.Game.AI.Bots.MasterDuel.KaibaAntiMeta;
using BotTest.Game.AI.Bots.MasterDuel.KaibaGunBarrelDragon;
using BotTest.Game.AI.Bots.MasterDuel.KaibaHandBoardControl;
using BotTest.Game.AI.Bots.RushDuel.SevensRoad;
using BotTest.Game.AI.Bots.SpeedDuel.Amazoness;
using BotTest.Game.AI.Bots.SpeedDuel.MachineAngelAscension;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.AI.Heuristics;
using OmegaBotWrapper;
using OmegaBotWrapper.Heuristics;

namespace BotTest.Web;

public static class DeckCatalog
{
    public static readonly IReadOnlyList<DeckDefinition> Definitions =
    [
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.BlueEyes.ToString(), DeckManager.GetDeckName(MasterDuelDeck.BlueEyes), DeckManager.GetDeckHash(MasterDuelDeck.BlueEyes), () => new BlueEyesBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.DarkMagician.ToString(), DeckManager.GetDeckName(MasterDuelDeck.DarkMagician), DeckManager.GetDeckHash(MasterDuelDeck.DarkMagician), () => new DarkMagicianBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.SwordSoul.ToString(), DeckManager.GetDeckName(MasterDuelDeck.SwordSoul), DeckManager.GetDeckHash(MasterDuelDeck.SwordSoul), () => new SwordSoulBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.HandTraps.ToString(), DeckManager.GetDeckName(MasterDuelDeck.HandTraps), DeckManager.GetDeckHash(MasterDuelDeck.HandTraps), () => new HandTrapsBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.KaibaPureDragon.ToString(), DeckManager.GetDeckName(MasterDuelDeck.KaibaPureDragon), DeckManager.GetDeckHash(MasterDuelDeck.KaibaPureDragon), () => new KaibaPureDragonBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.KaibaHeavyFusion.ToString(), DeckManager.GetDeckName(MasterDuelDeck.KaibaHeavyFusion), DeckManager.GetDeckHash(MasterDuelDeck.KaibaHeavyFusion), () => new KaibaHeavyFusionBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.KaibaAntiMeta.ToString(), DeckManager.GetDeckName(MasterDuelDeck.KaibaAntiMeta), DeckManager.GetDeckHash(MasterDuelDeck.KaibaAntiMeta), () => new KaibaAntiMetaBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.KaibaGunBarrelDragon.ToString(), DeckManager.GetDeckName(MasterDuelDeck.KaibaGunBarrelDragon), DeckManager.GetDeckHash(MasterDuelDeck.KaibaGunBarrelDragon), () => new KaibaGunBarrelDragonBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.KaibaHandBoardControl.ToString(), DeckManager.GetDeckName(MasterDuelDeck.KaibaHandBoardControl), DeckManager.GetDeckHash(MasterDuelDeck.KaibaHandBoardControl), () => new KaibaHandBoardControlBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("SpeedDuel", "Speed Duel", SpeedDuelDeck.SuperDuck.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.SuperDuck), DeckManager.GetDeckHash(SpeedDuelDeck.SuperDuck), () => new SuperDuckBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("SpeedDuel", "Speed Duel", SpeedDuelDeck.MachineAngelAscension.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.MachineAngelAscension), DeckManager.GetDeckHash(SpeedDuelDeck.MachineAngelAscension), () => new MachineAngelAscensionBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("SpeedDuel", "Speed Duel", SpeedDuelDeck.Amazoness.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.Amazoness), DeckManager.GetDeckHash(SpeedDuelDeck.Amazoness), () => new AmazonessBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("RushDuel", "Rush Duel", RushDuelDeck.SevensRoad.ToString(), DeckManager.GetDeckName(RushDuelDeck.SevensRoad), DeckManager.GetDeckHash(RushDuelDeck.SevensRoad), () => new SevensRoadBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("RushDuel", "Rush Duel", RushDuelDeck.CustomRushDeck.ToString(), DeckManager.GetDeckName(RushDuelDeck.CustomRushDeck), DeckManager.GetDeckHash(RushDuelDeck.CustomRushDeck), () => new SevensRoadBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
    ];

    public static IReadOnlyList<FormatOption> GetFormatOptions()
    {
        return Definitions
            .GroupBy(definition => new { definition.FormatId, definition.FormatName })
            .Select(group => new FormatOption(
                group.Key.FormatId,
                group.Key.FormatName,
                group.Select(definition => new DeckOptionDto(definition.DeckId, definition.DeckName, definition.AssetDeckId)).ToArray()))
            .ToArray();
    }

    public static DuelBotWrapper CreateBot(string formatId, string deckId)
    {
        var definition = Definitions.FirstOrDefault(item =>
            item.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) &&
            item.DeckId.Equals(deckId, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
            throw new InvalidOperationException($"No bot implementation found for format '{formatId}' and deck '{deckId}'.");

        return definition.Factory();
    }

    private static DeckDefinition CreateBrain(
        string formatId,
        string formatName,
        string deckId,
        string deckName,
        string deckHash,
        Func<IBotBrain> brainFactory) =>
        new(formatId, formatName, deckId, deckName, deckId, () => new DuelBotWrapper(deckName, deckHash, brainFactory()));
}

public sealed record DeckDefinition(
    string FormatId,
    string FormatName,
    string DeckId,
    string DeckName,
    string AssetDeckId,
    Func<DuelBotWrapper> Factory);
