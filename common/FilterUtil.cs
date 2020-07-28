using health.common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace health
{
    public class FilterUtil
    {
        public static string GetUser(HttpContext context)
        {
            string remoteIpAddress = context?.Connection?.RemoteIpAddress?.ToString();
            if (context?.Request?.Headers?.ContainsKey("X-Forwarded-For")??false)
                remoteIpAddress = context.Request.Headers["X-Forwarded-For"];
            return context.GetUserInfo<int>("id") + remoteIpAddress; ;
        }


        public static string GetPerson(HttpContext context)
        {
            string remoteIpAddress = context?.Connection?.RemoteIpAddress?.ToString();
            if (context?.Request?.Headers?.ContainsKey("X-Forwarded-For") ?? false)
                remoteIpAddress = context.Request.Headers["X-Forwarded-For"];
            return context.GetPersonInfo<int>("id")+remoteIpAddress;
        }
    }
}
