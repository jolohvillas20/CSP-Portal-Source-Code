using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class SubscriptionVM
    {
        public string OfferID { get; set; }

        public string OfferName { get; set; }
        public int Quantity { get; set; }
        public int CustomerID { get; set; }
        public string UnitType { get; set; }
        public int Count { get; set; }
        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First name")]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]

        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Phone number must be numeric")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Agreement Date is required.")]
        [Display(Name = "Agreement Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        public string AgreementDate { get; set; }
        public string Status { get; set; }

    }


  
}