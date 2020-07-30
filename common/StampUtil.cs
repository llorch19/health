using health.common;
using health.Middleware;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace health
{
    public class StampUtil
    {
        public static string Stamp(HttpContext context)
        {
            string remoteIpAddress = context?.Connection?.RemoteIpAddress?.ToString();
            if (context?.Request?.Headers?.ContainsKey("X-Forwarded-For")??false)
                remoteIpAddress = context.Request.Headers["X-Forwarded-For"];
            return context.GetIdentityInfo<int?>("id") + remoteIpAddress; ;
        }
    }
}
