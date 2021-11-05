using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinderlyProcessor.Models
{
    public class Contract
    {

        [JsonProperty("DigitalChildminderContract-Wales")]
        public string DigitalChildminderContractWales { get; set; }

        [JsonProperty("Short-TermChildmindingDigitalContract")]
        public string ShortTermChildmindingDigitalContract { get; set; }

        [JsonProperty("Short-termregisteredchildmindingcontracts")]
        public string ShortTermRegisteredchildmindingcontracts { get; set; }
        public string Childmindingassistantcontracts { get; set; }
        public string Nannycontracts { get; set; }
        public string NannyDigitalContract { get; set; }

        public List<Product> Products { get; set; }


    }
}
