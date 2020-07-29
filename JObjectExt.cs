using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health
{
    public static class JObjectExt
    {
        public static object ToDateTime(this JObject jobj,string prop)
        {
            string data = jobj[prop]?.ToObject<string>();
            if (jobj[prop]==null 
                || string.IsNullOrWhiteSpace(data)
                || string.IsNullOrEmpty(data)
                )
                return null;

            DateTime dtOut;
            if (DateTime.TryParse(data, out dtOut))
                return dtOut;

            return null;
        }


        public static int? ToInt(this JObject jobj, string prop)
        {
            string data = jobj[prop]?.ToObject<string>();
            if (jobj[prop] == null
                || string.IsNullOrWhiteSpace(data)
                || string.IsNullOrEmpty(data)
                )
                return null;

            if (data.ToUpperInvariant()=="TRUE")
            {
                return 1;
            }

            if (data.ToUpperInvariant() == "FALSE")
            {
                return 0;
            }

            int dtOut;
            if (int.TryParse(data, out dtOut))
                return dtOut;

            return null;
        }
    }
}
