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
using TexasTRInventory.Data; //EXP
using TexasTRInventory.Models;
using TexasTRInventory.Services;
using TexasTRInventory.Authorization;
using Microsoft.Azure.KeyVault;
using System.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using TexasTRInventory.Constants;

namespace TexasTRInventory
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            GlobalCache.Initialize(Configuration, WebConfigurationManager.AppSettings);

            //For every class that uses my triple-model pattern (abstract, view, and db) call their mapper intializer here
            Product.MapperInitializer();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<InventoryContext>(options =>
               options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<InventoryContext>()
                .AddDefaultTokenProviders();

            
            services.AddMvc(config => //EXP 9.11. from https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
            
            //EXP 9/7/17. Adding this from https://blogs.msdn.microsoft.com/jpsanders/2017/05/16/azure-net-core-application-settings/
            //goal is to read env. variables from azure
            services.AddSingleton<IConfiguration>(Configuration);

                //EXP 8.2.17. copying from internet
            services.Configure<IdentityOptions>(options =>
            {
                //Password settings
                options.Password.RequireDigit = false;

                options.Password.RequiredLength = 2;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                //Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 20;

                //Cookie settings
                options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                options.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
                options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOut";

                // User settings
                options.User.RequireUniqueEmail = true;
            });
            
            //EXP 9.20.17 Hopefully this does the magic to make ApplicationUser.EmployerID map to a claim
            services.AddScoped<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            //services.AddTransient<ISmsSender, AuthMessageSender>(); EXP 9.2.17 I have no intention of supporting SMS

            //EXP 8.2.17 apparently this allows us to send emails
            //EXP 8.9.17 no longer using that user secret store garbage
            //services.Configure<AuthMessageSenderOptions>(Configuration);

            //EXP 9.11.17 https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data
            //Allows the authorization handlers to work
            services.AddSingleton<IAuthorizationHandler, InternalUserAuthorizationHandler>(); //EXP 12.7.17. Changing to Singleton. Should be good
            //EXP 12.7.17 adding another handler
            services.AddScoped<IAuthorizationHandler, TooBig4YourBritchesAuthorizationHandler>();
            //EXP 2.27.18 adding another
            services.AddScoped<IAuthorizationHandler, LockedProductAuthorizationHandler>();
            

            //EXP 9.20.17. I think the scopeds above are wrong. this is how to do it.
            //https://stackoverflow.com/questions/31464359/how-do-you-create-a-custom-authorizeattribute-in-asp-net-core
            services.Configure<AuthorizationOptions>(options =>
            {
                options.AddPolicy(PolicyNames.IsInternal, policy => policy.Requirements.Add(new InternalUserAuthorizationHandler()));
                options.AddPolicy(PolicyNames.OnlyAdminsEditAdmins, policy => policy.Requirements.Add(new TooBig4YourBritchesAuthorizationHandler()));
                //EXP 2.27.18 adding more handlers
                options.AddPolicy(PolicyNames.IsAdmin, policy => policy.Requirements.Add(new AdminAuthorizationHandler()));
                options.AddPolicy(PolicyNames.CanEditLockedProduct, policy => policy.Requirements.Add(new LockedProductAuthorizationHandler()));
            });

            //EXP 9.15.17 Don't understand, but apparently this is good for security
            //https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
            if (!GlobalCache.IsDevelopment())
            {
                services.Configure<MvcOptions>(options =>
                    {
                        options.Filters.Add(new RequireHttpsAttribute());
                    }
                 );
            }

        }

        //9.11.17 Making this async void. This is bad, but maybe we can get a heter because this is like an event handler?
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, InventoryContext context)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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


            //EXP 9.15.17 I should put this in for security, but it seems that I can't download the rewrite package
            //https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
            if (!GlobalCache.IsDevelopment())
            {
                var options = new RewriteOptions()
                   .AddRedirectToHttps();

                app.UseRewriter(options);
            }

            //EXP
            await DbInitializer.Initialize(context);
        }
    }
}
