using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace MediaServer.Controllers {
	[Route("/Version")]
    public class VersionController : Controller {
        static bool debugging;

        static VersionController() => CheckIfDEBUG();

		readonly IFileProvider fileProvider;

		public VersionController(IFileProvider fileProvider)
		    => this.fileProvider = fileProvider;      

		[ResponseCache(NoStore = true)]
		[HttpGet]
		public async Task<string> Get() {
			var file = fileProvider.GetFileInfo("wwwroot/VERSION.txt");
			using (var readStream = file.CreateReadStream()) {
				using (var ms = new MemoryStream()) {
					await readStream.CopyToAsync(ms);
					var bytes = ms.ToArray();
					var version = Encoding.UTF8.GetString(bytes);
					return $"{version} {(debugging ? "DEBUG" : "RELEASE")}";
				}            
			}         
		} 

        [Conditional("DEBUG")]
        static void CheckIfDEBUG() => debugging = true;
    }
}
