using Microsoft.AspNetCore.Diagnostics;
using UserService.Domain.Exceptions;

namespace UserService.Presentation.Handlers
{
    public class ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetailsService)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception, CancellationToken cancellationToken)
        {
            int statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };

            string title = exception switch
            {
                UserNotFoundException => "User not found",
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
