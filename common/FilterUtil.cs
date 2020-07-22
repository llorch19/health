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
            return context?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}
