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
        /// <summary>
        /// The SMS message body is JSON with event, long and lat properties.
        /// </summary>
        public class SmsMessage
        {
            [JsonProperty(PropertyName = "event")]
            public string Event { get; set; }
            [JsonProperty(PropertyName = "long")]
            public string Longitude { get; set; }
            [JsonProperty(PropertyName = "lat")]
            public string Latitude { get; set; }
        }

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

            SmsMessage sms = 
                JsonConvert.DeserializeObject<SmsMessage>(body);

            // TODO: respond to different event types. For now, we're just
            // recreating the same JSON to pass on to Pusher
            var jsonRequest = Json(new
            {
                @event = sms.Event,
                longitude = sms.Longitude,
                latitude = sms.Latitude
            });

            var pusher = new PusherServer.Pusher(pusherAppId, pusherPublicKey,
                pusherSecretKey);

            // TODO: logging (and auto-responder?)
            var result = pusher.Trigger(pusherChannel, "sms_panic",
                jsonRequest.Data);
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
