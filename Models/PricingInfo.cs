using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pricing.Models
{
    public class PricingInfo
    {
        public string PricingId { get; set; }
        public string PricingName { get; set; }
        public string CustomerName { get; set; }
        public long FeeRemaining { get; set; }

        public string BoreholeName { get; set; }

        public string WellName { get; set; }

        public DateTime PudDate { get; set; }

        public DateTime CompletionDate { get; set; }

        public string Class { get; set; }

        public string Status { get; set; }
    }
}