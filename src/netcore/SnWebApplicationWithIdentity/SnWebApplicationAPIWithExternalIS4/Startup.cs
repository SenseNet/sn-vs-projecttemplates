using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SenseNet.IdentityServer4.WebClient;
using SenseNet.OData;
using SenseNet.Services.Core;

namespace SnWebApplicationAPIWithExternalIS4
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

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:44311";
                    options.RequireHttpsMetadata = false;

                    options.Audience = "sensenet";
                })
                .AddDefaultSenseNetIdentityServerClients("https://localhost:44311");

            // add allowed client SPA urls
            services.AddCors(options =>
            {
                options.AddPolicy("default",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:44341")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("default");

            // [sensenet]: Authentication
            app.UseSenseNetAuthentication(); 
            
            app.UseAuthorization();

            // [sensenet]: OData
            app.UseSenseNetOdata();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
