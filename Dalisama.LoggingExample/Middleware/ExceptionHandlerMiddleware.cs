using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Dalisama.LoggingExample.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;


        public ExceptionHandlerMiddleware( RequestDelegate next,
            ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;

        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.Headers.TryGetValue("X-Request-Trace", out StringValues traceId);
            if (traceId == default(StringValues)) traceId = Guid.NewGuid().ToString();
            var error = new 
            {
                Id = traceId.ToString(),
                Status = (short)HttpStatusCode.InternalServerError,
                Title = "Some kind of error occurred in the API.  Please use the id and contact our " +
                        "support team if the problem persists."
            };

    

            var innerExMessage = GetInnermostExceptionMessage(exception);

            _logger.LogError(exception,  innerExMessage + " -- {ErrorId}.", error.Id);

            var result = JsonConvert.SerializeObject(error);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(result);
        }

        private string GetInnermostExceptionMessage(Exception exception)
        {
            if (exception.InnerException != null)
                return GetInnermostExceptionMessage(exception.InnerException);

            return exception.Message;
        }
    }

}





  


