using System.Collections.Generic;
using Newtonsoft.Json;

namespace MultiLingualBot.Translation.Model
{
    public class TranslatorResponse
    {
        [JsonProperty("translations")]
        public IEnumerable<TranslatorResult> Translations { get; set; }
    }
}
