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
            //get the api keys to use from the site settings page
        var siteSetting = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "GlobalSettings");
        var secretAPIKey = PaymentService.GetSecretApiKey(siteSetting);
        var publishAPIKey = PaymentService.GetPublishApiKey(siteSetting);

            // create a new stripe customer with these default values
            var customerService = new StripeCustomerService(secretAPIKey);
            StripeCustomer stripeCustomer;

            return CurrentUmbracoPage();
            }

        }
    }