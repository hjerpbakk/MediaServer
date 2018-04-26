using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Services;
using MediaServer.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MediaServer.Controllers
{
	public class SpeakerController : NavigateableController
	{
		readonly IConferenceService conferenceService;
		readonly IMemoryCache memoryCache;

		public SpeakerController(IConferenceConfig conferenceConfig, IConferenceService conferenceService, IMemoryCache memoryCache)
			: base(conferenceConfig)
		{
			this.conferenceService = conferenceService;
			this.memoryCache = memoryCache;
		}

		// TODO: Add a top speaker list
		[ResponseCache(NoStore = true)]
		[HttpGet("/[controller]/{speakerName}")]
		public async Task<IActionResult> Index(string speakerName)
		{
			var key = Keys.GetSpeakerKey(speakerName);
			if (!memoryCache.TryGetValue(key, out ViewResult view))
			{
				SetCurrentNavigation(speakerName);
				ViewData["Talks"] = await conferenceService.GetTalksBySpeaker(speakerName, HttpContext);
				view = View("Views/Home/Index.cshtml");
				memoryCache.Set(key, view, Keys.Options);
			}

			return view;
		}
    }
}
