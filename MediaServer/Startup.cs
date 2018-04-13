using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlackConnector;

namespace MediaServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
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

            var config = Configuration.Get<AppConfig>();
            services.AddSingleton<IConferenceConfig>(config);
            services.AddSingleton<IBlogStorageConfig>(config);
			services.AddSingleton<ISlackConfig>(config);

			services.AddSingleton<ISlackConnector, SlackConnector.SlackConnector>();
                        
            services.AddSingleton<ITalkService, TalkService>();
            services.AddSingleton<IContentService, DummyContentService>();
			services.AddSingleton<ISlackService, SlackService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
				// TODO: Fix this
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

			// TODO: Delete and use latest talks in conference controller as deafult
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
