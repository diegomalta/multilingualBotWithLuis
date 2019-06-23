using Newtonsoft.Json;

namespace MultiLingualBot.Translation.Model
{
    public class TranslatorResult
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }
    }
}
