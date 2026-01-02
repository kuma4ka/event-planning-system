using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventPlanning.Web.Filters;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled) return;

        switch (context.Exception)
        {
            case UnauthorizedAccessException:
                logger.LogWarning("Unauthorized access attempt caught by global filter. Path: {Path}", context.HttpContext.Request.Path);
                context.Result = new ForbidResult();
                context.ExceptionHandled = true;
                break;

            case KeyNotFoundException:
                logger.LogWarning("Resource not found caught by global filter. Path: {Path}", context.HttpContext.Request.Path);
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                break;
        }
    }
}
