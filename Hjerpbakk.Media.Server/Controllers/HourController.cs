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
    [Route("/")]
    public class HourController : Controller
    {
        readonly CloudStorageClient cloudStorageClient;

        public HourController(CloudStorageClient cloudStorageClient)
        {
            this.cloudStorageClient = cloudStorageClient;
        }

        public async Task<IActionResult> Index()
        {
            // TODO: Fra config, vise utvalg av hvilke vi har. Eks devdays og interessanttimer
            // TODO: Cache this fucker, ref save
            ViewData["hoursOfInterest"] = await cloudStorageClient.GetHoursOfInterest(HttpContext.Request);

            return View();
        }

        [HttpGet("/hour/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            // TODO: Cache this fucker, ref save
            ViewData["hourOfInterest"] = await cloudStorageClient.Get(new HourOfInterestSummary { Id = Uri.EscapeUriString(id) });
            return View("hour");
        }
    }
}
