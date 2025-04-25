#pragma warning disable CS8618
namespace BotTest.Decks
{
    /// <summary>
    /// Represents a deck option with an ID and name.
    /// </summary>
    public class DeckOption
    {
        public object Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Enum to identify available Master Duel decks.
    /// </summary>
    public enum MasterDuelDeck
    {
        BotTest,
        BlueEyes,
        DarkMagician
    }

    /// <summary>
    /// Enum to identify available Speed Duel decks.
    /// </summary>
    public enum SpeedDuelDeck
    {
        SuperDuck
    }

    /// <summary>
    /// Enum to identify available Speed Duel decks.
    /// </summary>
    public enum RushDuelDeck
    {
        SevensRoad
    }

    /// <summary>
    /// Manages deck names and hashes, providing access to deck data and options.
    /// </summary>
    public static class DeckManager
    {
        private static readonly Dictionary<MasterDuelDeck, (string DeckName, string DeckHash)> _decks = new()
        {
            {
                MasterDuelDeck.BotTest,
                (
                    DeckName: "BotTest",
                    DeckHash: "02ewdIhgme1uxrBGP5RRw8eeAcQuezqXkSHLjOmjWzsziM954RZTdvo9BpB85p35LCAaxF+8MpxV6OYCsBybvwgjDE/+WQ7H52ZMZIJh1wUX4BhkJgh/4WWHY5CdMHtBGGQ2CIPcBsIg+0AY5D4QBrkDAA=="
                )
            },
            {
                MasterDuelDeck.BlueEyes,
                (
                    DeckName: "Blue-Eyes",
                    DeckHash: "M2cwO76VaXuDL/PiPRNZl2tYM77c7c00/7c1S1KpEyuIXrwynHUpEC8DYpAcCN/svcRYwWvLzJgoAFFbs4MJhpsfHGSBYcM7gozue2oZYHgH1J6EDF7GrWV8jGsrtRiuSR9kTO+zZemaxcCclLWZGaQHhEH2g/CzOgWGs07hrKYXJjDDcLBZARMMM8U0wfHk3mCmvCJdhuMLrzJnRSsygtwOAA=="
                )
            },
            {
                MasterDuelDeck.DarkMagician,
                (
                    DeckName: "Dark Magician",
                    DeckHash: "M2Pg6L/AlCiRw/AwzIVp1wVGlhU/LzEKqaiygMQnWrEyXNjkyAhiz3Y/zrxhEyPDui9nmDKmngfTETNzmUD6uC48A8vnWUszsMj2MIPYvzYKsTjHpbLc3fSYESQP0vN6QiMrSD2I/eaHHgPIXpDc4ycVDLFnXzDA1IPsAcmB7Nixby5T0EtB1msT/jGtEnRkCv0SwfCQ9T7rTqA4iG95KoHlMZAPYlfNrWE6b17H8gjIB+kFAA=="
                )
            }
        };

        private static readonly Dictionary<SpeedDuelDeck, (string DeckName, string DeckHash)> _SDdecks = new()
        {
            {
                SpeedDuelDeck.SuperDuck,
                (
                    DeckName: "Super Duck",
                    DeckHash: "k2UrsbZkv3n/IqNv3AGG901iDPwskxlBGCTm93YHKwyfiuEB453hP5l7NziypLn/ZgSpB6lb8F+Vpf//EgYQNtHfxgwyC4S7p4sz3NnQzQLCc67HMhRaazKZFMaDzQDpB9lzePJNMAbpA9kDAA=="
                )
            }
        };

        private static readonly Dictionary<RushDuelDeck, (string DeckName, string DeckHash)> _RDdecks = new()
        {
            {
                RushDuelDeck.SevensRoad,
                (
                    DeckName: "Sevens Road",
                    DeckHash: "s2KY+0STHYZ1V2jAcaaLJhwbAfkgvJVbC46fbNeA44uPEFj3uBYcb9ytwa65AoEtz0Ow8TJNOL4ENAuGw45D8FluBDYF6oPhjUD+eiS8CQm/0EPgpVYI7H0cgkHqAQ=="
                )
            }
        };

        /// <summary>
        /// Gets the deck name for the specified Master Duel deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckName(MasterDuelDeck deckId)
        {
            return _decks[deckId].DeckName;
        }

        /// <summary>
        /// Gets the deck hash for the specified Master Duel deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck hash.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckHash(MasterDuelDeck deckId)
        {
            return _decks[deckId].DeckHash;
        }

        /// <summary>
        /// Gets a list of available Master Duel deck options for selection.
        /// </summary>
        /// <returns>A list of DeckOption objects containing DeckId and DeckName.</returns>
        public static IEnumerable<DeckOption> GetMasterDuelDeckOptions()
        {
            return _decks.Select(d => new DeckOption { Id = d.Key, Name = d.Value.DeckName });
        }

        /// <summary>
        /// Gets the deck name for the specified Speed Duel deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckName(SpeedDuelDeck deckId)
        {
            return _SDdecks[deckId].DeckName;
        }

        /// <summary>
        /// Gets the deck hash for the specified Speed Duel deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck hash.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckHash(SpeedDuelDeck deckId)
        {
            return _SDdecks[deckId].DeckHash;
        }

        /// <summary>
        /// Gets a list of available Speed Duel deck options for selection.
        /// </summary>
        /// <returns>A list of DeckOption objects containing DeckId and DeckName.</returns>
        public static IEnumerable<DeckOption> GetSpeedDuelDeckOptions()
        {
            return _SDdecks.Select(d => new DeckOption { Id = d.Key, Name = d.Value.DeckName });
        }

        /// <summary>
        /// Gets the deck name for the specified Rush Duel deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckName(RushDuelDeck deckId)
        {
            return _RDdecks[deckId].DeckName;
        }

        /// <summary>
        /// Gets the deck hash for the specified Rush Duel deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck hash.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckHash(RushDuelDeck deckId)
        {
            return _RDdecks[deckId].DeckHash;
        }

        /// <summary>
        /// Gets a list of available Rush Duel deck options for selection.
        /// </summary>
        /// <returns>A list of DeckOption objects containing DeckId and DeckName.</returns>
        public static IEnumerable<DeckOption> GetRushDuelDeckOptions()
        {
            return _RDdecks.Select(d => new DeckOption { Id = d.Key, Name = d.Value.DeckName });
        }
    }
}
