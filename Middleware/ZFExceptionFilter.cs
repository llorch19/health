using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class ZFExceptionFilter : IAsyncExceptionFilter
    {
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (!context.HttpContext.Response.HasStarted)
            {
                
                context.ExceptionHandled = true;
                JObject resException = new JObject();
                resException["status"] = 201;
                resException["msg"] = "参数不正确";
                context.Result = new OkObjectResult(resException);
                await Task.CompletedTask;
            }
        }
    }
}
