using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace FileShare.Filters
{

    public class GenerateAntiforgeryTokenAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            CreateToken(context);
            base.OnResultExecuting(context);
        }
        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            CreateToken(context);
            return base.OnResultExecutionAsync(context, next);
        }
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }

        private void CreateToken(ResultExecutingContext context)
        {
            var antiforgery = context.HttpContext.RequestServices.GetService<IAntiforgery>();

            // Send the request token as a JavaScript-readable cookie
            var tokens = antiforgery.GetAndStoreTokens(context.HttpContext);
            antiforgery.SetCookieTokenAndHeader(context.HttpContext);

            context.HttpContext.Response.Headers.Append(
                "_AntiforgeryToken",
                new Microsoft.Extensions.Primitives.StringValues(tokens.RequestToken));
        }
    }
}
