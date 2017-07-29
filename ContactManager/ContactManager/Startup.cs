using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ContactManager.Data;
using ContactManager.Models;
using ContactManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ContactManager.Authorization;

namespace ContactManager
{
    public class Startup
    {
        private IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var skipSsl = Configuration.GetValue<bool>("LocalTest:skipSsl");

            services.Configure<MvcOptions>(options =>
            {
                if (_environment.IsDevelopment() && !skipSsl)
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                }
            });

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Authorization Handlers
            services.AddScoped<IAuthorizationHandler, ContactIsOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ContactAdministratorsAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ContactManagerAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            _environment = env;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var testUserPassword = Configuration["SeedUserPW"];
            if (string.IsNullOrEmpty(testUserPassword))
            {
                throw new Exception(
                    "Use secrets manager to set SeedUserPW \n" +
                    "dotnet user-secrets set SeedUserPW <pw>");
            }

            try
            {
                SeedData.Initialize(app.ApplicationServices, "").Wait();
            }
            catch
            {
                throw new System.Exception("You need to update the DB "
                    + "\nPM > Update-Database " + "\n or \n" +
                      "> dotnet ef database update"
                      + "\nIf that doesn't work, comment out SeedData and register a new user");
            }
        }
    }
}
