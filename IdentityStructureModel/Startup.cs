using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityStructureModel.CustomAuthorization;
using IdentityStructureModel.CustomValidations;
using IdentityStructureModel.EmailSender;
using IdentityStructureModel.IdentityDbContexts;
using IdentityStructureModel.IdentityModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityStructureModel
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
            

            services.AddDbContext<AppIdentityDbContext>(opt =>
            {
                opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IEmailSender, SmtpEmailSender>(i =>
            {
                return new SmtpEmailSender(
                    Configuration["EmailSender:Host"],
                    Configuration.GetValue<int>("EmailSender:Port"),
                    Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    Configuration["EmailSender:UserName"],
                    Configuration["EmailSender:Password"]
                );
            });

            services.AddAuthentication().AddFacebook(opt =>
            {
                opt.AppId = Configuration["Authentication:FacebookAppId"];
                opt.AppSecret = Configuration["Authentication:FacebookAppSecret"];
            });

            services.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequiredUniqueChars = 3;
                opt.Password.RequiredLength = 5;
            }).AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddUserValidator<CustomUserValidator>()
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>();

            services.ConfigureApplicationCookie(opt =>
            {
                opt.AccessDeniedPath = "/Account/AccessDenied";
                opt.LoginPath = "/account/login";
                opt.LogoutPath = "/Home/Index";
                opt.Cookie = new CookieBuilder()
                {
                    SameSite = SameSiteMode.Strict,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest,
                    Name = "IdentityStructureModelCookie",
                    HttpOnly = false,
                };
                opt.ExpireTimeSpan = TimeSpan.FromDays(15);
                opt.SlidingExpiration = true;
            });
            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("CityPolicy", policy =>
                {
                    policy.RequireClaim("city", "ankara");
                });
                opt.AddPolicy("FreeDayPolicy", policy =>
                {
                    policy.AddRequirements(new ExpireFreeDayRequirement());
                });
            });
            
            services.AddTransient<IAuthorizationHandler, ExpireFreeDayHandler>();
            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();
            services.AddControllersWithViews();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
