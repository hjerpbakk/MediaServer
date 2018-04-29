using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaServer.Configuration;
using MediaServer.Services;
using MediaServer.Services.Cache;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SlackConnector;

namespace MediaServer
{
    public class Startup
    {
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
            services.AddMemoryCache();
            services.AddResponseCaching();
            services.AddMvc();

            var config = Configuration.Get<AppConfig>();         
            services.AddSingleton<IConferenceConfig>(config);
            services.AddSingleton<IBlogStorageConfig>(config);
			services.AddSingleton<ISlackConfig>(config);
            
			services.AddSingleton<MediaCache>();

			services.AddSingleton<ISlackConnector, SlackConnector.SlackConnector>();

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            
			services.AddSingleton<IOldTalkService, OldTalkService>();
            
			services.AddSingleton<IThumbnailService, ThumbnailService>();

			services.AddSingleton<IConferenceService, ConferenceService>();
            
			services.AddSingleton<IContentService, ContentService>();
			services.AddSingleton<ISlackService, SlackService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			// TODO: Page for version, configuration and runtime environment
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
				TelemetryDebugWriter.IsTracingDisabled = true;
            }
            else
            {
				// TODO: Fix this
                app.UseExceptionHandler("/Home/Error");
            }
                     
            const int OneYear = 31536000;
            var MaxAgeStaticFiles = "public,max-age=" + OneYear;
            var OneYearTimeSpan = TimeSpan.FromSeconds(OneYear);
			var varyByAllQueryKeys = new[] { "*" };
            app.UseResponseCaching();
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = OneYearTimeSpan
                };
                context.Response.Headers[HeaderNames.Vary] = new string[] { "Accept-Encoding" };
				var responseCachingFeature = context.Features.Get<IResponseCachingFeature>();
				if (responseCachingFeature != null) {
					responseCachingFeature.VaryByQueryKeys = varyByAllQueryKeys;
                }

                await next();
            });

            app.UseStaticFiles(new StaticFileOptions {
                OnPrepareResponse = ctx => {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = MaxAgeStaticFiles;
                }
            });

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
