using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.StdResponse
{
    public class Response_200_read 
    {
        public static JObject GetResult(JObject value,string msg="读取成功")
        {
            if (value == null)
                value = new JObject();
            if (value["status"]?.ToObject<string>()!="200")
                value["status"] = 200;
            if (value["msg"]?.ToObject<string>() != msg)
                value["msg"] = msg;
            return value;
        }
    }
}
