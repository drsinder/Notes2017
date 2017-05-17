/*--------------------------------------------------------------------------
**
**  Copyright (c) 2017, Dale Sinder
**
**  Name: Startup.cs
**
**  Description:
**      Startup code for Application
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notes2017.Data;
using Notes2017.Models;
using Notes2017.Services;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Hangfire.Annotations;
using Microsoft.Owin;
using Autofac;

namespace Notes2017
{
    public class Startup
    {
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

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<SQLFileDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SQLFileConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            if (string.Compare(Configuration["RequireSSL"], "true") == 0)
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                });
            }

            services.AddMemoryCache();
            services.AddSession();

            services.AddMvc();

            Globals.AdminEmail = Configuration["DefaultAdmin:Email"];
            Globals.SendGridEmail = Configuration["DefaultAdmin:SendGridEmail"];
            Globals.EmailName = Configuration["DefaultAdmin:SendGridEmailName"];
            Globals.SendGridApiKey = Configuration["DefaultAdmin:SendGridApiKey"];

            Globals.TwilioName = Configuration["DefaultAdmin:TwilioName"];
            Globals.TwilioPassword = Configuration["DefaultAdmin:TwilioPassword"];
            Globals.TwilioNumber = Configuration["DefaultAdmin:TwilioNumber"];


            Globals.InstKey = Configuration["ApplicationInsights:InstrumentationKey"];

            Globals.ProductionUrl = Configuration["Version:ProductionUrl"];

            Globals.ZoneMinId = int.Parse(Configuration["TZone:MinID"]);
            Globals.ZoneUtcid = int.Parse(Configuration["TZone:UTCID"]);

            Globals.PusherAppId = Configuration["Pusher:AppId"];
            Globals.PusherKey = Configuration["Pusher:AppKey"];
            Globals.PusherSecret = Configuration["Pusher:AppSecret"];

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment() || string.Compare(Configuration["RequireSSL"], "false") == 0)
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

            app.UseSession();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            if (Configuration["Authentication:GoogleAccount:ClientId"].Length > 5)
            {
                app.UseGoogleAuthentication(new GoogleOptions()
                {
                    ClientId = Configuration["Authentication:GoogleAccount:ClientId"],
                    ClientSecret = Configuration["Authentication:GoogleAccount:ClientSecret"]
                });
            }

            if (Configuration["Authentication:Twitter:ConsumerKey"].Length > 5)
            {
                app.UseTwitterAuthentication(new TwitterOptions()
                {
                    ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"],
                    ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"]
                });
            }

            if (Configuration["Authentication:Facebook:AppID"].Length > 5)
            {
                app.UseFacebookAuthentication(new FacebookOptions()
                {
                    AppId = Configuration["Authentication:Facebook:AppId"],
                    AppSecret = Configuration["Authentication:Facebook:AppSecret"],
                    //Scope.Add("public_profile"), 
                    //Scope.Add("email");
                    UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,name,email,first_name,last_name"
                });
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            ConfigHangfire(app);

        }

        private void ConfigHangfire(IApplicationBuilder app)
        {
            JobStorage.Current = new SqlServerStorage(Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions());

            app.UseHangfireServer();
            app.UseHangfireDashboard("/Hangfire", new DashboardOptions());

            //, new DashboardOptions { AuthorizationFilters = new List<IDashboardAuthorizationFilter> { new { Roles = "Admin" } } });
        }
    }

    internal class AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public string Roles { get; set; }

        public bool Authorize([NotNull] DashboardContext context)
        {
            IDictionary<string, object> owinEnv = context.GetOwinEnvironment();
            var owinContext = new OwinContext(owinEnv);
            return owinContext.Authentication.User.IsInRole("Admin");
        }
    }


    ////////////////////

    public interface IContainerJobActivator
    {
        JobActivator Current { get; set; }

        object ActivateJob(Type jobType);
        JobActivatorScope BeginScope();
    }

    public class ContainerJobActivator : JobActivator
    {
        private IContainer _container;

        public ContainerJobActivator(IContainer container)
        {
            _container = container;
        }

        public override object ActivateJob(Type type)
        {
            return _container.Resolve(type);
        }
    }


}

