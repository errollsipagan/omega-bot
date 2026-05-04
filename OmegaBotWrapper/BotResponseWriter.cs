using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Serializes validated bot responses into the exact byte packets Omega expects.
    /// </summary>
    internal static class BotResponseWriter
    {
        public static byte[] WriteBattleCmd(BattleCmdResponse response) =>
            BuildPacket(writer =>
            {
                writer.Write((ushort)response.Action);
                writer.Write(response.Index);
            });

        public static byte[] WriteIdleCmd(IdleCmdResponse response) =>
            BuildPacket(writer =>
            {
                writer.Write((ushort)response.Action);
                writer.Write(response.Index);
            });

        public static byte[] WriteCardSelection(IList<byte> response) =>
            BuildIndexedBytePacket(response);

        public static byte[] WriteSelectChain(int response) =>
            WriteInt32(response);

        public static byte[] WriteSelectCounter(IList<short> response) =>
            BuildPacket(writer =>
            {
                writer.Write((byte)response.Count);
                foreach (short counterAmount in response)
                    writer.Write(counterAmount);
            });

        public static byte[] WriteZoneSelection(int response) =>
            WriteInt32(response);

        public static byte[] WriteBoolean(bool response) =>
            BuildPacket(writer => writer.Write((byte)(response ? 1 : 0)));

        public static byte[] WriteSelectOption(int response) =>
            WriteInt32(response);

        public static byte[] WriteRockPaperScissors(byte response) =>
            BuildPacket(writer => writer.Write(response));

        public static byte[] WriteSelectPosition(CardPosition response) =>
            BuildPacket(writer => writer.Write((byte)response));

        public static byte[] WriteSortCard(IList<byte> response) =>
            BuildIndexedBytePacket(response);

        public static byte[] WriteAnnounceAttribute(IList<CardAttribute> response) =>
            WriteInt32(response.Sum(attribute => (int)attribute));

        public static byte[] WriteAnnounceRace(IList<CardRace> response) =>
            WriteInt32(response.Sum(race => (int)race));

        public static byte[] WriteAnnounceNumber(int response) =>
            WriteInt32(response);

        public static byte[] WriteAnnounceCard(int response) =>
            WriteInt32(response);

        private static byte[] BuildIndexedBytePacket(IList<byte> response) =>
            BuildPacket(writer =>
            {
                writer.Write((byte)response.Count);
                foreach (byte optionIndex in response)
                    writer.Write(optionIndex);
            });

        private static byte[] WriteInt32(int response) =>
            BuildPacket(writer => writer.Write(response));

        private static byte[] BuildPacket(Action<BinaryWriter> write)
        {
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);
            write(writer);
            writer.Flush();
            return ms.ToArray();
        }
    }
}
