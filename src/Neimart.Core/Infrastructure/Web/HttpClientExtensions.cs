using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Neimart.Core.Infrastructure.Web
{
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent httpContent)
        {
            using var stream = await httpContent.ReadAsStreamAsync();
            var jsonReader = new JsonTextReader(new StreamReader(stream));

            return _jsonSerializer.Deserialize<T>(jsonReader);
        }

        public static Task<HttpResponseMessage> SendAsJsonAsync(this HttpClient client, string url, HttpMethod method, object data = null, IDictionary<string, string> headers = null)
        {
            var stream = new MemoryStream();
            var jsonWriter = new JsonTextWriter(new StreamWriter(stream));
            _jsonSerializer.Serialize(jsonWriter, data);
            jsonWriter.Flush();
            stream.Position = 0;
            var request = new HttpRequestMessage(method, url)
            {
                Content = new StreamContent(stream)
            };

            if (headers != null)
            {
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);
            }

            request.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json");

            return client.SendAsync(request);
        }
    }
}
