using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace SweetSerenityBySarah.Models
    {
    public class PaymentListModel
        {
        public PaymentListModel()
        {
            Payments = new List<PaymentItem>();
        }

        public IPublishedContent CurrentPage { get; set; }

        public List<PaymentItem> Payments { get; set; }

        public partial class PaymentItem
            {
            [Display(Name = "Payment Description")]
            public string PaymentDescription { get; set; }

            [Display(Name = "Payment Status")]
            public string StripeChargeStatus { get; set; }

            [Display(Name = "Payment Amount")]
            public decimal PaymentAmount { get; set; }

            [Display(Name = "Payment Date")]
            public DateTime StripeChargeDate { get; set; }
            }

        }
    }