using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SenseNet.ContentRepository;
using SenseNet.IdentityServer4.WebClient;
using SenseNet.OData;
using SenseNet.Services.Core;
using SenseNet.Services.Core.Cors;
using SenseNet.Services.Core.Virtualization;

namespace SnWebApplication.Api.InMem.TokenAuth
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
            services.AddRazorPages();

            // [sensenet]: Authentication
            // Configure token authentication and add cookies so that non-script requests
            // (e.g. downloading files and images) work too.
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["sensenet:authentication:authority"];
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.Audience = "sensenet";
                })
                .AddDefaultSenseNetIdentityServerClients(Configuration["sensenet:authentication:authority"]);

            //UNDONE: cookie auth is missing

            //services.AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = "Cookies";
            //        options.DefaultChallengeScheme = "oidc";
            //    })
            //    .AddCookie("Cookies")
            //    .AddOpenIdConnect("oidc", options =>
            //    {
            //        options.Authority = Configuration["sensenet:authentication:authority"];
            //        options.RequireHttpsMetadata = false;

            //        options.ClientId = "mvc";
            //        options.ClientSecret = "secret";
            //        options.ResponseType = "code";

            //        options.SaveTokens = true;

            //        options.Scope.Add("sensenet");
            //    });

            // [sensenet]: add allowed client SPA urls
            services.AddSenseNetCors();

            //services.AddAuthorization();
            //services.AddAuthorization(options =>
            //{
            //    options.DefaultPolicy = new AuthorizationPolicyBuilder(
            //        CookieAuthenticationDefaults.AuthenticationScheme,
            //        JwtBearerDefaults.AuthenticationScheme)
            //        .RequireAuthenticatedUser()
            //        .Build();
            //});
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

            // [sensenet]: custom CORS policy
            app.UseSenseNetCors();
            // [sensenet]: use Authentication and set User.Current
            app.UseSenseNetAuthentication();

            //app.Use(async (context, next) =>
            //{
            //    await next();
            //    var bearerAuth = context.Request.Headers["Authorization"]
            //                         .FirstOrDefault()?.StartsWith("Bearer ") ?? false;
            //    if (context.Response.StatusCode == 401
            //        && !context.User.Identity.IsAuthenticated
            //        && !bearerAuth)
            //    {
            //        await context.ChallengeAsync("oidc");
            //    }
            //});

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                var user = User.Current.Name;

                if (next != null)
                    await next();
            });

            // [sensenet] Add the sensenet binary handler
            app.UseSenseNetFiles();

            // [sensenet]: OData middleware
            app.UseSenseNetOdata();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
