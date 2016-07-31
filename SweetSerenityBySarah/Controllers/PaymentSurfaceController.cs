using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Web.Mvc;
using Stripe;
using SweetSerenityBySarah.Models;
using Umbraco.Core.Logging;
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
            // check if we have the stripe token submitted
            if (string.IsNullOrWhiteSpace(paymentModel.StripeToken))
                {
                TempData["paymentError"] =
                    "Opps... Payment Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }
            // check if we have the user email address or id to use to get the account to charge
            if (string.IsNullOrWhiteSpace(paymentModel.MemberEmailAddress) &&
                string.IsNullOrWhiteSpace(paymentModel.MemberId))
                {
                TempData["paymentError"] =
                    "Opps... Payment Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }
            // try and get the member from the site members
            var currentMember = ApplicationContext.Services.MemberService.GetByUsername(paymentModel.MemberEmailAddress);
            if (currentMember == null)
                {
                TempData["paymentError"] =
                    "Opps... Payment Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }

            try
                {
                // get the user details to use for stripe
                var membershipService = ApplicationContext.Services.MemberService;
                var stripeCustomerId = "";
                var memberFullName = paymentModel.MemberEmailAddress;
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
                PaymentService.GetPublishApiKey(siteSetting);

                // create a new stripe customer with these default values
                var customerService = new StripeCustomerService(secretApiKey);
                StripeCustomer stripeCustomer;

                // add the token created earlier to be saved on our new customer
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
                    membershipService.Save(currentMember);
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

                //set the amount in cents to be charged and currency
                submitedCharge.Amount = Convert.ToInt32(paymentModel.PaymentAmount * 100);
                submitedCharge.Currency = "AUD";

                // set the description and charge the customer created above
                submitedCharge.Description = paymentDescription;
                submitedCharge.CustomerId = stripeCustomer.Id;

                //create the charge with the above details and charge the customer
                var chargeService = new StripeChargeService(secretApiKey);
                var stripeCharge = chargeService.Create(submitedCharge);

                //check if we have a failure code back from the created charge
                if (!string.IsNullOrWhiteSpace(stripeCharge.FailureCode))
                    {
                    TempData["paymentError"] = "Opps... Payment Error, your payment failed due to error :" +
                                               stripeCharge.FailureMessage;
                    return CurrentUmbracoPage();
                    }

                //create the customer email to send
                var amountPaid = stripeCharge.Amount / 100;
                string mailBody = "Thank you for your payment us on Sweet Serenity by Sarah, your payment has been processed with the details below: <br />";
                mailBody += "<br />Payment Amount : $" + amountPaid.ToString();
                mailBody += "<br />Payment Description : $" + stripeCharge.Description.Trim();
                mailBody += "<br />Payment email address : $" + stripeCharge.ReceiptEmail.Trim();
                mailBody += "<br /> <br />Regards, <br /> Sarah";
                var userEmailMessage = new MailMessage
                {
                    Subject = "Your payment on Sweet Serenity by Sarah",
                    Body = mailBody,
                    From = new MailAddress("admin@sweetserenitybysarah.com.au", "Sarah")
                };
                userEmailMessage.To.Add(new MailAddress(stripeCharge.ReceiptEmail.Trim(), memberFullName.Trim()));
                userEmailMessage.Bcc.Add("denfordmutseriwa@yahoo.com");
                userEmailMessage.IsBodyHtml = true;
                var userSmtpClient = new SmtpClient();
                userSmtpClient.Send(userEmailMessage);

                //create the admin email to send
                string adminMailBody =
                    "A payment has been submitted on the website with the details below. <br /><br />";
                adminMailBody += "Payment Amount : $" + amountPaid.ToString() + "<br />";
                adminMailBody += "Payment Description : " + stripeCharge.Description.Trim() + "<br />";
                adminMailBody += "Payment Email Address : " + stripeCharge.ReceiptEmail.Trim() + "<br />";
                adminMailBody += "Stripe Charge Id : " + stripeCharge.Id.Trim() + "<br />";
                adminMailBody += "Stripe Charge Customer Id : " + stripeCharge.CustomerId.Trim() + "<br />";
                adminMailBody += "Stripe Charge captured : " + stripeCharge.Captured.ToString() + "<br />";
                adminMailBody += "Stripe Charge Status : " + stripeCharge.Status.Trim() + "<br />";
                adminMailBody += "<br />Regards,<br />Sweet Serenity Team";

                var adminEmaillMessage = new MailMessage
                {
                    Subject = "A payment has been submited on Sweet Serenity by Sarah",
                    Body = adminMailBody,
                    From = new MailAddress("admin@sweetserenitybysarah.com.au", "Sweet Serenity Team")
                };
                adminEmaillMessage.To.Add(new MailAddress("sarahbelletty@yahoo.co.uk", "Sarah"));
                adminEmaillMessage.Bcc.Add("denfordmutseriwa@yahoo.com");
                adminEmaillMessage.IsBodyHtml = true;
                var adminSmtpClient = new SmtpClient();
                adminSmtpClient.Send(adminEmaillMessage);

                // save the payment details in umbraco
                var paymentDate = DateTime.UtcNow;
                var paymentDetails = "New payment made : " + paymentDate + " , ";
                paymentDetails += "Payment Amount : $" + amountPaid.ToString() + " , ";
                paymentDetails += "Payment Description : " + stripeCharge.Description.Trim() + " , ";
                paymentDetails += "Payment email address : " + stripeCharge.ReceiptEmail.Trim() + " , ";
                paymentDetails += "Stripe Charge Id : " + stripeCharge.Id.Trim() + " , ";
                paymentDetails += "Stripe Charge Customer Id : " + stripeCharge.CustomerId.Trim() + " , ";
                paymentDetails += "Stripe Charge captured : " + stripeCharge.Captured.ToString() + " , ";
                paymentDetails += "Stripe Charge Status : " + stripeCharge.Status.Trim() + " , ";
                var memberComments = currentMember.Comments;
                memberComments = memberComments + Environment.NewLine + Environment.NewLine + paymentDetails;
                currentMember.Comments = memberComments;
                membershipService.Save(currentMember);

                // save the payment in umbraco on the payment details page
                if (siteSetting != null && siteSetting.Id > 0 && siteSetting.Descendants("PaymentsContainer").Any())
                    {
                    var paymentsContainerPage = siteSetting.Descendants("PaymentsContainer").FirstOrDefault();
                    if (paymentsContainerPage != null && paymentsContainerPage.Id > 0)
                        {
                        //page name
                        var pageName = memberFullName + " - " + stripeCharge.Id.Trim();
                        var contentService = ApplicationContext.Services.ContentService;
                        var paymentDetailsPage = contentService.CreateContent(
                            pageName,
                            paymentsContainerPage.Id,
                            "PaymentDetails");

                        // set the properties of the new document
                        paymentDetailsPage.SetValue("chargedMember", currentMember.Id);
                        paymentDetailsPage.SetValue("paymentAmount", amountPaid);
                        paymentDetailsPage.SetValue("paymentDescription", stripeCharge.Description.Trim());
                        paymentDetailsPage.SetValue("paymentEmailAddress", stripeCharge.ReceiptEmail.Trim());
                        paymentDetailsPage.SetValue("stripeChargeId", stripeCharge.Id.Trim());
                        paymentDetailsPage.SetValue("stripeChargeCustomerId", stripeCharge.CustomerId.Trim());
                        paymentDetailsPage.SetValue("stripeChargeCaptured", stripeCharge.Captured);
                        paymentDetailsPage.SetValue("stripeChargeStatus", stripeCharge.Status.Trim());

                        //save and publish the document
                        var publishResult = contentService.SaveAndPublishWithStatus(paymentDetailsPage);
                        if (!publishResult.Success)
                            {
                            var errorMessage = publishResult.Exception.Message + "<br /><br />" + publishResult.Exception.StackTrace + "<br /><br />" + publishResult.Exception.InnerException;
                            LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, errorMessage, publishResult.Exception);

                            var errorEmaillMessage = new MailMessage
                            {
                                Subject = "Stripe payment details publish error on Sweet Serenity by Sarah",
                                Body = errorMessage,
                                From = new MailAddress("admin@sweetserenitybysarah.com.au", "Sweet Serenity Team")
                            };
                            errorEmaillMessage.To.Add(new MailAddress("denfordmutseriwa@yahoo.com", "Denford"));
                            errorEmaillMessage.IsBodyHtml = true;
                            var errorSmtpClient = new SmtpClient();
                            errorSmtpClient.Send(errorEmaillMessage);
                            }
                        }
                    }

                }
            // if we have any error in the process log it and send an email with the details
            catch (Exception ex)
                {
                var errorMessage = ex.Message + "<br /><br />" + ex.StackTrace + "<br /><br />" + ex.InnerException;
                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, errorMessage, ex);

                var errorEmaillMessage = new MailMessage
                {
                    Subject = "Stripe payment error on Sweet Serenity by Sarah",
                    Body = errorMessage,
                    From = new MailAddress("admin@sweetserenitybysarah.com.au", "Sweet Serenity Team")
                };
                errorEmaillMessage.To.Add(new MailAddress("denfordmutseriwa@yahoo.com", "Denford"));
                errorEmaillMessage.IsBodyHtml = true;
                var errorSmtpClient = new SmtpClient();
                errorSmtpClient.Send(errorEmaillMessage);

                TempData["paymentError"] =
                    "Opps... Payment Error, there was a problem submitting your payment, we have been informed of the error and we will look into it asap.";
                return CurrentUmbracoPage();
                }

            // if we get here everything has gone through fine
            TempData["paymentSuccess"] = "The payment has been processed, please check your emails for a confirmation email we sent.";
            return CurrentUmbracoPage();
            }

        /// <summary>
        /// get the list of payments from umbraco to display
        /// </summary>
        /// <returns></returns>
        public ActionResult PaymentsList()
            {
            //get the home page
            var siteSetting = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "GlobalSettings");
            var maximumPages = PaymentService.GetMaximumPageNumber(siteSetting);
            var homePage = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "HomePage");
            var model = new PaymentListModel
            {
                CurrentPage = UmbracoContext.Current.PublishedContentRequest.PublishedContent,
                MaximumPageItems = maximumPages
            };

            //get the current user and check if we have one
            var currentUser = Membership.GetUser();
            if (currentUser != null)
                {
                var username = currentUser.UserName;
                var currentMember = ApplicationContext.Services.MemberService.GetByUsername(username);
                if (currentMember != null)
                    {
                    //check if the current user is a member of the admin group
                    var adminUser = false;
                    List<string> adminGroupList = new List<string>();
                    adminGroupList.Add("Site Admin");
                    if (Members.IsMemberAuthorized(allowGroups: adminGroupList))
                        {
                        adminUser = true;
                        }
                    //get all the payments documents to add to our list
                    if (siteSetting != null && siteSetting.Id > 0 && siteSetting.Descendants("PaymentDetails").Any())
                        {
                        var paymentItemPages = siteSetting.Descendants("PaymentDetails").ToList();
                            if (!adminUser)
                            {
                            paymentItemPages = paymentItemPages.Where(itemPage => itemPage.HasProperty("chargedMember") &&
                                                                   itemPage.HasValue("chargedMember") &&
                                                                   itemPage.GetPropertyValue<int>("chargedMember") ==
                                                                   currentMember.Id)
                                    .ToList();
                            }
                        if (paymentItemPages.Any())
                            {
                            //get the properties from the payment page into the payment list item
                            foreach (var paymentItem in paymentItemPages)
                                {
                                var paymentListItem = new PaymentListModel.PaymentItem();

                                if (paymentItem.HasProperty("paymentDescription") && paymentItem.HasValue("paymentDescription"))
                                    {
                                    paymentListItem.PaymentDescription = paymentItem.GetPropertyValue<string>("paymentDescription");
                                    }
                                if (paymentItem.HasProperty("stripeChargeStatus") && paymentItem.HasValue("stripeChargeStatus"))
                                    {
                                    paymentListItem.StripeChargeStatus = paymentItem.GetPropertyValue<string>("stripeChargeStatus");
                                    }
                                if (paymentItem.HasProperty("paymentAmount") && paymentItem.HasValue("paymentAmount"))
                                    {
                                    paymentListItem.PaymentAmount = Convert.ToDecimal(paymentItem.GetPropertyValue<int>("paymentAmount"));
                                    }
                                paymentListItem.StripeChargeDate = paymentItem.CreateDate;
                                // add the payment list item model
                                model.Payments.Add(paymentListItem);

                                }
                            }
                        }
                    }
                }
            else
                {
                return RedirectToUmbracoPage(homePage);
                }

            // ReSharper disable once Mvc.PartialViewNotResolved
            return PartialView("Account/PaymentsList", model);
            }
        }
    }