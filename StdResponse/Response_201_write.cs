using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.StdResponse
{
    public class Response_201_write
    {
        public static JObject GetResult(JObject value=null, string msg = "无法更新相应的数据")
        {
            if (value==null)
                value = new JObject();
            if (value["status"]?.ToObject<string>() != "201")
                value["status"] = 201;
            if (value["msg"]?.ToObject<string>() != msg)
                value["msg"] = msg;
            return value;
        }
    }
}
