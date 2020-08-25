using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.StdResponse
{
    public class Response_401
    {
        public static JObject GetResult(JObject value, string msg = "请登录")
        {
            if (value == null)
                value = new JObject();
            if (value["status"]?.ToObject<string>() != "401")
                value["status"] = 401;
            if (value["msg"]?.ToObject<string>() != msg)
                value["msg"] = msg;
            return value;
        }
    }
}
