using Afonsoft.Logger;
using FileShare.Extensions;
using FileShare.Filters;
using FileShare.Interfaces;
using FileShare.Jobs;
using FileShare.Repository;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Heartbeat;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO.Compression;
using System.Text;

namespace FileShare
{
    public class Startup
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 6000000000;
            });

            services.AddAfonsoftLogging();
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddEntityFrameworkSqlServer();
            services.AddAntiforgery();
            services.AddHttpClient();

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddDataProtection()
            .SetDefaultKeyLifetime(TimeSpan.FromDays(365));

            string connectionString = _appConfiguration.GetConnectionString("Default");

            services.AddSingleton(typeof(IRepository<,>), typeof(Repository<,>));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseLazyLoadingProxies(true);
                options.UseSqlServer(connectionString, options =>
                {
                    options.CommandTimeout(300);
                    options.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
                });
            }, ServiceLifetime.Transient);

            services.AddIdentity<ApplicationIdentityUser, ApplicationIdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddHangfire(configuration => configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseConsole()
            .UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(5))
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.LoginPath = new PathString("/Auth/Login");
                options.AccessDeniedPath = new PathString("/Auth/Denied");
                options.SlidingExpiration = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.LoginPath = new PathString("/Auth/Login");
                        options.AccessDeniedPath = new PathString("/Auth/Denied");
                        options.SlidingExpiration = true;
                        options.Cookie.IsEssential = true;
                        options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    })
                 .AddJwtBearer(options =>
                 {
                     options.Audience = "https://files.afonsoft.com.br/";
                     options.Authority = "https://files.afonsoft.com.br/";
                     options.RequireHttpsMetadata = false;
                     options.SaveToken = true;
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuerSigningKey = true,
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("Senha#2021Afonsoft")),
                         ValidateIssuer = true,
                         ValidateAudience = true
                     };
                 });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#!$";
                options.User.RequireUniqueEmail = true;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.IsEssential = true;
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 6000000000;
                options.MultipartBoundaryLengthLimit = 512;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddAntiforgery(options =>
            {
                options.FormFieldName = "RequestVerificationToken";
                options.HeaderName = "X-CSRF-TOKEN-REQUEST";
                options.SuppressXFrameOptionsHeader = false;
            });

            services.TryAddSingleton<IHangfireJob, HangfireJob>();

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.EnableForHttps = false;
            });

            services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2;
                options.HeartbeatInterval = TimeSpan.FromSeconds(5);
                options.ServerCheckInterval = TimeSpan.FromSeconds(5);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHsts();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DisplayStorageConnectionString = false,
                Authorization = new[] { new AllowAllDashboardAuthorizationFilter() },
                IsReadOnlyFunc = (DashboardContext context) => !AllowAllDashboardAuthorizationFilter.IsUserAuthorizedToEditHangfireDashboard(context)
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            InitializeHangfire(serviceProvider.GetService<IHangfireJob>());
        }

        private void InitializeHangfire(IHangfireJob job)
        {
            job.Initialize();
        }
    }
}