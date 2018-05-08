using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MediaServer
{
	public class Program
    {
        public static void Main(string[] args)
        {
			BuildWebHost(args).Build().Run();
        }

		public static IWebHostBuilder BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseApplicationInsights()
				.UseUrls("http://*:5000")
			    .UseStartup<Startup>();
    }
}
