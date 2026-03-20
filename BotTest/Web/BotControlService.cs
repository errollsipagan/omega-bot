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
using BotTest.Utils;
using OmegaBotWrapper;

namespace BotTest.Web;

public sealed class BotControlService
{
    private const int MaxLogEntries = 16;

    private readonly object _sync = new();
    private readonly IReadOnlyList<FormatOption> _formatOptions;

    private string _selectedFormat;
    private string _selectedDeckId;
    private string _selectedDeckName;
    private bool _isBusy;
    private string _status = "Ready";
    private string _statusDetail = "Choose a format and a deck to start the duel.";
    private DateTimeOffset _lastUpdated = DateTimeOffset.Now;
    private readonly List<LogEntry> _logs = new();
    private Task? _runningTask;

    public BotControlService()
    {
        _formatOptions = BuildFormatOptions();

        var defaultFormat = _formatOptions.First();
        var defaultDeck = defaultFormat.Decks.First();

        _selectedFormat = defaultFormat.Id;
        _selectedDeckId = defaultDeck.Id;
        _selectedDeckName = defaultDeck.Name;

        AddLog("Panel initialized.", "system");
    }

    public OptionsResponse GetOptions() => new(_formatOptions, _selectedFormat, _selectedDeckId);

    public BotStateResponse GetState()
    {
        lock (_sync)
        {
            return new BotStateResponse(
                _selectedFormat,
                _selectedDeckId,
                _selectedDeckName,
                _isBusy,
                _status,
                _statusDetail,
                _lastUpdated,
                _logs.ToArray());
        }
    }

    public CommandResult UpdateSelection(string format, string deckId)
    {
        lock (_sync)
        {
            if (_isBusy)
            {
                return CommandResult.Fail("Wait for the current match to finish before changing your selection.");
            }

            var formatOption = _formatOptions.FirstOrDefault(option => option.Id.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatOption is null)
            {
                return CommandResult.Fail("Invalid format.");
            }

            var deckOption = formatOption.Decks.FirstOrDefault(option => option.Id.Equals(deckId, StringComparison.OrdinalIgnoreCase));
            if (deckOption is null)
            {
                return CommandResult.Fail("Invalid deck for the selected format.");
            }

            _selectedFormat = formatOption.Id;
            _selectedDeckId = deckOption.Id;
            _selectedDeckName = deckOption.Name;
            _status = "Ready";
            _statusDetail = $"Selection ready: {deckOption.Name} in {formatOption.Name}.";
            _lastUpdated = DateTimeOffset.Now;

            AddLog($"Selected deck: {deckOption.Name} ({formatOption.Name}).", "system");

            return CommandResult.Ok("Updated selection.");
        }
    }

    public CommandResult StartDuel()
    {
        DeckSelection selection;

        lock (_sync)
        {
            if (_isBusy)
            {
                return CommandResult.Fail("A duel is already underway.");
            }

            selection = new DeckSelection(_selectedFormat, _selectedDeckId, _selectedDeckName);
            _isBusy = true;
            _status = "Connecting";
            _statusDetail = $"Connecting the bot to the deck {selection.DeckName}...";
            _lastUpdated = DateTimeOffset.Now;
            AddLog($"Starting the duel with {selection.DeckName}.", "system");
            _runningTask = Task.Run(() => ExecuteDuelAsync(selection, requestRematch: false));
        }

        return CommandResult.Ok("Duel started.");
    }

    public CommandResult RequestRematch()
    {
        DeckSelection selection;

        lock (_sync)
        {
            if (_isBusy)
            {
                return CommandResult.Fail("Wait for the current flow to finish before requesting a rematch.");
            }

            selection = new DeckSelection(_selectedFormat, _selectedDeckId, _selectedDeckName);
            _isBusy = true;
            _status = "Rematch";
            _statusDetail = "Requesting rematch and preparing a new connection...";
            _lastUpdated = DateTimeOffset.Now;
            AddLog("Requesting rematch.", "system");
            _runningTask = Task.Run(() => ExecuteDuelAsync(selection, requestRematch: true));
        }

        return CommandResult.Ok("Rematch requested.");
    }

    public CommandResult ResetSelection()
    {
        lock (_sync)
        {
            if (_isBusy)
            {
                return CommandResult.Fail("Cannot reset while a duel is in progress.");
            }

            _status = "Ready";
            _statusDetail = "Selection kept. You can switch decks or start a new duel.";
            _lastUpdated = DateTimeOffset.Now;
            AddLog("Panel ready for a new duel.", "system");

            return CommandResult.Ok("Panel reset.");
        }
    }

