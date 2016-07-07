using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace SweetSerenityBySarah
    {
    public class PaymentService : UmbracoHelper
        {

        // set the default values of the api key
        private static string SecretApiKey = "sk_test_XgqG8RG2MWDMsm6WcOuVZc5A";
        private static string PublishApiKey = "pk_test_7mFtUZzbxDespYuEjpQPYzzi";

        /// <summary>
        /// get the secret api key from site settings
        /// </summary>
        /// <returns></returns>
        public static string GetSecretApiKey(IPublishedContent siteSetting)
            {
            var secretApiKey = SecretApiKey;
            var liveMode = false;

            if (siteSetting != null && siteSetting.Id > 0 && siteSetting.Descendants("SiteDetails").Any())
                {
                var siteDetailsPage = siteSetting.Descendants("SiteDetails").FirstOrDefault();
                if (siteDetailsPage != null && siteDetailsPage.Id > 0)
                    {
                    if (siteDetailsPage.HasProperty("livePaymentmode") && siteDetailsPage.HasValue("livePaymentmode"))
                        {
                        liveMode = siteDetailsPage.GetPropertyValue<bool>("livePaymentmode");
                        }

                    if (liveMode)
                        {
                        if (siteDetailsPage.HasProperty("livesecretapikey") && siteDetailsPage.HasValue("livesecretapikey"))
                            {
                            secretApiKey = siteDetailsPage.GetPropertyValue("livesecretapikey").ToString();
                            }
                        }
                    else
                        {
                        if (siteDetailsPage.HasProperty("testsecretapikey") && siteDetailsPage.HasValue("testsecretapikey"))
                            {
                            secretApiKey = siteDetailsPage.GetPropertyValue("testsecretapikey").ToString();
                            }
                        }
                    }
                }

            return secretApiKey;
            }

        /// <summary>
        /// get the publish api key from site settings
        /// </summary>
        /// <returns></returns>
        public static string GetPublishApiKey(IPublishedContent siteSetting)
            {
            var publishApiKey = PublishApiKey;
            var liveMode = false;

            if (siteSetting != null && siteSetting.Id > 0 && siteSetting.Descendants("SiteDetails").Any())
                {
                var siteDetailsPage = siteSetting.Descendants("SiteDetails").FirstOrDefault();
                if (siteDetailsPage != null && siteDetailsPage.Id > 0)
                    {
                    if (siteDetailsPage.HasProperty("livePaymentmode") && siteDetailsPage.HasValue("livePaymentmode"))
                        {
                        liveMode = siteDetailsPage.GetPropertyValue<bool>("livePaymentmode");
                        }

                    if (liveMode)
                        {
                        if (siteDetailsPage.HasProperty("livepublishapikey") && siteDetailsPage.HasValue("livepublishapikey"))
                            {
                            publishApiKey = siteDetailsPage.GetPropertyValue("livepublishapikey").ToString();
                            }
                        }
                    else
                        {
                        if (siteDetailsPage.HasProperty("testpublishapikey") && siteDetailsPage.HasValue("testpublishapikey"))
                            {
                            publishApiKey = siteDetailsPage.GetPropertyValue("testpublishapikey").ToString();
                            }
                        }
                    }
                }

            return publishApiKey;
            }

        }
    }