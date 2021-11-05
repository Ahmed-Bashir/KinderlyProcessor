using System;
using System.Collections.Generic;
using System.Text;

namespace KinderlyProcessor.Models
{
    public class PaceyMembership
    {

        public string contact_id { get; set; }
        public string email { get; set; }
        public string postcode { get; set; }
        public string last_name { get; set; }
        public string first_name { get; set; }
        public Membership membership { get; set; }
   

    }


    public class Membership
    {
        public string membership_sub_type { get; set; }
        public string membership_class { get; set; }
        public string membership_no { get; set; }
        public string membership_grade { get; set; }
        public string membership_id { get; set; }
        public string payment_method { get; set; }
        public string membership_valid_from { get; set; }
        public string membership_valid_to { get; set; }

    }

}
