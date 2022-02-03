using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.Client;
using SenseNet.Extensions.DependencyInjection;

namespace SnConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            // server context factory service is available through DI 
            var factory = host.Services.GetRequiredService<IServerContextFactory>();

            //TODO: configure repository url below
            var server = await factory.GetServerAsync();

            // start working with the sensenet content repository
            var content = await Content.LoadAsync("/Root/Content", server);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder
                    .AddJsonFile("appsettings.json", true, true)
                    .AddUserSecrets<Program>()
                )
                .ConfigureServices((hb, services) => services
                    .AddLogging(logging =>
                    {
                        logging.ClearProviders()
                            .AddConsole()
                            .AddFile("logs/sn-console-client-{Date}.txt", LogLevel.Trace);
                    })
                    .AddSenseNetRepository(options =>
                    {
                        //TODO: configure repository url and authentication here
                        hb.Configuration.Bind("sensenet:Repository", options);
                    })                    
                );
    }
}
