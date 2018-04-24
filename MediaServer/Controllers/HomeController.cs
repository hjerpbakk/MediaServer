using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Configuration;

namespace MediaServer.Controllers
{
    public class HomeController : Controller
    {
        readonly IConferenceConfig conferenceConfig;
        readonly ITalkService talkService;

        public HomeController(IConferenceConfig conferenceConfig, ITalkService talkService) {
            this.conferenceConfig = conferenceConfig;
            this.talkService = talkService;
        }

        public async Task<IActionResult> Index() {
            // TODO: Ta inn konferansene programmatisk fra config i _Layout.cshtml
            ViewData["Title"] = "Latest Talks";

            var talks = (await talkService.GetLatestTalks(conferenceConfig.Conferences.Values))
                .Select(latestTalk => new TalkSummaryViewModel(latestTalk.Conference, latestTalk.Talk, HttpContext));
            ViewData["Talks"] = talks;

            // TODO: Show conference link on card
            // TODO: Use model binding
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
