﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.TwiML;

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
            var thing = Request.Form["From"];

            var stuff = "";
        }


    }
}
