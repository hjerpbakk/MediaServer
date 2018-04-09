using Hjerpbakk.Media.Server.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Hjerpbakk.Media.Server.Controllers
{
    [Route("/")]
    public class ConferenceController : Controller
    {
        readonly IConferenceConfiguration conferenceConfiguration;

        public ConferenceController(IConferenceConfiguration conferenceConfiguration)
        {
            this.conferenceConfiguration = conferenceConfiguration;
        }

        public IActionResult Index()
        {
            ViewData["conferences"] = conferenceConfiguration.Conferences;
            return View();
        }
    }
}
