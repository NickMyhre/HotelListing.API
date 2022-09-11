using HotelListing.API.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace HotelListing.API.Core.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        //requestdelegate initializes request object to handle each request that comes into the class
        //hijacks each request and does stuff while it's being processed
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }

        //puts every request in a universal try catch statement
        public async Task InvokeAsync(HttpContext context)
        {
            //try the next operation
            try
            {
                //awaiting the result of the next operation relative to the request
                await _next(context);
            }
            catch (Exception ex)
            {
                //log information about the exception and include information from the httpcontext, shows what they were trying to do
                _logger.LogError(ex, $"Something Went Wrong while processing {context.Request.Path}");
                //if exceptions are thrown, handle them and create response
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            //set default exception properties
            context.Response.ContentType = "application/json";
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            //error response object
            //also a potential option to standardize error responses
            var errorDetails = new ErrorDetails
            {
                ErrorType = "Failure",
                ErrorMessage = ex.Message
            };

            //catch exceptions of a specific type
            switch (ex)
            {
                //change default exception properties
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorDetails.ErrorType = "Not Found";
                    break;
                default:
                    break;
            }


            //convert error details object to JSON string
            string response = JsonConvert.SerializeObject(errorDetails);
            //change the status code to current status code
            context.Response.StatusCode = (int)statusCode;

            //return response object after modifications
            return context.Response.WriteAsync(response);
        }
    }
    public class ErrorDetails
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }
}
