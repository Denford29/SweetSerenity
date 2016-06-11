using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SweetSerenityBySarah.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace SweetSerenityBySarah.Controllers
    {
    public class AccountSurfaceController : SurfaceController
        {

        //public ActionResult Register()
        //{
        //return PartialView("Account/Login.");
        //}

        [HttpPost]
        public ActionResult RegisterSubmit(AccountRegisterModel registerModel)
            {
            if (!ModelState.IsValid)
                {
                TempData["registerError"] = "Opps... Registration Error, There was an error with your details please check them and try again.";
                //return RedirectToCurrentUmbracoPage();
                return CurrentUmbracoPage();
                }
            else
                {
                var membershipService = ApplicationContext.Services.MemberService;
                var siteMember = membershipService.GetByEmail(registerModel.Email.Trim());
                if (siteMember != null)
                    {
                    TempData["registerError"] = "Opps... Registration Error, Please use another email address";
                    //return RedirectToCurrentUmbracoPage();
                    return CurrentUmbracoPage();
                    }
                else
                    {
                    var member = membershipService.CreateMember(registerModel.Email.Trim(), registerModel.Email.Trim(),
                        registerModel.Name.Trim(), "Member");
                    if (member != null)
                        {
                        //Assign custom properties
                        member.SetValue("name", registerModel.Name.Trim());
                        member.SetValue("email", registerModel.Email.Trim());
                        member.IsApproved = false;
                        membershipService.Save(member);
                        membershipService.AssignRole(member.Id, "Site Users");
                        membershipService.SavePassword(member, registerModel.Password.Trim());

                        var mailBody = "Thank you for registering on RDM Online and a member of the team will get back to you once you account is activated .<br />Regards,<br />Website Team";
                        var userEmailMessage = new MailMessage { Subject = "Your Registration on RDM Online", Body = mailBody, From = new MailAddress("support@rdmonline.co.uk", "Web Team") };
                        userEmailMessage.To.Add(new MailAddress(registerModel.Email, registerModel.Name));
                        userEmailMessage.IsBodyHtml = true;
                        var userSmtpClient = new SmtpClient();
                        userSmtpClient.Send(userEmailMessage);

                        var adminMailBody = "The registration form has been submitted on the website with the details below. <br />";
                        adminMailBody += "Full Name : " + registerModel.Name.Trim() + "<br />";
                        adminMailBody += "Password : " + registerModel.Password.Trim() + "<br />";
                        adminMailBody += "Email Address : " + registerModel.Email.Trim() + "<br />";
                        adminMailBody += "Regards, <br />Website Team";

                        var adminEmail = new MailMessage { Subject = "The registration form submited on RDM Online", Body = adminMailBody, From = new MailAddress("support@rdmonline.co.uk", "Web Team") };
                        adminEmail.To.Add(new MailAddress("denfordmutseriwa@yahoo.com", "Denford"));
                        adminEmail.IsBodyHtml = true;
                        var adminSmtpClient = new SmtpClient();
                        adminSmtpClient.Send(adminEmail);

                        return Redirect("/");
                        //Response.Redirect("/");
                        }
                    TempData["registerError"] = "Opps... Registration Error, we cannot create your account at the moment, please try again later.";
                    //return RedirectToCurrentUmbracoPage();
                    return CurrentUmbracoPage();
                    }
                }
            }

        //public ActionResult Login()
        //{
        //return PartialView("Account/Login.");
        //}

        [HttpPost]
        public ActionResult LoginSubmit(AccountLoginModel loginModel)
            {
            var homepage = Umbraco.TypedContentAtRoot().FirstOrDefault(x => x.ContentType.Alias == "HomePage");
            if (!ModelState.IsValid)
                {
                TempData["LoginError"] = "Opps... There was an error with your details please check them and try again.";
                return CurrentUmbracoPage();
                }
            else
                {
                var isMemberValid = Membership.ValidateUser(loginModel.Email, loginModel.Password);
                var isLoginSuccess = Members.Login(loginModel.Email, loginModel.Password);
                if (isLoginSuccess && isMemberValid)
                    {
                    FormsAuthentication.SetAuthCookie(loginModel.Email, loginModel.RememberMe);
                    if (homepage != null && homepage.Id > 0)
                        {
                        var myWorldPage = homepage.Descendants("MyWorld").FirstOrDefault();
                        if (myWorldPage != null && myWorldPage.Id > 0)
                            {
                            return Redirect(myWorldPage.Url);
                            }
                        else
                            {
                            return Redirect(homepage.Url);
                            }
                        }
                    else
                        {
                        return Redirect("/");
                        }
                    }
                else
                    {
                    TempData["LoginError"] = "Opps... There was an error with your details please check them and try again.";
                    //return RedirectToCurrentUmbracoPage();
                    return CurrentUmbracoPage();
                    }
                }
            }

        [HttpPost]
        public ActionResult LogoutUser()
            {
            FormsAuthentication.SignOut();
            return Redirect("/");
            }

        }
    }