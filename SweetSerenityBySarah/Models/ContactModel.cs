using System.ComponentModel.DataAnnotations;

namespace SweetSerenityBySarah.Models
    {
    public class ContactModel
        {
        [Required(ErrorMessage = "Please enter your full name")]
        [Display(Name = "Your Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please check your email address, doesnt look valid")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Your Email Address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Please enter your phone number")]
        [Display(Name = "Your Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Please just pick any date, this is to prove you are not spam")]
        [Display(Name = "Select Any Date")]
        public string FormDate { get; set; }

        [Required(ErrorMessage = "Please enter your enquiry, provide as much details as posible")]
        [Display(Name = "Your Contact Message")]
        public string Message { get; set; }
        }
    }