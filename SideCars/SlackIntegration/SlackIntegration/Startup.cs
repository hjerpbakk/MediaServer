using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlackConnector;
using SlackIntegration.Configuration;
using SlackIntegration.Services;

namespace SlackIntegration {
	public class Startup {
		public Startup(IHostingEnvironment env) {
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var config = Configuration.Get<AppConfig>();
			services.AddSingleton<ISlackConfig>(config);

			var conferenceMetaDataService = new ConferenceMetaDataService(config);
            var conferenceConfig = conferenceMetaDataService.CreateConferenceConfig().GetAwaiter().GetResult();
            services.AddSingleton(conferenceConfig);

			services.AddSingleton<ISlackConnector, SlackConnector.SlackConnector>();

			services.AddSingleton<SlackService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
			app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
