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

        public string StripeToken { get; set; }

        [Display(Name = "Select Member To Charge")]
        public string MemberId { get; set; }

        public List<IMember> SiteMembers { get; set; }

        public string PublishApiKey { get; set; }

        public string MemberEmailAddress { get; set; }

        public bool IsUserAdmin { get; set; }

        }
    }