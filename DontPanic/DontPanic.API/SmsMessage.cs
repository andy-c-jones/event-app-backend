using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PusherServer;

namespace DontPanic.API
{
    public interface ISmsMessage
    {
        /// <summary>
        /// The type of SMS message event, e.g. "panic"!
        /// </summary>
        string Event { get; set; }

        /// <summary>
        /// Push this message notification to the Pusher service.
        /// </summary>
        /// <returns></returns>
        PusherServer.ITriggerResult PusherPush(
            PusherServer.Pusher pusher, string pusherChannel);
    }

    /// <summary>
    /// The SMS panic message body is JSON with event, long and lat properties.
    /// </summary>
    public class SmsPanicMessage : ISmsMessage
    {
        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }

        [JsonProperty(PropertyName = "long")]
        public string Longitude { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public string Latitude { get; set; }

        public PusherServer.ITriggerResult PusherPush(
            PusherServer.Pusher pusher, string pusherChannel)
        {
            return pusher.Trigger(pusherChannel, "sms_panic", new
            {
                @event = Event,
                longitude = Longitude,
                latitude = Latitude
            });
        }
    }

    /// <summary>
    /// Registration SMS messages include basic person details.
    /// </summary>
    public class SmsRegistrationMessage : ISmsMessage
    {
        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        public ITriggerResult PusherPush(Pusher pusher, string pusherChannel)
        {
            return pusher.Trigger(pusherChannel, "register", new
            {
                @event = Event,
                firstName = FirstName,
                lastName = LastName
            });
        }
    }

    /// <summary>
    /// Convert JSON into a specific type of SmsMessage object.
    /// </summary>
    class SmsMessageConverter : Newtonsoft.Json.Converters.CustomCreationConverter<ISmsMessage>
    {
        public override ISmsMessage Create(Type objectType)
        {
            throw new NotImplementedException();
        }

        public ISmsMessage Create(Type objectType, JObject jObject)
        {
            var eventType = (string)jObject.Property("event");

            switch (eventType)
            {
                case "panic":
                    return new SmsPanicMessage();

                case "register":
                    return new SmsRegistrationMessage();
            }

            throw new ApplicationException(String.Format(
                "Unsupported message type {0}.", eventType));
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            //return base.ReadJson(reader, objectType, existingValue, serializer);

            // Load JObject from stream 
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject 
            var target = Create(objectType, jObject);

            // Populate the object properties 
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }
}