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
    public class ModelInvalid201Filter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 删除apicontroller修饰符才可起效
            if (!context.ModelState.IsValid)
            {
                JObject res = new JObject();
                res["status"] = 201;
                res["msg"] = "访问错误，请更换请求参数后再试。";
                context.Result = new OkObjectResult(res);
            }
        }
    }
}
