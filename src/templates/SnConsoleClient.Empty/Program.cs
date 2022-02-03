using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SenseNet.Client;
using SenseNet.Extensions.DependencyInjection;

namespace SnConsoleClient.Empty
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
                .ConfigureServices((hb, services) => services
                    .AddSenseNetRepository(options =>
                    {
                        //TODO: configure repository url and authentication here
                        options.Url = "";
                        options.Authentication.ClientId = "";
                        options.Authentication.ClientSecret = "";
                    })                    
                );
    }
}
