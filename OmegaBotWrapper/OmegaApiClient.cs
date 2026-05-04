using Newtonsoft.Json;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper.States;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Typed HTTP client for the Omega bot API. Centralizes all endpoint calls and owns the HttpClient lifetime.
    /// </summary>
    public class OmegaApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly JsonSerializer _serializer = new();

        public static readonly OmegaApiClient Default = new();

        public OmegaApiClient(string baseUrl = Const.BASE_API, TimeSpan? timeout = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _http = new HttpClient { Timeout = timeout ?? TimeSpan.FromSeconds(5) };
        }

        private async Task<string> GetStringAsync(string url, CancellationToken ct = default) =>
            await _http.GetStringAsync(url, ct);

        private async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
        {
            string json = await GetStringAsync(url, ct);
            using StringReader sr = new(json);
            using JsonTextReader jr = new(sr);
            return _serializer.Deserialize<T>(jr)!;
        }

        public Task<BotList> GetBotList(CancellationToken ct = default) =>
            GetAsync<BotList>($"{_baseUrl}/bot-list", ct);

        public Task<BotGeneralState> GetBotState(int botId, CancellationToken ct = default) =>
            GetAsync<BotGeneralState>($"{_baseUrl}/get-bot-state/{botId}", ct);

        public Task<BotStateHeader> GetBotStateHeader(int botId, CancellationToken ct = default) =>
            GetAsync<BotStateHeader>($"{_baseUrl}/get-bot-state-header/{botId}", ct);

        public Task<T> GetBotStateAs<T>(int botId, CancellationToken ct = default) =>
            GetAsync<T>($"{_baseUrl}/get-bot-state/{botId}", ct);

        public async Task<bool> SetBotData(int botId, string deckName, string deckHashSafe, CancellationToken ct = default)
        {
            var result = await GetAsync<ResponseReason>($"{_baseUrl}/set-bot-data/{botId}/{deckName}/{deckHashSafe}", ct);
            return result.Code == 200;
        }

        public async Task<bool> SendBotResponse(int botId, string packetBase64Safe, CancellationToken ct = default)
        {
            var result = await GetAsync<ResponseReason>($"{_baseUrl}/set-bot-response/{botId}/{packetBase64Safe}", ct);
            return result.Code == 200;
        }

        public Task<FieldInfo> GetBotFieldInfo(int botId, CancellationToken ct = default) =>
            GetAsync<FieldInfo>($"{_baseUrl}/bot-field-info/{botId}", ct);

        public Task<FieldInfo> GetPlayerFieldInfo(CancellationToken ct = default) =>
            GetAsync<FieldInfo>($"{_baseUrl}/field-info", ct);

        public Task<DynamicCard> GetPlayerFieldCardInfo(int controller, int location, int sequence, CancellationToken ct = default) =>
            GetAsync<DynamicCard>($"{_baseUrl}/field-card-info/{controller}/{location}/{sequence}", ct);

        public Task<DynamicCard> GetBotFieldCardInfo(int botId, int controller, int location, int sequence, CancellationToken ct = default) =>
            GetAsync<DynamicCard>($"{_baseUrl}/bot-field-card-info/{botId}/{controller}/{location}/{sequence}", ct);

        public Task<DeckData> GetPlayerDeck(CancellationToken ct = default) =>
            GetAsync<DeckData>($"{_baseUrl}/current-deck", ct);

        public Task<DBCardList> GetMatchingCards(int[]? filters = null, CancellationToken ct = default)
        {
            string url = $"{_baseUrl}/card-list";
            if (filters != null && filters.Length > 0)
                url += $"/{string.Join(",", filters)}";
            return GetAsync<DBCardList>(url, ct);
        }

        public Task<string> RematchDuel(CancellationToken ct = default) =>
            GetStringAsync($"{_baseUrl}/rematch-duel", ct);
    }
}
