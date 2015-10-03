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
        // Pusher service settings
        static readonly string PusherAppId;
        static readonly string PusherPublicKey;
        static readonly string PusherSecretKey;
        static readonly string PusherChannel;

        static HomeController()
        {
            PusherAppId = ConfigurationManager.AppSettings["PUSHER_APP_ID"];
            PusherPublicKey = ConfigurationManager.AppSettings[
                "PUSHER_PUBLIC_KEY"];
            // Note: secret key is either local to this machine, or loaded from
            // Azure's environment vars.
            PusherSecretKey = Environment.GetEnvironmentVariable(
                "PUSHER_SECRET");
            if (PusherSecretKey == null)
            {
                PusherSecretKey =
                    Environment.GetEnvironmentVariable("PUSHER_SECRET",
                    EnvironmentVariableTarget.User);
            }
            PusherChannel = ConfigurationManager.AppSettings["PUSHER_CHANNEL"];
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        /// <summary>
        /// Process an SMS message request from the Twilio service.
        /// </summary>
        /// <param name="response">Currently unused.</param>
        [HttpPost]
        public void IncomingTwilio(TwilioResponse response)
        {
            // We're encoding the request parameters in the body of the message
            // as JSON
            if (Request.Form["Body"] != null)
            {
                // NB: 'From' corresponds to the SMS sender's phone number
                PushEvent(Request.Form["Body"], Request.Form["From"]);
            }
        }

        /// <summary>
        /// Take Twilio message parameters and push a notification event to a
        /// Pusher server.
        /// </summary>
        /// <param name="body">The body of the SMS message. A JSON-encoded 
        /// <see cref="SmsMessage">SmsMessage</see>.</param>
        /// <param name="fromPhone">Phone number of the caller who sent the 
        /// SMS.</param>
        private void PushEvent(string body, string fromPhone)
        {
            var pusher = new PusherServer.Pusher(PusherAppId, PusherPublicKey,
                PusherSecretKey);

            ISmsMessage sms = JsonConvert.DeserializeObject<ISmsMessage>(body, 
                new SmsMessageConverter());

            // An SMS message knows how to push itself...
            var pushResponse = sms.PusherPush(pusher, fromPhone, PusherChannel);
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

        public void CheckRegister()
        {
            string body = "{\"event\": \"register\", \"name\": \"Sarah Jones\", \"nok\": \"Bob Jones\", \"nokphone\": \"+441234567\" }";
            PushEvent(body, "+44234567");
        }
    }
}
