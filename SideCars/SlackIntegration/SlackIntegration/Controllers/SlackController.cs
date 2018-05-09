using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SlackIntegration.Configuration;
using SlackIntegration.Models;
using SlackIntegration.Services;

namespace SlackIntegration.Controllers {
	[Route("[controller]")]
    [ApiController]
    public class SlackController : ControllerBase {
		readonly Dictionary<string, Conference> conferences;
		readonly SlackService slackService;

		public SlackController(ConferenceConfig conferenceConfig, SlackService slackService) {
			conferences = conferenceConfig.Conferences;
			this.slackService = slackService;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] Talk talk) {
			var conference = conferences[talk.ConferenceId];
			await slackService.PostTalkToChannel(conference, talk);
            return Ok();
        }

		[HttpGet]
		public async Task<User[]> ListUsers() => await slackService.ListUsers();

		[HttpGet("Links")]
		public SlackLink GetMetaData() => slackService.GetMetaData();      
    }
}
