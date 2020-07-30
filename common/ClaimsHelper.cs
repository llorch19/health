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
        public static Dictionary<string,string> GetRequestClaims(this HttpContext httpContext)
        {
            return httpContext.User.Claims
                .ToDictionary<Claim,string,string>(claim=>claim.Type,claim=>claim.Value);
        }
        public static JObject GetIdentity(this HttpContext httpContext,string entry, Func<dbfactory,object[],JObject> getIdentityFunc, Func<HttpContext, object[]> getIdentityArgsFunc)
        {
            config conf = new config();
            dbfactory db = new dbfactory();
            IMemoryCache memoryCache = httpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            int slide = int.Parse(conf.GetValue("sys:memorycache:SlidingExpiration"));
            int absolute = int.Parse(conf.GetValue("sys:memorycache:AbsoluteExpiration"));
            var funcArgs = getIdentityArgsFunc(httpContext);
            var nEntry = entry + string.Join('_',funcArgs);
            return memoryCache.GetOrCreate<JObject>(
                    nEntry
                    , e => {
                        e.SlidingExpiration = TimeSpan.FromMinutes(slide);
                        e.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(absolute);
                        JObject newCache = new JObject();
                        
                        newCache = getIdentityFunc(db,funcArgs);
                        return (JObject)newCache.DeepClone();
                    }
                );
        }
        public static T GetIdentityInfo<T>(this HttpContext httpContext, string key)
        {
            var identity = httpContext.GetIdentity(
                ConstFunc.IdentityEntry
                ,ConstFunc.GetIdentityFunc
                ,ConstFunc.GetIdentityArgsFunc);
            ILogger logger = httpContext.RequestServices.GetService(typeof(ILogger)) as ILogger;
            try
            {
                string s = identity[key]?.ToObject<string>();
                var makeNullableType = Nullable.GetUnderlyingType(typeof(T));
                if (makeNullableType == null)
                    return (T)System.Convert.ChangeType(s, typeof(T));
                else
                    return (T)System.Convert.ChangeType(s, makeNullableType);
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex.Message);
                return default(T);
            }
        }
    }
}
