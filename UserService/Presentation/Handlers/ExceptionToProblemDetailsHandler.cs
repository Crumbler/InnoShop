﻿using Microsoft.AspNetCore.Diagnostics;
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
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthenticatedException => StatusCodes.Status401Unauthorized,
                UnauthorizedException => StatusCodes.Status403Forbidden,
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
                InvalidTokenException => "Invalid token",
                UserAlreadyConfirmedException => "User already confirmed",
                SamePasswordException => "Identical password",
                OtherUserAdminOnlyException => "Unauthorized attempt to edit other user",
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
