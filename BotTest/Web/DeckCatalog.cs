using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.AI.Bots.MasterDuel.DarkMagician;
using BotTest.Game.AI.Bots.MasterDuel.HandTraps;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.RushDuel.SevensRoad;
using BotTest.Game.AI.Bots.SpeedDuel.Amazoness;
using BotTest.Game.AI.Bots.SpeedDuel.MachineAngelAscension;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;

namespace BotTest.Web;

public static class DeckCatalog
{
    public static readonly IReadOnlyList<DeckDefinition> Definitions =
    [
        new("MasterDuel", "Master Duel", MasterDuelDeck.BlueEyes.ToString(), DeckManager.GetDeckName(MasterDuelDeck.BlueEyes), () => new BlueEyesActions(MasterDuelDeck.BlueEyes)),
        new("MasterDuel", "Master Duel", MasterDuelDeck.DarkMagician.ToString(), DeckManager.GetDeckName(MasterDuelDeck.DarkMagician), () => new DarkMagicianActions(MasterDuelDeck.DarkMagician)),
        new("MasterDuel", "Master Duel", MasterDuelDeck.SwordSoul.ToString(), DeckManager.GetDeckName(MasterDuelDeck.SwordSoul), () => new SwordSoulActions(MasterDuelDeck.SwordSoul)),
        new("MasterDuel", "Master Duel", MasterDuelDeck.HandTraps.ToString(), DeckManager.GetDeckName(MasterDuelDeck.HandTraps), () => new HandTrapsActions(MasterDuelDeck.HandTraps)),
        new("SpeedDuel", "Speed Duel", SpeedDuelDeck.SuperDuck.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.SuperDuck), () => new SuperDuckActions(SpeedDuelDeck.SuperDuck)),
        new("SpeedDuel", "Speed Duel", SpeedDuelDeck.MachineAngelAscension.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.MachineAngelAscension), () => new MachineAngelAscensionActions(SpeedDuelDeck.MachineAngelAscension)),
        new("SpeedDuel", "Speed Duel", SpeedDuelDeck.Amazoness.ToString(), DeckManager.GetDeckName(SpeedDuelDeck.Amazoness), () => new AmazonessActions(SpeedDuelDeck.Amazoness)),
        new("RushDuel", "Rush Duel", RushDuelDeck.SevensRoad.ToString(), DeckManager.GetDeckName(RushDuelDeck.SevensRoad), () => new SevensRoadActions(RushDuelDeck.SevensRoad))
    ];

    public static IReadOnlyList<FormatOption> GetFormatOptions()
    {
        return Definitions
            .GroupBy(definition => new { definition.FormatId, definition.FormatName })
            .Select(group => new FormatOption(
                group.Key.FormatId,
                group.Key.FormatName,
                group.Select(definition => new DeckOptionDto(definition.DeckId, definition.DeckName)).ToArray()))
            .ToArray();
    }

    public static BaseBotActions CreateBotActions(string formatId, string deckId)
    {
        var definition = Definitions.FirstOrDefault(item =>
            item.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase) &&
            item.DeckId.Equals(deckId, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
            throw new InvalidOperationException($"No bot implementation found for format '{formatId}' and deck '{deckId}'.");

        return definition.Factory();
    }
}

public sealed record DeckDefinition(
    string FormatId,
    string FormatName,
    string DeckId,
    string DeckName,
    Func<BaseBotActions> Factory);
