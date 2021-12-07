using System;

namespace KinderlyProcessor.Core.Models
{

    public class KinderlyMembership
    {
        public string Id { get; set; }

        public Contact Contact { get; set; }

        public bool KinderlyMember { get; set; }

        public bool KinderlyLead { get; set; }

        public string KinderlyIntegrationDate { get; set; } = DateTime.Now.ToString("s");
    }
}
