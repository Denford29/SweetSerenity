using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace SweetSerenityBySarah.Models
    {
    /// <summary>
    /// payment list model
    /// </summary>
    public class PaymentListModel
        {

        /// <summary>
        /// initiate the defaults of the model
        /// </summary>
        public PaymentListModel()
        {
            Payments = new List<PaymentItem>();
        }

        /// <summary>
        /// get or set the current umbraco page
        /// </summary>
        public IPublishedContent CurrentPage { get; set; }

        /// <summary>
        /// create a list of payments
        /// </summary>
        public List<PaymentItem> Payments { get; set; }

        /// <summary>
        /// get or set the maximum page items
        /// </summary>
        public int MaximumPageItems { get; set; }

        /// <summary>
        /// get or set partial class for a payment item
        /// </summary>
        public partial class PaymentItem
            {

            /// <summary>
            /// get or set the payment description
            /// </summary>
            [Display(Name = "Payment Description")]
            public string PaymentDescription { get; set; }

            /// <summary>
            /// get or set the stripe charge status
            /// </summary>
            [Display(Name = "Payment Status")]
            public string StripeChargeStatus { get; set; }

            /// <summary>
            /// get or set the payment amount
            /// </summary>
            [Display(Name = "Payment Amount")]
            public decimal PaymentAmount { get; set; }

            /// <summary>
            /// get or set the payment date
            /// </summary>
            [Display(Name = "Payment Date")]
            public DateTime StripeChargeDate { get; set; }
            }

        }
    }