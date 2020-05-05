using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.InMemory;
using SenseNet.ContentRepository.Security;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Packaging;

namespace SnDemoWebApplication.Api.InMem.TokenAuth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var host = builder.Build();

            using (InMemoryExtensions.StartInMemoryRepository(repositoryBuilder =>
            {
                repositoryBuilder
                    .UseAccessProvider(new UserAccessProvider());
                //UNDONE: set doc preview provider here

                //.DisableNodeObservers();
            }))
            {
                // put repository initialization here (e.g. import)

                using (new SystemAccount())
                {
                    var parent = RepositoryTools.CreateStructure("/Root/temp", "SystemFolder").ContentHandler;
                    const string fileName = "abc.docx";
                    var file = new File(parent)
                    {
                        Name = fileName,
                        Binary = new BinaryData
                        {
                            FileName = fileName
                        }
                    };

                    file.Binary.SetStream(System.IO.File.OpenRead("abc.docx"));
                    file.Save();
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
