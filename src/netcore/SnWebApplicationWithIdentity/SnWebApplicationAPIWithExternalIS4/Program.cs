using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.InMemory;
using SenseNet.ContentRepository.Security;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;

namespace SnWebApplicationAPIWithExternalIS4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();

            SnTrace.EnableAll();

            using (InMemoryExtensions.StartInMemoryRepository(repositoryBuilder =>
            {
                repositoryBuilder
                    .UseAccessProvider(new UserAccessProvider())
                    .UseLogger(new SnFileSystemEventLogger())
                    .UseTracer(new SnFileSystemTracer());
            }))
            {
                // create a temp user and make it admin
                using (new SystemAccount())
                {
                    var user = new User(User.Administrator.Parent)
                    {
                        Name = "edvin",
                        LoginName = "edvin",
                        Password = "edvin",
                        Email = "edvin@example.com"
                    };
                    user.Save();
                    Group.Administrators.AddMember(user);
                }

                SnTrace.EnableAll();
                host.Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
