using FileShare.Repository;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;

namespace FileShare.Filters
{
    public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var _userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationIdentityUser>>();

            if (httpContext.User != null)
            {
                var user = _userManager.GetUserAsync(httpContext.User).Result;
                if (user != null)
                {
                    return _userManager.IsInRoleAsync(user, "Admin").Result;
                }
            }
            return false;
        }
    }
}