using Newtonsoft.Json;

namespace MultiLingualBot.Translation.Model
{
    public class DetectResult
    {
        [JsonProperty("Language")]
        public string Language { get; set; }
        [JsonProperty("Score")]
        public float Score { get; set; }
        [JsonProperty("IsTranslationSupported")]
        public bool IsTranslationSupported { get; set; }
        [JsonProperty("IsTransliterationSupported")]
        public bool IsTransliterationSupported { get; set; }
    }
}
