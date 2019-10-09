using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class UtilsDTO
    {
        public int ID { get; set; }
        public string Terms { get; set; }
     
    }
    [MetadataType(typeof(EmailTemplatesMetadata))]
    public partial class EmailTemplates
    {
        // No field here
    }

    public class EmailTemplatesMetadata
    {
        public int ID { get; set; }
        [Display(Name = "Email Template")]
        public string EmailType { get; set; }
        //[Required(ErrorMessage = "Email Content is required.")]
        [Display(Name = "Body")]
        public string EmailContent { get; set; }
        [Display(Name = "Subject")]
        public string EmailSubject { get; set; }
        [Display(Name = "Email Variables")]
        public string EmailVariables { get; set; }

        [Display(Name = "Order Details Content")]
        public string DetailsContent { get; set; }
        [Display(Name = "Order Details Variables")]
        public string DetailsVariables { get; set; }

    }
       
}