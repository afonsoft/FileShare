using System;
using System.IO;
using FileShare.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace FileShare
{
    public class Program
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationIdentityUser>>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<ApplicationIdentityRole>>();


                // Create the role if it doesn't already exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    // Create an identity role object out of the enum value
                    await roleManager.CreateAsync(new ApplicationIdentityRole
                    {
                        Id = Guid.NewGuid(),
                        Name = "Admin"
                    });
                }

                // Create the role if it doesn't already exist
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    // Create an identity role object out of the enum value
                    await roleManager.CreateAsync(new ApplicationIdentityRole
                    {
                        Id = Guid.NewGuid(),
                        Name = "User"
                    });
                }

                // Our default user
                var user = new ApplicationIdentityUser
                {
                    Email = "afonsoft@afonsoft.com.br",
                    UserName = "afonsoft@afonsoft.com.br",
                    LockoutEnabled = false
                };

                // Add the user to the database if it doesn't already exist
                if (await userManager.FindByEmailAsync(user.Email) == null)
                {
                    await userManager.CreateAsync(user, "Senha#2020");
                    await userManager.AddToRolesAsync(user, new string[] { "Admin", "User" });
                }
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
                .CaptureStartupErrors(true)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel(options => 
                { 
                    options.AddServerHeader = false;
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(120);
                    options.Limits.MaxRequestBodySize = 6000000000;
                    options.Limits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(10));
                    options.Limits.RequestHeadersTimeout =  TimeSpan.FromMinutes(120);
                    options.Limits.Http2.HeaderTableSize = 4096;
                    options.Limits.Http2.MaxStreamsPerConnection = 255;
                    options.Limits.Http2.MaxFrameSize = 16777215;
                    options.Limits.Http2.MaxRequestHeaderFieldSize = 16384;

                })
                .UseIIS()
                .UseIISIntegration()
                .UseStartup<Startup>();
        }
    }
}
