using Newtonsoft.Json;

namespace KinderlyProcessor.Core.Models
{
    public class PaceyMembership
    {
        [JsonProperty("contact_id")]
        public string ContactId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("membership")]
        public Membership Membership { get; set; }
    }

    public class Membership
    {
        [JsonProperty("membership_sub_type")]
        public string MembershipSubType { get; set; }

        [JsonProperty("membership_class")]
        public string MembershipClass { get; set; }

        [JsonProperty("membership_no")]
        public string MembershipNo { get; set; }

        [JsonProperty("membership_grade")]
        public string MembershipGrade { get; set; }

        [JsonProperty("membership_id")]
        public string MembershipId { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("membership_valid_from")]
        public string MembershipValidFrom { get; set; }

        [JsonProperty("membership_valid_to")]
        public string MembershipValidTo { get; set; }
    }
}
