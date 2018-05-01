using System.Net.Http;
using CachePopulator.Configuration;
using CachePopulator.Extensions;
using CachePopulator.Services;
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
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();

			var config = Configuration.Get<AppConfig>();
			var conferenceMetaDataService = new ConferenceMetaDataService(config);
			var conferences = conferenceMetaDataService.CreateConferenceConfig().GetAwaiter().GetResult();
			services.AddSingleton(conferences);

            services.AddSingleton<HttpClient>();
            services.AddSingleton<FireAndForgetService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            app.WarmupCache();
        }
    }
}
