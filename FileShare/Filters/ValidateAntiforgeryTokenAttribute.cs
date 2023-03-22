using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace FileShare.Filters
{
    public class ValidateAntiforgeryTokenAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            ValidateToken(context);
            base.OnResultExecuting(context);
        }

        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            ValidateToken(context);
            return base.OnResultExecutionAsync(context, next);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
        }

        private void ValidateToken(ResultExecutingContext context)
        {
        }
    }
}