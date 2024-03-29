﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MultiLingualBot.Translation.Model;
using Newtonsoft.Json;

namespace MultiLingualBot.Translation
{
    public class Translator
    {
        private const string Host = "https://api.cognitive.microsofttranslator.com";
        private const string PathTraslate = "/translate?api-version=3.0";
        private const string PathDetect = "/detect?api-version=3.0";
        private const string UriParams = "&to=";

        private static HttpClient _client = new HttpClient();

        private readonly string _key;

        public Translator(IConfiguration configuration)
        {
            var key = configuration["TranslatorKey"];
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public async Task<string> TranslateAsync(string text, string targetLocale, CancellationToken cancellationToken = default(CancellationToken))
        {
            // From Cognitive Services translation documentation:
            // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-csharp-translate
            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var request = new HttpRequestMessage())
            {
                var uri = Host + PathTraslate + UriParams + targetLocale;
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                var response = await _client.SendAsync(request, cancellationToken);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TranslatorResponse[]>(responseBody);

                return result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;
            }
        }

        public async Task<string> DetectLanguageAsync(string inputText, CancellationToken cancellationToken = default(CancellationToken))
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                var uri = Host + PathDetect;
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                var response = await _client.SendAsync(request, cancellationToken);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DetectResult[]>(responseBody);

                return result?.FirstOrDefault()?.Language;
            }
        }
    }
}
