using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace MediaServer.Controllers
{
	[Route("/Version")]
    public class VersionController : Controller
    {
        static bool debugging;

        static VersionController() => CheckIfDEBUG();

		// TODO: Fetch VERSION from version file...
        [HttpGet]
        public string Get() => $"{Assembly.GetExecutingAssembly().GetName().Version} {(debugging ? "DEBUG" : "RELEASE")}";

        [Conditional("DEBUG")]
        static void CheckIfDEBUG() => debugging = true;
    }
}
