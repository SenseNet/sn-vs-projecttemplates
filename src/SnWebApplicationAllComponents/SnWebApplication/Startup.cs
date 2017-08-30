using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SnWebApplication.Startup))]
namespace SnWebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
