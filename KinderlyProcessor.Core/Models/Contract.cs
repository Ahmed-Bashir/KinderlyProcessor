using Newtonsoft.Json;
using System.Collections.Generic;

namespace KinderlyProcessor.Core.Models
{
    public class Contract
    {
        [JsonProperty("DigitalChildminderContract-Wales")]
        public string DigitalChildminderContractWales { get; set; }

        [JsonProperty("Short-TermChildmindingDigitalContract")]
        public string ShortTermChildmindingDigitalContract { get; set; }

        [JsonProperty("Short-termregisteredchildmindingcontracts")]
        public string ShortTermRegisteredchildmindingcontracts { get; set; }

        [JsonProperty("Childmindingassistantcontracts")]
        public string ChildMindingAssistantContracts { get; set; }

        [JsonProperty("Nannycontracts")]
        public string NannyContracts { get; set; }

        public string NannyDigitalContract { get; set; }

        public List<Product> Products { get; set; }
    }
}
