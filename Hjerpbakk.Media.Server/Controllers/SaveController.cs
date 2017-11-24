using System;
using System.Threading.Tasks;
using Hjerpbakk.Media.Server.Clients;
using Hjerpbakk.Media.Server.Model;
using Hjerpbakk.Media.Server.Slack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hjerpbakk.Media.Server.Controllers
{
    [Route("/[controller]")]
    public class SaveController : Controller
    {
        readonly SlackIntegration slackIntegration;
        readonly CloudStorageClient cloudStorageClient;

        public SaveController(SlackIntegration slackIntegration, CloudStorageClient cloudStorageClient)
        {
            this.slackIntegration = slackIntegration;
            this.cloudStorageClient = cloudStorageClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var availableVideos = await cloudStorageClient.GetAvailableNewVideos();
            ViewBag.VideoList = new SelectList(availableVideos, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind("Title,Description,Author,Id")] HourOfInterest hourOfInterest)
        {
            if (string.IsNullOrEmpty(hourOfInterest.Title) || string.IsNullOrEmpty(hourOfInterest.Description) || string.IsNullOrEmpty(hourOfInterest.Author) || string.IsNullOrEmpty(hourOfInterest.Id)) {
                // TODO: Clientside verification together with enabling of button
                // TODO: Error page...
                throw new Exception("Damned user...");
            }

            hourOfInterest.URL = hourOfInterest.GetURL(HttpContext.Request);
            hourOfInterest.VideoURL = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + "/videos/" + hourOfInterest.Id + Video.SupportedFileType;
            hourOfInterest.TimeStamp = DateTime.UtcNow;

            await cloudStorageClient.Save(hourOfInterest);

            await slackIntegration.PostInterestingHourToChannel(hourOfInterest);

            return new RedirectResult(hourOfInterest.URL, false, false);
        }

    }
}
