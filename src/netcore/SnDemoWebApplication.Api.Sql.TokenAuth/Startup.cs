using System.IdentityModel.Tokens.Jwt;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SenseNet.Configuration;
using SenseNet.Extensions.DependencyInjection;
using SenseNet.Security.EFCSecurityStore;
using SenseNet.Services.Core.Authentication;
using SenseNet.Services.Core.Authentication.IdentityServer4;

namespace SnDemoWebApplication.Api.Sql.TokenAuth
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // [sensenet]: Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["sensenet:authentication:authority"];
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.Audience = "sensenet";

                    var metadataHost = Configuration["sensenet:authentication:metadatahost"];
                    if (!string.IsNullOrWhiteSpace(metadataHost))
                        options.MetadataAddress = $"{metadataHost}/.well-known/openid-configuration";

                    if (Environment.IsDevelopment())
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                        };
                    }
                })
                .AddDefaultSenseNetIdentityServerClients(Configuration["sensenet:authentication:authority"])
                .AddSenseNetRegistration(options =>
                {
                    // add newly registered users to this group
                    options.Groups.Add("/Root/IMS/Public/Administrators");
                });

            // [sensenet]: add sensenet services
            services.AddSenseNet(Configuration, (repositoryBuilder, provider) =>
                {
                    repositoryBuilder
                        .UseSecurityDataProvider(
                            new EFCSecurityDataProvider(connectionString: ConnectionStrings.ConnectionString))
                        .UseLucene29LocalSearchEngine(Path.Combine(System.Environment.CurrentDirectory, "App_Data",
                            "LocalIndex"));
                })
                .AddAsposeDocumentPreviewProvider(options =>
                {
                    options.SkipLicenseCheck = Configuration.GetValue("sensenet:AsposePreviewProvider:SkipLicenseCheck", false);
                })
                .AddSenseNetClientTokenStore();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // [sensenet]: general cookie settings
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always
            });

            // [sensenet]: custom CORS policy
            app.UseSenseNetCors();
            // [sensenet]: use Authentication and set User.Current
            app.UseSenseNetAuthentication(options =>
            {
                options.AddJwtCookie = true;
            });
            
            app.UseAuthorization();

            // [sensenet] Add the sensenet binary handler
            app.UseSenseNetFiles();

            // [sensenet]: OData middleware
            app.UseSenseNetOdata();
            // [sensenet]: WOPI middleware
            app.UseSenseNetWopi();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("sensenet is listening. Visit https://sensenet.com for " +
                                                      "more information on how to call the REST API.");
                });
            });
        }
    }
}
