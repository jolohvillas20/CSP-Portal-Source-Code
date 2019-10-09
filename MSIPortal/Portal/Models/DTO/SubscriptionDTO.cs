using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class SubscriptionDTO
    {
    }

    [MetadataType(typeof(SubscriptionsMetadata))]
    public partial class Subscriptions
    {
        // No field here
    }

    public class SubscriptionsMetadata
    {
        public int ID { get; set; }
        [Display(Name = "Customer ID")]
        public Nullable<int> CustomerID { get; set; }
        [Display(Name = "Subscription ID")]
        public string SubscriptionID { get; set; }
        [Display(Name = "Offers ID")]
        public string OffersId { get; set; }
        [Display(Name = "Offer Name")]
        public string OfferName { get; set; }
        public Nullable<int> Quantity { get; set; }
        [Display(Name = "Order Date Completed")]
        public Nullable<System.DateTime> CreationDate { get; set; }
        [Display(Name = "Effective Start Date")]
        public Nullable<System.DateTime> EffectiveStartDate { get; set; }
        [Display(Name = "Commitment End Date")]
        public Nullable<System.DateTime> CommitmentEndDate { get; set; }
        public string Status { get; set; }
        [Display(Name = "Billing Cycle")]
        public string BillingCycle { get; set; }
        [Display(Name = "Order Id")]
        public string OrderId { get; set; }
        [Display(Name = "Partner Id")]
        public string PartnerId { get; set; }



    }

    public class SubscriptionClass
    {
        public int ID { get; set; }
        public string CustomerID { get; set; }
        public string SubscriptionID { get; set; }
        public string OffersId { get; set; }
        public string OfferName { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> EffectiveStartDate { get; set; }
        public Nullable<System.DateTime> CommitmentEndDate { get; set; }
        public string Status { get; set; }
        public string BillingCycle { get; set; }
        public string OrderId { get; set; }
        public string PartnerId { get; set; }

    }

}