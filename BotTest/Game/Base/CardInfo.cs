namespace BotTest.Game.Base
{

    /// <summary>
    /// Stores Card ID and its corresponding name.
    /// </summary>
    public class CardInfo
    {
        public int CardId { get; }
        public string CardName { get; }

        public CardInfo(int cardId, string cardName)
        {
            CardId = cardId;
            CardName = cardName;
        }
    }
}
