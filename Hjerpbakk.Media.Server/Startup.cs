using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hjerpbakk.Media.Server.Clients;
using Hjerpbakk.Media.Server.Configuration;
using Hjerpbakk.Media.Server.Environment;
using Hjerpbakk.Media.Server.Slack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlackConnector;

namespace Hjerpbakk.Media.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // For debugging
            services.AddDirectoryBrowser();

            var configuration = ReadConfig();
            services.AddSingleton<ISlackConfiguration>(configuration);
            services.AddSingleton<IBlobStorageConfiguration>(configuration);
            services.AddSingleton<IConferenceConfiguration>(configuration);
            services.AddSingleton<DebuggingService>();
            services.AddSingleton<ISlackConnector, SlackConnector.SlackConnector>();
            services.AddSingleton<SlackIntegration>();
            services.AddSingleton<CloudStorageClient>();
            services.AddSingleton<FileStorageClient>();
            services.AddSingleton<Paths>();
            services.AddSingleton<HttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDirectoryBrowser();
            }

            app.UseStaticFiles();

            app.UseMvc();
        }

        static AppConfiguration ReadConfig()
        {
            return JsonConvert.DeserializeObject<AppConfiguration>(File.ReadAllText("config.json"));
        }
    }
}
