using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameReactor.TwitterBot
{
    class Startup
    {
        public IServiceProvider Provider => provider;
        public IConfiguration Configuration => configuration;

        private readonly IConfiguration configuration;
        private readonly IServiceProvider provider;
        public Startup()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{environment}.json", optional: true)
                            .AddJsonFile($"secrets.json", optional: false)
                            .AddEnvironmentVariables()
                            .Build();

            var services = new ServiceCollection();

            services.AddSingleton(Configuration);
            services.AddTransient<TwitterService>();

            // build the pipeline
            provider = services.BuildServiceProvider();
        }
    }
}
