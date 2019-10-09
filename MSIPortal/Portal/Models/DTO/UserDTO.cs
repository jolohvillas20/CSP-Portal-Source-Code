using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class UserDTO
    {
        public int ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string Oldpassword { get; set; }
        public string Newpassword { get; set; }

        public Nullable<bool> Active { get; set; }
    }

    [MetadataType(typeof(UsersMetadata))]
    public partial class Users
    {
        // No field here
    }

    public class UsersMetadata
    {
        //[Required(ErrorMessage = "Microsoft ID is required.")]
        //[Display(Name = "Microsoft ID")]


        [Key]
        public int ID { get; set; }
        [Display(Name = "Username")]
        public string username { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
        public bool Active { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Usertype")]
        public Nullable<int> UserTypeID { get; set; }

    }
}