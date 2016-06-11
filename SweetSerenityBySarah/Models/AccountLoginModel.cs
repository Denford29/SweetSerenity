using System.ComponentModel.DataAnnotations;

namespace SweetSerenityBySarah.Models
    {
    public class AccountLoginModel
        {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Account Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        }
    }