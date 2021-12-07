using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KinderlyProcessor.Core.Models
{
    public class DigitalContract
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("invoice")]
        public string Invoice { get; set; }

        [JsonProperty("order_summary")]
        public List<OrderSummary> OrderSummary { get; set; }

        public string KinderlyIntegrationDate { get; set; } = DateTime.Now.ToString("s");
    }

    public class OrderSummary
    {
        [JsonProperty("urid")]
        public string UrId { get; set; }

        [JsonProperty("order_ref_id")]
        public string OrderRefId { get; set; }

        [JsonProperty("quantity")]
        public double Quantity { get; set; }
    }
}
