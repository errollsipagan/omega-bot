using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.MasterDuel.DarkMagician;
using BotTest.Game.AI.Bots.MasterDuel.HandTraps;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.RushDuel.SevensRoad;
using BotTest.Game.AI.Bots.SpeedDuel.Amazoness;
using BotTest.Game.AI.Bots.SpeedDuel.MachineAngelAscension;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.AI.Heuristics;
using BotTest.Game.Base;
using OmegaBotWrapper;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.Heuristics.Examples;

namespace BotTest.Web;

public static class DeckCatalog
{
    public static readonly IReadOnlyList<DeckDefinition> Definitions =
    [
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.BlueEyes.ToString(), DeckManager.GetDeckName(MasterDuelDeck.BlueEyes), DeckManager.GetDeckHash(MasterDuelDeck.BlueEyes), () => new BlueEyesBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.DarkMagician.ToString(), DeckManager.GetDeckName(MasterDuelDeck.DarkMagician), DeckManager.GetDeckHash(MasterDuelDeck.DarkMagician), () => new DarkMagicianBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.SwordSoul.ToString(), DeckManager.GetDeckName(MasterDuelDeck.SwordSoul), DeckManager.GetDeckHash(MasterDuelDeck.SwordSoul), () => new SwordSoulBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("MasterDuel", "Master Duel", MasterDuelDeck.HandTraps.ToString(), DeckManager.GetDeckName(MasterDuelDeck.HandTraps), DeckManager.GetDeckHash(MasterDuelDeck.HandTraps), () => new HandTrapsBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateClassic("SpeedDuel", "Speed Duel", SpeedDuelDeck.SuperDuck.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.SuperDuck), () => new SuperDuckActions(SpeedDuelDeck.SuperDuck)),
        CreateBrain("SpeedDuel", "Speed Duel", SpeedDuelDeck.MachineAngelAscension.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.MachineAngelAscension), DeckManager.GetDeckHash(SpeedDuelDeck.MachineAngelAscension), () => new MachineAngelAscensionBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateClassic("SpeedDuel", "Speed Duel", SpeedDuelDeck.Amazoness.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.Amazoness), () => new AmazonessActions(SpeedDuelDeck.Amazoness)),
        CreateBrain("RushDuel", "Rush Duel", RushDuelDeck.SevensRoad.ToString(), DeckManager.GetDeckName(RushDuelDeck.SevensRoad), DeckManager.GetDeckHash(RushDuelDeck.SevensRoad), () => new SevensRoadBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateBrain("RushDuel", "Rush Duel", RushDuelDeck.CustomRushDeck.ToString(), DeckManager.GetDeckName(RushDuelDeck.CustomRushDeck), DeckManager.GetDeckHash(RushDuelDeck.CustomRushDeck), () => new SevensRoadBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())),
        CreateHeuristic("SpeedDuel", "Speed Duel", SpeedDuelDeck.SuperDuck.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.SuperDuck), DeckManager.GetDeckHash(SpeedDuelDeck.SuperDuck)),
        CreateHeuristic("SpeedDuel", "Speed Duel", SpeedDuelDeck.Amazoness.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.Amazoness), DeckManager.GetDeckHash(SpeedDuelDeck.Amazoness))
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

    private static DeckDefinition CreateClassic(
        string formatId,
        string formatName,
        string deckId,
        string deckName,
        Func<BotActionsBase> actionsFactory) =>
        new(formatId, formatName, deckId, deckName, deckId, () => new DuelBotWrapper(actionsFactory()));

    private static DeckDefinition CreateBrain(
        string formatId,
        string formatName,
        string deckId,
        string deckName,
        string deckHash,
        Func<IBotBrain> brainFactory) =>
        new(formatId, formatName, deckId, deckName, deckId, () => new DuelBotWrapper(deckName, deckHash, brainFactory()));

    private static DeckDefinition CreateHeuristic(
        string formatId,
        string formatName,
        string baseDeckId,
        string baseDeckName,
        string deckHash) =>
        new(
            formatId,
            formatName,
            $"{baseDeckId}.BOT",
            $"{baseDeckName} BOT",
            baseDeckId,
            () => new DuelBotWrapper(
                $"{baseDeckName} BOT",
                deckHash,
                new ExampleAggroBotBrain(new WindBotInspiredCardEvaluator(), new DefaultBoardEvaluator())));
}

public sealed record DeckDefinition(
    string FormatId,
    string FormatName,
    string DeckId,
    string DeckName,
    string AssetDeckId,
    Func<DuelBotWrapper> Factory);
