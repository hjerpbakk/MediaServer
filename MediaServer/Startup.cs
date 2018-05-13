using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using MediaServer.Clients;
using MediaServer.Configuration;
using MediaServer.Models;
using MediaServer.Services;
using MediaServer.Services.Cache;
using MediaServer.Services.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Polly;

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
			// TODO: Support tags, comma separated
			services.AddMemoryCache();
			services.AddResponseCaching();
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var config = Configuration.Get<AppConfig>();
			var conferenceConfiguration = new ConferenceConfiguration(config);
			var conferences = conferenceConfiguration.GetConferences().GetAwaiter().GetResult();
			services.AddSingleton(conferences);
			var conferenceIds = conferences.Values.Select(c => c.Id).ToArray();
			services.AddSingleton(conferenceIds);         
			services.AddSingleton<Paths>();

			var httpClient = new HttpClient();
			services.AddSingleton(httpClient);
			AddSlackIntegrationClient(services, httpClient);
			services.AddSingleton<CacheWarmerClient>();

			services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
			services.AddSingleton<IBlogStorageConfig>(config);

			services.AddSingleton<ThumbnailPersistence>();
			services.AddSingleton<ConferencePersistence>();
			services.AddSingleton<TalkPersistence>();

			services.AddSingleton<TalkService>();
			services.AddSingleton<ThumbnailService>();
			services.AddSingleton<ConferenceService>();
			services.AddSingleton<SpeakerService>();
			services.AddSingleton<ContentService>();
			services.AddSingleton<MediaCache>();
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
            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

		static void AddSlackIntegrationClient(IServiceCollection services, HttpClient httpClient)
        {
			Console.WriteLine("Populating metadata from Slack");
            var slackIntegrationClient = new SlackIntegrationClient(httpClient);
            var policyResult = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(new[] {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(4)
            }).ExecuteAndCaptureAsync(slackIntegrationClient.PopulateMetaData).GetAwaiter().GetResult();

            if (policyResult.FinalException != null)
            {
                Console.WriteLine($"Failed to populate metadata in slackIntegrationClient {policyResult.FinalException}");
            }

			Console.WriteLine("Getting users from Slack");
            var usersResult = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(new[] {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(4)
            }).ExecuteAndCaptureAsync(slackIntegrationClient.GetUsers).GetAwaiter().GetResult();

            if (policyResult.FinalException == null)
            {
                var users = new Users(usersResult.Result);
                services.AddSingleton(users);
            }
            else
            {
                Console.WriteLine($"Failed to get users from Slack {policyResult.FinalException}");
                services.AddSingleton(new Users(new User[0]));
            }

            services.AddSingleton<ISlackClient>(slackIntegrationClient);
        }
    }
}
