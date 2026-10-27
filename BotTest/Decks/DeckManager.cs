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
        BlueEyes,
        DarkMagician,
        SwordSoul,
        HandTraps,
        KaibaPureDragon,
        KaibaHeavyFusion,
        KaibaAntiMeta,
        KaibaGunBarrelDragon,
        KaibaHandBoardControl,
    }

    /// <summary>
    /// Enum to identify available Speed Duel decks.
    /// </summary>
    public enum SpeedDuelDeck
    {
        SuperDuck,
        MachineAngelAscension,
        Amazoness
    }

    /// <summary>
    /// Enum to identify available Speed Duel decks.
    /// </summary>
    public enum RushDuelDeck
    {
        SevensRoad,
        CustomRushDeck
    }

    /// <summary>
    /// Manages deck names and hashes, providing access to deck data and options.
    /// </summary>
    public static class DeckManager
    {
        private static readonly Dictionary<MasterDuelDeck, (string DeckName, string DeckHash)> _decks = new()
        {
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
            },
            {
                MasterDuelDeck.SwordSoul,
                (
                    DeckName: "SwordSoul",
                    DeckHash: "M2ewnRXLrM0Tx+y54yHzEn8DVhje1JXKCMIr+iaxwjBIHQgvNjJkhOGcDG9mGF6vfI8BhivP3GQFYXteKSYYBtkFwk48M1hB9oFwTnMnIwwrecxkgGEmrUdgfLcmhSlMl481ueUQGHcsV2KszJrAcOSNDwPDUgUWEI5m+seyz2wy0/b9SSxbjD8wgrDphQlg/wAA"
                )
            },
            {
                MasterDuelDeck.HandTraps,
                (
                    DeckName: "HandTraps",
                    DeckHash: "s2Fer3yPAYaVPGbC8XWp+Yww/LdzDisMbzI6A8dyLgFMft9LGWE4v+w9AwxfPFTB+s5tAwsMGy2qZQDhtY+4mGD4gpwRKwh7f0pnguGXZ06wwrDFxmQWGNbklmKG4aDvXXC8wX0THD+MmAvHpuzNTDAcMfkok7B7KgOIvlR8jRWGQX4GAA=="
                )
            },
            {
                MasterDuelDeck.KaibaPureDragon,
                (
                    DeckName: "Kaiba Pure Dragon",
                    DeckHash: "02ZYujKcFYZrOCxYJgU/ZgDh27IXWddL+TLE9ZQztrI9heOKvD5WEFbV5WIEYfUCITB2/sTDAsIsz/4xgbDphX7GHa22DD8197GUuGszvlf9x7J3eT6jV4Ens/NaXhazX/OYQPgr50GW9+qBDIWHLVi0N+9lCv76i+X3LENma8+TrE8meTJeUs5jzC5zZn669hMzyI0A"
                    // DeckHash: "02ZYujKcFYZrOCxYQPjcjIlMMLxzsQILDGcmcTDAcCvbU0YYrsjrYwVhmU0cTDtabRlA2HPyNCYQ/qm5j+X3HRHGEndtxveq/1j2Ls9n9CrwZHZey8vylfMgy3v1QIbCwxYs2pv3MgndXMAU/PUXy+9ZhszWnidZn0zyZLyknMeYXebM/HTtJ2aQGwE="
                )
            },
            {
                MasterDuelDeck.KaibaHeavyFusion,
                (
                    DeckName: "Kaiba Heavy Fusion",
                    DeckHash: "02JYujKcFYZrOCxYJgU/ZgDh27IXWVvZnjLC8BVbA2YQVi8QYgRhlmf/mEDY9EI/o8wmDqZPAWtYYXhHqy1DiAIn40/NfSwl7tqM71X/sexdns8Iwl4FnszOa3lZzH7NYwLhr5wHWd6rBzIUHrZg0d68lyn46y+W37MMma09T7I+meTJeEk5jzG7zJkZ5D4A"
                    // DeckHash: "02JYujKcFYZrOCxYzs2YyATCOxcrsMBwK9tTRhi+YmvADMIsz/4xgbDphX5GmU0cTJ8C1rDC8I5WW4YQBU7Gn5r7WGC4xF2b8b3qP5a9y/MZQdirwJPZeS0vy1fOgyzv1QMZCg9bsGhv3ssU/PUXy+9ZhszWnidZn0zyZLyknMeYXebMDHIfAA=="
                )
            },
            {
                MasterDuelDeck.KaibaAntiMeta,
                (
                    DeckName: "Kaiba Anti-Meta",
                    DeckHash: "02RYujKcFYRrOCxYQHhS8GMGEL4te5F1vZQvQ1xPOSPLs39MIGx6oZ9RZhMH06eANaww/FJ2JhhHvXVkBuEdrbYMjzdoMf/U3MdS4q7N+F71H8tkXhuWvcvzGb0KPJmd1/KyvFcPZDA6n81UeNiCRXvzXiarewXMwV9/sYDw71mGzNaeJ1mfTPJk3C0hyHxJOY8R5D4A"
                )
            },
            {
                MasterDuelDeck.KaibaGunBarrelDragon,
                (
                    DeckName: "Kaiba Gun Barrel Dragon",
                    // DeckHash: "02JYujKcFYRrOCxYQHhS8GMGEM5M4gBjHu/bLCDM8uwfEwibXuhnlNnEwfQpYA0rDL+UnQnGUW8dmXe02jKEKHAyTmRfzfRTcx9Libs243vVfyx7l+czehV4Mjuv5WXJ32fI8l49kKHwsAWL9ua9TFb3CpiDv/5i+T3LkNna8yTrk0mejCDs7DiXBYQvKecxgtwIAA=="
                    DeckHash: "02CYFPwYjjOTOOCY1/s2CwybXuhnlNnEwfQpYA0rDL+UnQnGUW8dmUF44sSHjDtabRlCFDgZJ7KvZvqpuY+lxF2b8b3qPxavAk9m57W8LPn7DFmMzmczFR62YNHevJdJ6OYCOP49y5DZ2vMk65NJnowg7Ow4lwWEQe4CAA=="
                )
            },
            {
                MasterDuelDeck.KaibaHandBoardControl,
                (
                    DeckName: "Kaiba Hand & Board Control",
                    DeckHash: "02RYujKctYbDggWEJwU/ZoDhzCQOOOb1vs1y+OQVBhA2vdDPKLOJg+lTwBpWGI5668gMwhMnPmTc0WrLEKLAyTiRfTXTT819LCXu2ozvVf+x7F2ez+hV4MnsvJaXxdnen8W2o5cJhL9yHmR5rx7IoL15L5PVvQJmEA7++ovl9yxDZmvPk6xPJnkygtwIAA=="
                )
            },
        };

        private static readonly Dictionary<SpeedDuelDeck, (string DeckName, string DeckHash)> _SDdecks = new()
        {
            {
                SpeedDuelDeck.SuperDuck,
                (
                    DeckName: "Super Duck",
                    DeckHash: "k2UrsbZkv3n/IqNv3AGG901iDPwskxlBGCTm93YHKwyfiuEB453hP5l7NziypLn/ZgSpB6lb8F+Vpf//EgYQNtHfxgwyC4S7p4sz3NnQzQLCc67HMhRaazKZFMaDzQDpB9lzePJNMAbpA9kDAA=="
                )
            },
            {
                SpeedDuelDeck.MachineAngelAscension,
                (
                    DeckName: "Machine Angel Ascension",
                    DeckHash: "k2IrsLZkLxBNZQRhKe18ZhDeuL+MFYT7mmexwLBSgjkDDM8P8QPjL7XRrDD8Xj2Q4a9XEhMIP+Sax9g9XZzhzoZulqMJcxhMCuNZD0++ySh6ZgMTDMtqJzODMMh+AA=="
                )
            }
            ,
            {
                SpeedDuelDeck.Amazoness,
                (
                    DeckName: "Amazoness",
                    DeckHash: "k2F7rx7IsGz2F0YQblGtY1iT9hmOG35wssKwb0sWMwzv2/CDBYbbeG6wgjCb1EcGEP5SG8162tqSvXu6OMOdDd0sc67HMjx/+4PJpDCe1dvmJDPIDhD2+17KCMIbpH4z+8YdANsFAA=="
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
            },
            {
                RushDuelDeck.CustomRushDeck,
                (
                    DeckName: "Custom Rush Deck",
                    DeckHash: "M2S4wKnJHvxOC46fXUBgPwdtOO6cogXHG2IROPAVAq+LReAiYQS+slQbjhW9tOBYe5Y2HGshYXUkbO6BwL+3a8BxTKEWHIP8AAA="
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
