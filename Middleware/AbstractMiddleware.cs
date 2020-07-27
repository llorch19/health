using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public abstract class AbstractMiddleware
    {
        protected RequestDelegate _next;
        public AbstractMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public abstract Task InvokeAsync(HttpContext context);
    }
}
