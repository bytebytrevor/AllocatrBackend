using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AllocatrApi.Filters;

public sealed class ProjectWorkflowExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case InvalidOperationException invalid:
                context.Result = new ConflictObjectResult(new { message = invalid.Message });
                break;
            case KeyNotFoundException missing:
                context.Result = new NotFoundObjectResult(new { message = missing.Message });
                break;
            case UnauthorizedAccessException:
                context.Result = new ForbidResult();
                break;
            default:
                return;
        }
        context.ExceptionHandled = true;
    }
}
