using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dalisama.LoggingExample.Middleware
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationMiddleware> _logger;

        public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //check if the request has already a correlation id
            context.Request.Headers.TryGetValue("X-Request-Trace", out StringValues traceId);
            // if no correlation id is found, generate a new one
            if (traceId == default(StringValues)) traceId = Guid.NewGuid().ToString();
            // add the correlation id to the response
            context.Response.Headers.Add("X-Request-Trace",traceId);
            // Adding the correlation id to all log related to the current request
            using (_logger.BeginScope(new Dictionary<string, string> { ["X-Request-Trace"] = traceId }))
            {
                await _next(context);
            }
        }
    }
}
