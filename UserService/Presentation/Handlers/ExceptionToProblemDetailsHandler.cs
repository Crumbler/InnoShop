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
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => 0
            };

            if (statusCode == 0)
            {
                return false;
            }

            string title = exception switch
            {
                InvalidCredentialsException => "Invalid credentials",
                UserNotFoundException => "User not found",
                EmailInUseException => "Email in use",
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
