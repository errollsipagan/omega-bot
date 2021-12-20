using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using Newtonsoft.Json;
namespace OmegaBotWrapper
{
    /// <summary>
    /// Contains helpers for interacting with the web api
    /// </summary>
    public static class WebHandler
    {
        /// <summary>
        /// HTTP client instance to be used on the requests
        /// </summary>
        private readonly static HttpClient WebClient = new();
        /// <summary>
        /// Makes a request to the given url and returns the string of the response body
        /// </summary>
        /// <param name="url">URL which the request will be made to</param>
        /// <returns>String with the response body data</returns>
        public static async Task<string> DownloadString(string url)
        {
            return await WebClient.GetStringAsync(url);
        }
        /// <summary>
        /// Makes a request to the given url and returns the object representation of the json response body
        /// </summary>
        /// <typeparam name="T">Type of the object to be requested</typeparam>
        /// <param name="url">URL which the request will be made to</param>
        /// <returns>Object with the response body data on it</returns>
        public static async Task<T?> DownloadObject<T>(string url)
        {
            string json = await DownloadString(url);
            return json.FromJSON<T>();
        }
    }
}
