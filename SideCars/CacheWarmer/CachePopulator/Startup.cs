using System.Net.Http;
using CachePopulator.Clients;
using CachePopulator.Configuration;
using CachePopulator.Services;
using CachePopulator.InitialWarmup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CachePopulator
{
    public class Startup {
		public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables()
                .AddJsonFile($"config.json", true);
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			var httpClient = new HttpClient();
			services.AddSingleton(httpClient);
			var careFreeHttpClient = new CareFreeHttpClient(httpClient);
			services.AddSingleton(careFreeHttpClient);

			// TODO: From config. And theres more in blob storage services.
			var baseUrl = "http://media-server:5000";
			var mediaServerConfig = new MediaServerConfig(baseUrl, baseUrl + "/Conference", baseUrl + "/Speaker");
			services.AddSingleton<MediaServerConfig>();
			WarmUpCache(careFreeHttpClient, mediaServerConfig);

			services.AddSingleton<ContinuousWarmupService>();
		}
        
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

		void WarmUpCache(CareFreeHttpClient careFreeHttpClient, MediaServerConfig mediaServerConfig)
        {
            var config = Configuration.Get<AppConfig>();
            var conferenceMetaDataService = new ConferenceMetaDataService(config);
            var conferences = conferenceMetaDataService.CreateConferenceConfig().GetAwaiter().GetResult();
            var speakerMetadataService = new SpeakerMetadataService(config, conferences);
            var speakers = speakerMetadataService.GetAllSpeakers().GetAwaiter().GetResult();
			var initialWarmupService = new InitialWarmupService(mediaServerConfig, careFreeHttpClient, conferences, speakers);
            initialWarmupService.TouchEndpoints().GetAwaiter().GetResult();
        }
    }
}
