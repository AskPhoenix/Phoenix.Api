using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Phoenix.Api.App_Plugins;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Talagozis.AspNetCore.Services.TokenAuthentication;

namespace Phoenix.Api
{
    public class Startup
    {
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
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
