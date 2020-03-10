using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.InMemory;
using SenseNet.ContentRepository.Security;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.Packaging;

namespace SnWebApplication.Api.InMem.TokenAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();

            using (InMemoryExtensions.StartInMemoryRepository(repositoryBuilder =>
            {
                repositoryBuilder.UseAccessProvider(new UserAccessProvider());
            }))
            {
                // put repository initialization here (e.g. import)

                //UNDONE: remove temp content
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

                    var parent = RepositoryTools.CreateStructure("/Root/MyContent", "SystemFolder");
                    var docLib = RepositoryTools.CreateStructure("/Root/MyContent/MyFiles", "DocumentLibrary");

                    ((GenericContent)docLib.ContentHandler).AllowChildType("Image", save: true);

                    var file = new File(docLib.ContentHandler) { Name = "testfile.txt" };
                    file.Binary.SetStream(RepositoryTools.GetStreamFromString($"temp text data {DateTime.UtcNow}"));
                    file.Save();

                    var installer = new Installer();
                    installer.Import("C:\\temp\\import\\Root");
                }

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
