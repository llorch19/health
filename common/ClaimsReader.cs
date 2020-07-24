using health.Controllers;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using util;
using util.mysql;

namespace health.common
{
    public static class ClaimsReader
    {
        /// <summary>
        /// 扩展httpContext读取用户信息，获取的用户信息缓存在内存中
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static JObject GetUser(this HttpContext httpContext)
        {
            int id = 0;
            string userid =
                 httpContext.User
                .Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
                ?.Value;
            config conf = new config();
            dbfactory db = new dbfactory();
            IMemoryCache memoryCache = httpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            string controllername = httpContext.GetRouteData().Values["controller"].ToString();
            if (int.TryParse(userid,out id))
            {
                int slide = int.Parse(conf.GetValue("sys:memorycache:SlidingExpiration"));
                int absolute = int.Parse(conf.GetValue("sys:memorycache:AbsoluteExpiration"));
                string entry = controllername + id;
                return memoryCache.GetOrCreate<JObject>(
                    entry
                    , e => {
                        e.SlidingExpiration = TimeSpan.FromMinutes(slide);
                        e.AbsoluteExpiration =DateTimeOffset.UtcNow.AddMinutes(absolute);
                        JObject newCache = new JObject();
                        newCache = db.GetOne(@"SELECT id,OrgnizationID,GroupId,ProvinceID,CityID,CountyID FROM t_user where id=?p1", id);
                        return (JObject)newCache.DeepClone();
                    }
                );
            }
            else
            {
                return new JObject();
            }
        }
    }
}
