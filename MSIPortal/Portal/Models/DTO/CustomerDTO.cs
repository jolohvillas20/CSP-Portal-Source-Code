using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    [MetadataType(typeof(CustomersMetadata))]
    public partial class Customers
    {
        // No field here
    }

    public class CustomersMetadata
    {
        //[Required(ErrorMessage = "Microsoft ID is required.")]
        //[Display(Name = "Microsoft ID")]
        [Key]
        public int ID { get; set; }
        public string MicrosoftID { get; set; }
        [Required(ErrorMessage = "Company Name is required.")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "Domain Name is required.")]
        [Display(Name = "Domain Name")]
        [RegularExpression("^[a-zA-Z0-9]+$")]
        public string DomainName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Email Address is required.")]
        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }
        public string Country { get; set; }

        [Required(ErrorMessage = "Address line 1 is required.")]
        [Display(Name = "Address line 1")]
        public string AddressLine1 { get; set; }
        [Display(Name = "Address line 2")]
        public string AddressLine2 { get; set; }
        [Display(Name = "State/Province")]
        public string State { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Zip/Postal Code is required.")]
        [Display(Name = "Zip/Postal Code")]
        public string ZipPostal { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        
        //[RegularExpression(@"^[0-9]*$", ErrorMessage = "Phone number must be numeric")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Microsoft cloud agreement is required.")]
        [Display(Name = "Microsoft cloud agreement")]
        public bool AgreedToMCA { get; set; }

        [Required(ErrorMessage = "Agreement Date is required.")]
        [Display(Name = "Agreement Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        public string AgreementDate { get; set; }

        [Display(Name = "Special Qualifications")]
        public int SpecialQualificationsID { get; set; }

        [Display(Name = "Indirect reseller selection")]
        public int IndirectResellerID { get; set; }
    }


    public class CustomerOffers
    {
        public Customers Customers { get; set; }
        public List<Catalog> Catalog { get; set; }

    }
    public class CustomerOffersOrders
    {
        public Customers Customers { get; set; }
        public List<_Categories> Catalog { get; set; }

    }

    public class Catalog
    {
        public string Category { get; set; }
        public List<Offers> Offers { get; set; }
    }
    public class CatalogwithOrders
    {
        public string OffersID { get; set; }
        public string PartnerOnRecord { get; set; }
        public int CustomerID { get; set; }
        public string category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> DealersPrice { get; set; }
        public Nullable<decimal> ComputedDealersPrice { get; set; }
        public string UnitType { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> MinimumQuantity { get; set; }
        public string Checked { get; set; }

    }

    public class _Categories
    {
        public string Category { get; set; }
        public List<CatalogwithOrders> OrderItems { get; set; }
    }

    public class CustomerSubscriptionDTO
    {
        public class CustomerSubscription
        {
            public string OfferId { get; set; }
            public int LicenseCount { get; set; }
            public decimal Price { get; set; }


        }

        public string CompanyName { get; set; }
        public string DomainName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipPostal { get; set; }

        public int SpecialQualificationsID { get; set; }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public int IndirectResellerID { get; set; }


        //ORDER
        public int ResellerID { get; set; }
        public string Billing { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalAmount { get; set; }

        public List<CustomerSubscription> CustomerSubscriptions { get; set; }
    }

    public class CustomerSubscriptionWithIDDTO
    {
        public class CustomerSubscription
        {
            public string OfferId { get; set; }
            public int LicenseCount { get; set; }
            public decimal Price { get; set; }
            public string BillingFrequency { get; set; }
        }

        public string MicrosoftID { get; set; }

        public int IndirectResellerID { get; set; }

        public int ResellerID { get; set; }
        public string Billing { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalAmount { get; set; }
        public string BillingCylcle { get; set; }

        public List<CustomerSubscription> CustomerSubscriptions { get; set; }
    }


    public class RequestRelationshipDTO
    {
        public string EmailMessage { get; set; }
        public string EmailAddress { get; set; }

    }

    public class UpdateSubDTO
    {
        public string MicrosoftID { get; set; }
        public string SubscriptionID { get; set; }
        public int Quantity { get; set; }

        public string Status { get; set; }
        public string BillingCycle { get; set; }

    }
}