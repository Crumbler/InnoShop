﻿namespace ProductService.Domain.Exceptions
{
    public class UnauthorizedException : Exception
    {
        protected UnauthorizedException(string message) : base(message) { }
    }
}
