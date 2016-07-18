using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;
using Stripe;
using SweetSerenityBySarah.Models;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace SweetSerenityBySarah.Controllers
    {
    public class PaymentSurfaceController : SurfaceController
        {
        /// <summary>
        /// get the payment form
        /// </summary>
        /// <returns></returns>
        public ActionResult PaymentForm()
            {
            var membersList = new List<IMember>();
            var membershipService = ApplicationContext.Services.MemberService;
            var allMembersList = membershipService.GetAllMembers().ToList();
            var currentUser = Membership.GetUser();
            var userEmail = "";
            var userId = "";
            var adminUser = false;
            if (currentUser != null)
                {
                var username = currentUser.UserName;
                var currentMember = ApplicationContext.Services.MemberService.GetByUsername(username);
                if (currentMember != null)
                    {
                    userEmail = currentMember.Email;
                    userId = currentMember.Id.ToString();
                    List<string> adminGroupList = new List<string>();
                    adminGroupList.Add("Site Admin");
                    if (Members.IsMemberAuthorized(allowGroups: adminGroupList))
                        {
                        adminUser = true;
                        }
                    membersList.Add(currentMember);
                    membersList.AddRange(allMembersList.Where(member => member.IsApproved && member.Id != currentMember.Id).ToList());
                    }
                }

            var siteSetting = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "GlobalSettings");
            var setPublishApiKey = PaymentService.GetPublishApiKey(siteSetting);

            var model = new PaymentFormModel
            {
                SiteMembers = membersList,
                PaymentAmount = 1,
                PublishApiKey = setPublishApiKey,
                MemberId = userId,
                MemberEmailAddress = userEmail,
                IsUserAdmin = adminUser
            };
            // ReSharper disable once Mvc.PartialViewNotResolved
            return PartialView("Account/PaymentForm", model);
            }

        /// <summary>
        /// process the payment form
        /// </summary>
        /// <param name="paymentModel"></param>
        /// <returns></returns>
        public ActionResult ProcessPaymentForm(PaymentFormModel paymentModel)
            {

            if (string.IsNullOrWhiteSpace(paymentModel.StripeToken))
                {
                TempData["paymentError"] =
                    "Opps... Payment Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }

            if (string.IsNullOrWhiteSpace(paymentModel.MemberEmailAddress) &&
                string.IsNullOrWhiteSpace(paymentModel.MemberId))
                {
                TempData["paymentError"] =
                    "Opps... Payment Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }

            var currentMember = ApplicationContext.Services.MemberService.GetByUsername(paymentModel.MemberEmailAddress);

            if (currentMember == null)
                {
                TempData["paymentError"] =
                    "Opps... Payment Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }

            var stripeCustomerId = "";
            var memberFullName = "";
            var paymentDescription = "Payment for treatment";
            var typedMember = Umbraco.TypedMember(currentMember.Id);
            if (typedMember != null && typedMember.Id > 0)
                {
                if (typedMember.HasProperty("StripeCustomerId") && typedMember.HasValue("StripeCustomerId"))
                    {
                    stripeCustomerId = typedMember.GetPropertyValue<string>("StripeCustomerId");
                    }

                if (typedMember.HasProperty("fullName") && typedMember.HasValue("fullName"))
                    {
                    memberFullName = typedMember.GetPropertyValue<string>("fullName");
                    paymentDescription += " for : " + memberFullName;
                    }
                }


            //get the api keys to use from the site settings page
            var siteSetting = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "GlobalSettings");
            var secretApiKey = PaymentService.GetSecretApiKey(siteSetting);
            var publishApiKey = PaymentService.GetPublishApiKey(siteSetting);

            // create a new stripe customer with these default values
            var customerService = new StripeCustomerService(secretApiKey);
            StripeCustomer stripeCustomer;

            // add the token created earlier to be saved on our customer
            if (string.IsNullOrWhiteSpace(stripeCustomerId))
                {
                var chargedCustomer = new StripeCustomerCreateOptions
                {
                    Email = currentMember.Email,
                    Description = paymentDescription,
                    SourceToken = paymentModel.StripeToken
                };

                //send these details to stripe to create the customer
                stripeCustomer = customerService.Create(chargedCustomer);

                //Save the new members id on the member
                currentMember.SetValue("stripeCustomerID", stripeCustomer.Id);
                }
            else
                {
                var chargedCustomer = new StripeCustomerUpdateOptions { SourceToken = paymentModel.StripeToken };
                // update the customer with the token created earlier

                //send these details to stripe to create the customer
                stripeCustomer = customerService.Update(stripeCustomerId, chargedCustomer);
                }

            // set a charge on the account based on the amount calaculated bellow
            var submitedCharge = new StripeChargeCreateOptions();

            //set the amount to be charged and currency
            submitedCharge.Amount = Convert.ToInt32(paymentModel.PaymentAmount);
            submitedCharge.Currency = "AUD";

            // set the description and charge the customer created above
            submitedCharge.Description = "Treatment charges";
            submitedCharge.CustomerId = stripeCustomer.Id;

            //create the charge with the above details and charge the customer
            var chargeService = new StripeChargeService(secretApiKey);
            var stripeCharge = chargeService.Create(submitedCharge);

            TempData["paymentSuccess"] =
                    "The payment has been processed and we will email you the details.";

            return CurrentUmbracoPage();
            }

        }
    }