    private async Task ExecuteDuelAsync(DeckSelection selection, bool requestRematch)
    {
        void OnMessageLogged(string message) => AddLog(message, "engine");

        ConsoleUtils.MessageLogged += OnMessageLogged;

        try
        {
            if (requestRematch)
            {
                UpdateStatus("Rematch", "Sending the rematch request to Omega...");
                await DuelBotWrapper.RematchDuel();
                AddLog("Rematch request sent.", "system");
                await Task.Delay(500);
            }

            var botActions = CreateBotActions(selection);
            var bot = new DuelBotWrapper(botActions);

            UpdateStatus("Connecting", $"Trying to connect the bot using {selection.DeckName}...");
            var connected = await bot.Connect();

            if (!connected)
            {
                UpdateStatus("Connection failed", "No bot slot is available or Omega did not respond as expected.");
                AddLog("Failed to connect to the bot.", "error");
                return;
            }

            UpdateStatus("Dueling", $"Bot is dueling with {selection.DeckName}. Follow the match in Omega.");
            AddLog("Bot connected successfully.", "system");

            await bot.Start();

            UpdateStatus("Finished", "The duel is over. You can request a rematch or start another deck.");
            AddLog("Duel finished.", "system");
        }
        catch (Exception ex)
        {
            UpdateStatus("Error", $"An error occurred during execution: {ex.Message}");
            AddLog($"Error: {ex.Message}", "error");
        }
        finally
        {
            ConsoleUtils.MessageLogged -= OnMessageLogged;

            lock (_sync)
            {
                _isBusy = false;
                _runningTask = null;
                _lastUpdated = DateTimeOffset.Now;
            }
        }
    }

    private void UpdateStatus(string status, string detail)
    {
        lock (_sync)
        {
            _status = status;
            _statusDetail = detail;
            _lastUpdated = DateTimeOffset.Now;
        }
    }

    private void AddLog(string message, string kind = "info")
    {
        lock (_sync)
        {
            _logs.Insert(0, new LogEntry(DateTimeOffset.Now, message, kind));
            if (_logs.Count > MaxLogEntries)
            {
                _logs.RemoveRange(MaxLogEntries, _logs.Count - MaxLogEntries);
            }
        }
    }

    private static IReadOnlyList<FormatOption> BuildFormatOptions()
    {
        return
        [
            new FormatOption(
                "MasterDuel",
                "Master Duel",
                DeckManager.GetMasterDuelDeckOptions()
                    .Select(deck => new DeckOptionDto(deck.Id.ToString()!, deck.Name))
                    .ToArray()),
            new FormatOption(
                "SpeedDuel",
                "Speed Duel",
                DeckManager.GetSpeedDuelDeckOptions()
                    .Select(deck => new DeckOptionDto(deck.Id.ToString()!, deck.Name))
                    .ToArray()),
            new FormatOption(
                "RushDuel",
                "Rush Duel",
                DeckManager.GetRushDuelDeckOptions()
                    .Select(deck => new DeckOptionDto(deck.Id.ToString()!, deck.Name))
                    .ToArray())
        ];
    }

    private static BaseBotActions CreateBotActions(DeckSelection selection)
    {
        return selection.FormatId switch
        {
            "MasterDuel" => Enum.Parse<MasterDuelDeck>(selection.DeckId) switch
            {
                MasterDuelDeck.BlueEyes => new BlueEyesActions(MasterDuelDeck.BlueEyes),
                MasterDuelDeck.DarkMagician => new DarkMagicianActions(MasterDuelDeck.DarkMagician),
                MasterDuelDeck.SwordSoul => new SwordSoulActions(MasterDuelDeck.SwordSoul),
                MasterDuelDeck.HandTraps => new HandTrapsActions(MasterDuelDeck.HandTraps),
                _ => throw new InvalidOperationException($"No Master Duel implementation found for deck: {selection.DeckId}")
            },
            "SpeedDuel" => Enum.Parse<SpeedDuelDeck>(selection.DeckId) switch
            {
                SpeedDuelDeck.SuperDuck => new SuperDuckActions(SpeedDuelDeck.SuperDuck),
                SpeedDuelDeck.MachineAngelAscension => new MachineAngelAscensionActions(SpeedDuelDeck.MachineAngelAscension),
                SpeedDuelDeck.Amazoness => new AmazonessActions(SpeedDuelDeck.Amazoness),
                _ => throw new InvalidOperationException($"No Speed Duel implementation found for deck: {selection.DeckId}")
            },
            "RushDuel" => Enum.Parse<RushDuelDeck>(selection.DeckId) switch
            {
                RushDuelDeck.SevensRoad => new SevensRoadActions(RushDuelDeck.SevensRoad),
                _ => throw new InvalidOperationException($"No Rush Duel implementation found for deck: {selection.DeckId}")
            },
            _ => throw new InvalidOperationException($"Unsupported format: {selection.FormatId}")
        };
    }
}

public sealed record SelectionRequest(string Format, string DeckId);

public sealed record CommandResult(bool Success, string Message)
{
    public static CommandResult Ok(string message) => new(true, message);

    public static CommandResult Fail(string message) => new(false, message);
}

public sealed record OptionsResponse(
    IReadOnlyList<FormatOption> Formats,
    string SelectedFormat,
    string SelectedDeckId);

public sealed record BotStateResponse(
    string SelectedFormat,
    string SelectedDeckId,
    string SelectedDeckName,
    bool IsBusy,
    string Status,
    string StatusDetail,
    DateTimeOffset LastUpdated,
    IReadOnlyList<LogEntry> Logs);

public sealed record FormatOption(string Id, string Name, IReadOnlyList<DeckOptionDto> Decks);

public sealed record DeckOptionDto(string Id, string Name);

public sealed record LogEntry(DateTimeOffset Timestamp, string Message, string Kind);

internal sealed record DeckSelection(string FormatId, string DeckId, string DeckName);
