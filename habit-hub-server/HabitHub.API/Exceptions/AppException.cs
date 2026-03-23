using System;

namespace HabitHub.API.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }

        protected AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }


    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException(string message) : base(message, 401) { }
    }
    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(message, 409) { }
    }
    public class ValidationException : AppException
    {
        public ValidationException(string message) : base(message, 400) { }
    }


}