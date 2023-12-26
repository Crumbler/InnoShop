using Microsoft.AspNetCore.Diagnostics;
using ProductService.Domain.Exceptions;

namespace ProductService.Presentation.Handlers
{
    public class ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetailsService)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int statusCode = exception switch
            {
                UnauthorizedException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => 0
            };

            if (statusCode == 0)
            {
                return false;
            }

            string title = exception switch
            {
                ProductNotFoundException => "Product not found",
                OtherUserAdminOnlyException => "Unauthorized attempt to edit other user's product",
                _ => "An error occured"
            };

            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Title = title,
                    Detail = exception.Message,
                    Type = exception.GetType().Name
                },
                Exception = exception
            });
        }
    }
}
