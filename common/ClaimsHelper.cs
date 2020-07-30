using health.Controllers;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509.Qualified;
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
    public static class ClaimsHelper
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

            string role =
                 httpContext.User
                .Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.Role
                )
                ?.Value;
            config conf = new config();
            dbfactory db = new dbfactory();
            IMemoryCache memoryCache = httpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            string prefix = "SysUser_";
            if (int.TryParse(userid,out id) && role=="user")
            {
                int slide = int.Parse(conf.GetValue("sys:memorycache:SlidingExpiration"));
                int absolute = int.Parse(conf.GetValue("sys:memorycache:AbsoluteExpiration"));
                string entry = prefix + id;
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

        public static T GetUserInfo<T>(this HttpContext httpContext,string key)
        {
            var user = GetUser(httpContext);
            ILogger logger = httpContext.RequestServices.GetService(typeof(ILogger)) as ILogger;
            try
            {
                string s = user[key]?.ToObject<string>();
                var nullabletype=Nullable.GetUnderlyingType(typeof(T));
                if (nullabletype==null)
                    return (T)System.Convert.ChangeType(s, typeof(T));
                else
                    return (T)System.Convert.ChangeType(s, nullabletype);
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex.Message);
                return default(T);
            }
        }



        /// <summary>
        /// 扩展httpContext读取用户信息，获取的用户信息缓存在内存中
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static JObject GetPerson(this HttpContext httpContext)
        {
            int id = 0;
            string userid =
                 httpContext.User
                .Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier
                )
                ?.Value;
            string role=
                 httpContext.User
                .Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.Role
                )
                ?.Value;
            config conf = new config();
            dbfactory db = new dbfactory();
            IMemoryCache memoryCache = httpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            string prefix = "SysPerson_";
            if (int.TryParse(userid, out id) && role=="person")
            {
                int slide = int.Parse(conf.GetValue("sys:memorycache:SlidingExpiration"));
                int absolute = int.Parse(conf.GetValue("sys:memorycache:AbsoluteExpiration"));
                string entry = prefix + id;
                return memoryCache.GetOrCreate<JObject>(
                    entry
                    , e => {
                        e.SlidingExpiration = TimeSpan.FromMinutes(slide);
                        e.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(absolute);
                        JObject newCache = new JObject();
                        newCache = db.GetOne(@"SELECT id,OrgnizationID,PrimaryOrgnizationID,IDCardNO,IDCategoryID,OccupationCategoryID,AddressCategoryID,ProvinceID,CityID,CountyID FROM t_patient where id=?p1", id);
                        return (JObject)newCache.DeepClone();
                    }
                );
            }
            else
            {
                return new JObject();
            }
        }


        public static T GetPersonInfo<T>(this HttpContext httpContext, string key)
        {
            var user = GetPerson(httpContext);
            return user.ContainsKey(key)
                ?user[key].ToObject<T>()
                :default(T);
        }


        public static string GetRole(this HttpContext httpContext)
        {
            string role = httpContext
                .User
                .Claims
                .FirstOrDefault(claim=>claim.Type==ClaimTypes.Role)?.Value;
            return role;
        }
    }
}
