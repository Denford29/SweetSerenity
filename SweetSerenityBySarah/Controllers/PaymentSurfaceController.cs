using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Stripe;
using SweetSerenityBySarah.Models;
using Umbraco.Web;

namespace SweetSerenityBySarah.Controllers
    {
    public class PaymentSurfaceController : SurfaceController
        {

        public ActionResult PaymentForm()
            {
            var model = new PaymentFormModel();
            model.PaymentAmount = 1;
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