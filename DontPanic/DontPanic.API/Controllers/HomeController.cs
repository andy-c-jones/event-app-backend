using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.TwiML;
using System.Collections.Specialized;

namespace DontPanic.API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public void IncomingTwilio(TwilioResponse response)
        {
            if (Request.Form["Body"] != null)
            {
                PushEvent(Request.Form["Body"], Request.Form["From"]);
            }
        }

        /// <summary>
        /// Take Twilio message parameters and push notification event to 
        /// Pusher server.
        /// </summary>
        /// <param name="body">The body of the SMS message. A JSON-encoded 
        /// <see cref="SmsMessage">SmsMessage</see>.</param>
        /// <param name="fromPhone">Phone number of the caller who sent the 
        /// SMS.</param>
        private void PushEvent(string body, string fromPhone)
        {
            string pusherAppId =
                ConfigurationManager.AppSettings["PUSHER_APP_ID"];
            string pusherPublicKey =
                ConfigurationManager.AppSettings["PUSHER_PUBLIC_KEY"];
            // Note: secret key is either local to this machine, or loaded from
            // Azure's environment vars.
            string pusherSecretKey = 
                Environment.GetEnvironmentVariable("PUSHER_SECRET");
            if (pusherSecretKey == null)
            {
                pusherSecretKey =
                    Environment.GetEnvironmentVariable("PUSHER_SECRET",
                    EnvironmentVariableTarget.User);
            }
            string pusherChannel =
                ConfigurationManager.AppSettings["PUSHER_CHANNEL"];

            var pusher = new PusherServer.Pusher(pusherAppId, pusherPublicKey,
                pusherSecretKey);

            ISmsMessage sms = JsonConvert.DeserializeObject<ISmsMessage>(body, 
                new SmsMessageConverter());

            // An SMS message knows how to push itself...
            var pushResponse = sms.PusherPush(pusher, pusherChannel);
        }

        /// <summary>
        /// Quick check for PushEvent().
        /// </summary>
        public void Check()
        {
            // Garrison Lane Park, Birmingham
            string body = "{\"event\": \"panic\", \"long\": \"-1.874356\", \"lat\": \"52.479793\"}";
            PushEvent(body, "+44123456");
        }
    }
}
