using System;
using System.IO;
using System.IO.Compression;
using Afonsoft.Logger;
using FileShare.Extensions;
using FileShare.Interfaces;
using FileShare.Jobs;
using FileShare.Repository;
using Hangfire;
using Hangfire.Console;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            services.Configure<IISOptions>(o =>
            {
                o.ForwardClientCertificate = true;
            });

            services.AddAfonsoftLogging();
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddEntityFrameworkSqlite();
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
                options.UseSqlite(connectionString);
            });

            services.AddHangfire(x =>
            {
                x.UseSQLiteStorage(connectionString);
                x.UseConsole();
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

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 6000000000;
            });

            services.AddAntiforgery(options =>
            {
                options.FormFieldName = "RequestVerificationToken";
                options.HeaderName = "X-CSRF-TOKEN-REQUEST";
                options.SuppressXFrameOptionsHeader = false;
            });

            services.AddTransient<IHangfireJob, HangfireJob>();


            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.EnableForHttps = true;
            });

            services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });

            services.AddHangfireServer();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();

            app.UseHangfireDashboard();

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

            InitializeHangfire(new HangfireJob());
        }

        private void InitializeHangfire(HangfireJob job)
        {
            job.Initialize();
        }
    }
}
