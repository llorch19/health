using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class ModelValidateFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 删除apicontroller修饰符才可起效
            if (!context.ModelState.IsValid)
            {
                var ex = context.HttpContext.RequestServices.GetService(typeof(ModelException)) as ModelException;
                throw ex;
            }
            
        }
    }
}
