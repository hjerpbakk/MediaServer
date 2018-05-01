using System.Threading.Tasks;
using CachePopulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace CachePopulator.Controllers {
    [Route("[controller]")]
    public class PopulateController : Controller {
        readonly FireAndForgetService fireAndForgetService;

        public PopulateController(FireAndForgetService fireAndForgetService) {
            this.fireAndForgetService = fireAndForgetService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Model.TalkMetadata talkMetadata) {
            await fireAndForgetService.TouchEndpoints(talkMetadata);
            return Ok();
        }
    }
}
