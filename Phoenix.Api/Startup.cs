using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Phoenix.Api.App_Plugins;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Talagozis.AspNetCore.Services.TokenAuthentication;

namespace Phoenix.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(this._configuration.GetConnectionString("AuthConnection")));
            services.AddDbContext<PhoenixContext>(options => options.UseLazyLoadingProxies().UseSqlServer(this._configuration.GetConnectionString("PhoenixConnection")));
            
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            }).AddUserStore<ApplicationStore>().AddUserManager<ApplicationUserManager>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            //services.TryAddScoped<ApplicationUserManager>();

            services.AddTokenAuthentication<UserManagementService>(this._configuration);

            services.AddCors();

            services.AddHttpsRedirection(options => options.HttpsPort = 443);
            
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    
                    //options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                    //{
                    //    NamingStrategy = new Newtonsoft.Json.Serialization.DefaultNamingStrategy()
                    //};
                });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();

                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseRouting();

            app.UseAuthentication();
            app.Use(async (context, next) =>
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                ILogger logger = context.RequestServices.GetService(typeof(ILogger<Startup>)) as ILogger;

                if (logger == null)
                    return;

                ClaimsPrincipal claimsPrincipal = context?.User;
                if (claimsPrincipal == null)
                {
                    logger.LogTrace("No authorized user is set");
                    return;
                }

                logger.LogTrace($"{nameof(ClaimTypes.NameIdentifier)}: {claimsPrincipal.getNameIdentifier()}");
                logger.LogTrace($"{nameof(ClaimTypes.Role)}s: {string.Join(", ", claimsPrincipal.getRoles())}");

                await next();
            });
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
