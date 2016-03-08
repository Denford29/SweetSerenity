using System;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Web.Mvc;
using SweetSerenityBySarah.Models;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;

namespace SweetSerenityBySarah.Controllers
{
    public class ContactSurfaceController : SurfaceController
    {
        public ActionResult GetContactForm()
        {
            return PartialView("Contact/Form");
        }

        [HttpPost]
        public ActionResult ContactSubmit(ContactModel formContactModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["contactError"] =
                    "Opps... Form Error, There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
            }

            DateTime dateTime;
            var submittedDate = formContactModel.FormDate;
            if (DateTime.TryParseExact(submittedDate, "dd/mm/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                try
                {
                    const string mailBody =
                        "Thank you for contacting us on Sweet Serenity by Sarah, we have got your enquiry and a member of the team will get back to you as soon as posible.<br /> <br />Regards, <br /> Sarah";
                    var userEmailMessage = new MailMessage
                    {
                        Subject = "Your Contact on Sweet Serenity by Sarah",
                        Body = mailBody,
                        From = new MailAddress("support@sweetserenitybysarah.com", "Sarah")
                    };
                    userEmailMessage.To.Add(new MailAddress(formContactModel.EmailAddress.Trim(),
                        formContactModel.FullName.Trim()));
                    userEmailMessage.Bcc.Add("denfordmutseriwa@yahoo.com");
                    userEmailMessage.IsBodyHtml = true;
                    var userSmtpClient = new SmtpClient();
                    userSmtpClient.Send(userEmailMessage);

                    string adminMailBody =
                        "The contact form has been submitted on the website with the details below. <br /><br />";
                    adminMailBody += "Full Name : " + formContactModel.FullName.Trim() + "<br />";
                    adminMailBody += "Email Address : " + formContactModel.EmailAddress.Trim() + "<br />";
                    adminMailBody += "Phone Number : " + formContactModel.PhoneNumber.Trim() + "<br />";
                    adminMailBody += "Spam Date : " + formContactModel.FormDate.Trim() + "<br />";
                    adminMailBody += "Message : " + formContactModel.Message.Trim() + "<br />";
                    adminMailBody += "<br />Regards,<br />Sweet Serenity Team";

                    var adminEmaillMessage = new MailMessage
                    {
                        Subject = "The contact form has been submited on Sweet Serenity by Sarah",
                        Body = adminMailBody,
                        From = new MailAddress("support@sweetserenitybysarah.com", "Sweet Serenity Team")
                    };
                    adminEmaillMessage.To.Add(new MailAddress("sarahbelletty@yahoo.co.uk", "Sarah"));
                    adminEmaillMessage.Bcc.Add("denfordmutseriwa@yahoo.com");
                    adminEmaillMessage.IsBodyHtml = true;
                    var adminSmtpClient = new SmtpClient();
                    adminSmtpClient.Send(adminEmaillMessage);
                }
                catch (Exception ex)
                {
                var errorMessage = ex.Message + "<br /><br />" + ex.StackTrace + "<br /><br />" + ex.InnerException;
                    LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, errorMessage, ex);

                    var errorEmaillMessage = new MailMessage
                    {
                        Subject = "Website error on Sweet Serenity by Sarah",
                        Body = errorMessage,
                        From = new MailAddress("support@sweetserenitybysarah.com", "Sweet Serenity Team")
                    };
                    errorEmaillMessage.To.Add(new MailAddress("denfordmutseriwa@yahoo.com", "Denford"));
                    errorEmaillMessage.IsBodyHtml = true;
                    var errorSmtpClient = new SmtpClient();
                    errorSmtpClient.Send(errorEmaillMessage);

                    TempData["contactError"] =
                        "Opps... Contact Error, there was a problem submitting your request.";
                    return RedirectToCurrentUmbracoPage();
                }

                TempData["contactSuccess"] =
                    "Your contact request has been submitted successfully, a member of the team will get in touch with you shortly...";
                return CurrentUmbracoPage();
            }

