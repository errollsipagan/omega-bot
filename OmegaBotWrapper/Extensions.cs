using Newtonsoft.Json;
using OmegaBotWrapper.Struct;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OmegaBotWrapper
{
    public static class Extensions
    {
        /// <summary>
        /// JSON Serializer used for serialization
        /// </summary>
        private readonly static JsonSerializer Serializer = new();
        /// <summary>
        /// Extension method for deserializing strings that contains json data to its object representation
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="json">JSON string</param>
        /// <returns>Object representation of the json data</returns>
        public static T? FromJSON<T>(this string json)
        {
            using StringReader stringReader = new(json);
            using JsonTextReader jsonReader = new(stringReader);
            T? result = Serializer.Deserialize<T>(jsonReader);
            return result;
        }
        /// <summary>
        /// Encodes an array of byte data into base 64 string
        /// </summary>
        /// <param name="data">Data to be encoded</param>
        /// <returns>String representation of the base 64 encoded data</returns>
        public static string Base64Encode(this byte[] data)
        {
            if (data == null || data.Length <= 0)
                return string.Empty;
            return Convert.ToBase64String(data);
        }
        /// <summary>
        /// Transforms a base 64 string into its safe mode representation
        /// </summary>
        /// <param name="b64">Unsafe base 64 string to be converted</param>
        /// <returns>Base 64 string converted into safe mode</returns>
        public static string ToSafeB64(this string b64)
        {
            return b64.Replace("=", string.Empty).Replace('/', '_').Replace('+', '-');
        }
        /// <summary>
        /// Sends a response to the server with given data to the given bot
        /// </summary>
        /// <param name="data">Data to be sent</param>
        /// <param name="botId">ID of the target bot to receive</param>
        /// <returns>True if there was no error, False otherwise</returns>
        public static async Task<bool> SendResponsePacket(this byte[] data, int botId)
        {
            string resp = data.Base64Encode().ToSafeB64();
            var result = await WebHandler.DownloadObject<ResponseReason>($"{Const.SET_BOT_RESPONSE}/{botId}/{resp}");
            return result.Code == 200;
        }
        /// <summary>
        /// Sends a response to the server from the binary writer to the given bot
        /// </summary>
        /// <param name="pkt">Binary packet with data</param>
        /// <param name="botId">ID of the target bot to receive</param>
        /// <returns>True if there was no error, False otherwise</returns>
        public static async Task<bool> SendResponsePacket(this BinaryWriter pkt, int botId)
        {
            byte[] data = ((MemoryStream)pkt.BaseStream).ToArray();
            return await data.SendResponsePacket(botId);
        }
        /// <summary>
        /// Gets the action index for the activation of given effect of the card
        /// </summary>
        /// <param name="card">Card which the activation belongs to</param>
        /// <param name="description">Description index of the card activation effect</param>
        /// <returns>The action index to be used to activate the given effect or -1 if not found</returns>
        public static int GetActivationIndexForDescription(this DynamicCard card, int description)
        {
            foreach (var action in card.ActionActivateIndex)
            {
                if (action.ActionDescription == description)
                    return action.ActionIndex;
            }
            return -1;
        }
        /// <summary>
        /// Gets all combinations from enumerable
        /// </summary>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="elements">Enumerable of elements</param>
        /// <param name="k">Size of resulting collection</param>
        /// <returns>Collection of combinations with k elements</returns>
        public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { Array.Empty<T>() } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).GetCombinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }        
    }
}
