using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using HalfMoon.Query;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HalfMoon.Endpoint.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"_private/appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IContainer ApplicationContainer { get; private set; }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(TrustServiceUrlAttribute));
            });

            services.AddOptions();

            services.AddSingleton(_ => Configuration);
            services.AddSingleton<QueryEngine>();
            services.Configure<BotFrameworkOptions>(Configuration.GetSection("BotFramework"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // We use this application behind a reverse proxy.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseBotAuthentication(new StaticCredentialProvider(
                Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
                Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value
            ));

            app.UseMvc();

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
        }
    }
}