            return Redirect("/");
        }

        //[HttpPost]
        //public ActionResult SweetSerenityContactSubmit(ContactModel formContactModel)
        //    {
        //    if (!ModelState.IsValid)
        //        {
        //        TempData["contactError"] =
        //            "Opps... Form Error, There was an error with your details please check them and try again.";
        //        return CurrentUmbracoPage();
        //        }
        //    else
        //        {
        //        DateTime dateTime;
        //        var submittedDate = formContactModel.FormDate;
        //        if (DateTime.TryParseExact(submittedDate, "dd/mm/yyyy", CultureInfo.InvariantCulture,
        //            DateTimeStyles.None, out dateTime))
        //            {
        //            try
        //                {
        //                const string mailBody =
        //                    "Thank you for contacting us on Sweet Serenity by Sarah, we have got your enquiry and a member of the team will get back to you as soon as posible.<br /> <br />Regards, <br /> Sarah";
        //                var userEmailMessage = new MailMessage
        //                {
        //                    Subject = "Your Contact on Sweet Serenity by Sarah",
        //                    Body = mailBody,
        //                    From = new MailAddress("support@rdmonline.co.uk", "RDM Online Team")
        //                    //From = new MailAddress("support@sweetserenitybysarah.com", "Sarah")
        //                };
        //                userEmailMessage.To.Add(new MailAddress(formContactModel.EmailAddress.Trim(),
        //                    formContactModel.FullName.Trim()));
        //                userEmailMessage.Bcc.Add("denfordmutseriwa@yahoo.com");
        //                userEmailMessage.IsBodyHtml = true;
        //                var userSmtpClient = new SmtpClient();
        //                userSmtpClient.Send(userEmailMessage);

        //                string adminMailBody =
        //                    "The contact form has been submitted on the website with the details below. <br /><br />";
        //                adminMailBody += "Full Name : " + formContactModel.FullName.Trim() + "<br />";
        //                adminMailBody += "Email Address : " + formContactModel.EmailAddress.Trim() + "<br />";
        //                adminMailBody += "Phone Number : " + formContactModel.PhoneNumber.Trim() + "<br />";
        //                adminMailBody += "Spam Date : " + formContactModel.FormDate.Trim() + "<br />";
        //                adminMailBody += "Message : " + formContactModel.Message.Trim() + "<br />";
        //                adminMailBody += "<br />Regards,<br />Sarah";

        //                var adminEmaillMessage = new MailMessage
        //                {
        //                    Subject = "The contact form has been submited on Sweet Serenity by Sarah",
        //                    Body = adminMailBody,
        //                    From = new MailAddress("support@rdmonline.co.uk", "RDM Online Team")
        //                    //From = new MailAddress("support@sweetserenitybysarah.com", "Sarah")
        //                };
        //                adminEmaillMessage.To.Add(new MailAddress("sarahbelletty@yahoo.co.uk", "Sarah"));
        //                adminEmaillMessage.Bcc.Add("denfordmutseriwa@yahoo.com");
        //                adminEmaillMessage.IsBodyHtml = true;
        //                var adminSmtpClient = new SmtpClient();
        //                adminSmtpClient.Send(adminEmaillMessage);
        //                }
        //            catch (Exception ex)
        //                {
        //                var errorMessage = ex.Message + "\r\n \r\n" + ex.StackTrace + "\r\n \r\n" + ex.InnerException;
        //                LogHelper.Error(MethodBase.GetCurrentMethod().DeclaringType, errorMessage, ex);
        //                TempData["contactError"] =
        //                    "Opps... Contact Error, there was a problem submitting your request.";
        //                return RedirectToCurrentUmbracoPage();
        //                }

        //            TempData["contactSuccess"] =
        //                "Your contact request has been submitted successfully, a member of the team will get in touch with you shortly...";
        //            return CurrentUmbracoPage();
        //            }
        //        else
        //            {
        //            return Redirect("/");
        //            }
        //        }
        //    }
    }
}