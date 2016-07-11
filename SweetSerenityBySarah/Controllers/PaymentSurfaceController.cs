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
            return PartialView("Account/PaymentForm", model);
            }

        public ActionResult ProcessPaymentForm(PaymentFormModel paymentModel)
            {

            if (string.IsNullOrWhiteSpace(paymentModel.MemberEmailAddress) &&
                string.IsNullOrWhiteSpace(paymentModel.MemberId))
                {
                return CurrentUmbracoPage();
                }

            var currentMember = ApplicationContext.Services.MemberService.GetByUsername(paymentModel.MemberEmailAddress);

            if (currentMember == null)
                {
                return CurrentUmbracoPage();
                }

            var StripeCustomerId = currentMember.GetValue("StripeCustomerId").ToString();

            //get the api keys to use from the site settings page
            var siteSetting = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "GlobalSettings");
            var secretAPIKey = PaymentService.GetSecretApiKey(siteSetting);
            var publishAPIKey = PaymentService.GetPublishApiKey(siteSetting);

            // create a new stripe customer with these default values
            var customerService = new StripeCustomerService(secretAPIKey);
            StripeCustomer stripeCustomer;

            // add the token created earlier to be saved on our customer
            if (string.IsNullOrWhiteSpace(StripeCustomerId))
            {
            var chargedCustomer = new StripeCustomerCreateOptions
            {
                Email = currentMember.Email,
                Description = currentMember.GetValue("fullName")
                              + " - Payment for treatment.",
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
            stripeCustomer = customerService.Update(StripeCustomerId, chargedCustomer);
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
            var chargeService = new StripeChargeService(secretAPIKey);
            var stripeCharge = chargeService.Create(submitedCharge);

            return CurrentUmbracoPage();
            }

        }
    }