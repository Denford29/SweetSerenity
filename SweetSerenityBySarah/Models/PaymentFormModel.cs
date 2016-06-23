using System.ComponentModel.DataAnnotations;

namespace SweetSerenityBySarah.Models
    {
    public class PaymentFormModel
        {
        [Required(ErrorMessage = "Please enter the card holders name")]
        [Display(Name = "Cardholders Name")]
        public string CardholdersName { get; set; }

        [Required(ErrorMessage = "Please enter your card number")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Please enter your card expire date")]
        [Display(Name = "Card Expiry Date")]
        public string CardExpiryDate { get; set; }

        [Required(ErrorMessage = "Please enter your card's security code")]
        [Display(Name = "Card Security Code")]
        public string CardCcv { get; set; }

        public string StripeToken { get; set; }

        public string UserId { get; set; }
        }
    }