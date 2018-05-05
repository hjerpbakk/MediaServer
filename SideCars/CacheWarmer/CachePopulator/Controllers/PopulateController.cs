using System.Threading.Tasks;
using CachePopulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace CachePopulator.Controllers {
    [Route("[controller]")]
    public class PopulateController : Controller {
		readonly ContinuousWarmupService continuousWarmupService;

		public PopulateController(ContinuousWarmupService continuousWarmupService) {
			this.continuousWarmupService = continuousWarmupService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Model.TalkMetadata talkMetadata) {
			await continuousWarmupService.TouchEndpoints(talkMetadata);
            return Ok();
        }
    }
}
