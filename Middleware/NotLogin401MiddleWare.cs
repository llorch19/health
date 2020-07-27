using health.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class NotLogin401MiddleWare:AbstractMiddleware
    {
        public NotLogin401MiddleWare(RequestDelegate next):base(next)
        {
        }

        public override async Task InvokeAsync(HttpContext context)
        {

            var controller = context.GetRouteValue("controller");
            List<string> whitelist = new List<string>() { "Login" };
            if (controller==null || (!whitelist.Contains(controller.ToString()) && context.GetUser()["id"] == null))
            {
                JObject res = new JObject();
                res["status"] = 401;
                res["msg"] = "鉴权失败";
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json;charset=utf-8";
                await context.Response.WriteAsync(
                    JsonConvert.SerializeObject(res)
                    );
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
        }
    }
}
