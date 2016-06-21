using System.Configuration;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Stripe;

namespace SweetSerenityBySarah.Controllers
    {
    public class PaymentSurfaceController : SurfaceController
        {
        [HttpPost]
        public ActionResult ProcessPayment()
            {
            var stripeProcessingKey = ConfigurationManager.AppSettings["StripeSecretAPIKey"];
            // create a new stripe customer with these default values
            var customerService = new StripeCustomerService(stripeProcessingKey);
            StripeCustomer stripeCustomer;

            return CurrentUmbracoPage();
            }

        }
    }