using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                var _logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<ZFExceptionFilter>)) as ILogger<ZFExceptionFilter>;
                _logger.LogError(context.ActionDescriptor.AttributeRouteInfo.Template);
                _logger.LogError(context.Exception.Message);
                context.ExceptionHandled = true;
                JObject resException = new JObject();
                resException["status"] = 201;
                resException["msg"] = "发生错误，请联系管理员。";
                context.Result = new OkObjectResult(resException);
                await Task.CompletedTask;
            }
        }
    }
}
