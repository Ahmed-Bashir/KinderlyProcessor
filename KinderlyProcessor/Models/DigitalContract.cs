using System;
using System.Collections.Generic;
using System.Text;

namespace KinderlyProcessor.Models
{
   public class DigitalContract
    {
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string invoice { get; set; }
        public List<OrderSummary> order_summary { get; set; }
        public string KinderlyIntegrationDate { get; set; } = DateTime.Now.ToString("s");

    }
    public class OrderSummary
    {
        public string urid { get; set; }
        public string order_ref_id { get; set; }
        public double quantity { get; set; }
    }


}
