using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using HabitHub.API.Exceptions;

namespace HabitHub.API.Middleware
{
    public class ExceptionMiddleware
    {

        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex) 
            {
                await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
            }

        }
        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                error = message,
                statusCode = statusCode
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
