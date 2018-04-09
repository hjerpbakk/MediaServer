using System;
using System.Net.Http;
using System.Threading.Tasks;
using Hjerpbakk.Media.Server.Clients;
using Hjerpbakk.Media.Server.Configuration;
using Hjerpbakk.Media.Server.Model;
using Hjerpbakk.Media.Server.Slack;
using Microsoft.AspNetCore.Mvc;

namespace Hjerpbakk.Media.Server.Controllers
{
    [Route("/[controller]")]
    public class HourController : Controller
    {
        readonly CloudStorageClient cloudStorageClient;
        readonly IConferenceConfiguration conferenceConfiguration;

        // TODO: Rename this and other to Talk
        public HourController(CloudStorageClient cloudStorageClient, IConferenceConfiguration conferenceConfiguration)
        {
            this.cloudStorageClient = cloudStorageClient;
            this.conferenceConfiguration = conferenceConfiguration;
        }

        [HttpGet("/hour/{conferenceName}")]
        public async Task<IActionResult> Index(string conferenceName)
        {
            // TODO: Hente basert på hvilken conference dette er
            // TODO: Cache this fucker
            var conference = conferenceConfiguration.GetConference(conferenceName);
            ViewData["hoursOfInterest"] = await cloudStorageClient.GetTalks(HttpContext.Request, conference);
            ViewData["conferenceName"] = conferenceName;
            return View();
        }

        [HttpGet("/hour/{conference}/{id}")]
        public async Task<IActionResult> Index(string conference, string id)
        {
            // TODO: Cache this fucker
            ViewData["hourOfInterest"] = await cloudStorageClient.Get(new TalkSummary { Id = Uri.EscapeUriString(id) });
            return View("hour");
        }
    }
}
