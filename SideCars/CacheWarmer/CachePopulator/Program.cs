using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace CachePopulator {
	public class Program {
        public static void Main(string[] args) {
			BuildWebHost(args).Build().Run();
        }

		public static IWebHostBuilder BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				   .UseUrls("http://*:1337")
				   .UseStartup<Startup>();
    }
}
