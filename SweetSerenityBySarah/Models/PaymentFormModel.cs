using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using umbraco.presentation.webservices;
using Umbraco.Core.Models;

namespace SweetSerenityBySarah.Models
    {
    public class PaymentFormModel
        {
        [Required(ErrorMessage = "Please enter the amount to pay.")]
        [Display(Name = "Payment Amount")]
        public decimal PaymentAmount { get; set; }

        //[Required(ErrorMessage = "Please enter the card holders name")]
        //[Display(Name = "Cardholders Name")]
        //public string CardholdersName { get; set; }

        //[Required(ErrorMessage = "Please enter your card number")]
        //[Display(Name = "Card Number")]
        //public string CardNumber { get; set; }

        //[Required(ErrorMessage = "Please enter your card expire date")]
        //[Display(Name = "Card Expiry Date")]
        //public string CardExpiryDate { get; set; }

        //[Required(ErrorMessage = "Please enter your card's security code")]
        //[Display(Name = "Card Security Code")]
        //public string CardCcv { get; set; }

        public string StripeToken { get; set; }

        [Display(Name = "Select Member To Charge")]
        public string MemberId { get; set; }

        public List<IMember> SiteMembers { get; set; }

        public string PublishApiKey { get; set; }

        public string MemberEmailAddress { get; set; }

        public bool IsUserAdmin { get; set; }

        }
    }