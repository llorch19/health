using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.Controllers
{
    public abstract class AbstractBLLController : ControllerBase
    {
        protected dbfactory db = new dbfactory();

        public abstract string TableName { get; }

        public abstract JObject GetList();

        public abstract JObject Get(int id);

        [NonAction]
        public abstract Dictionary<string, object> GetReq(JObject req);

        public virtual JObject Set(JObject req)
        {
            JObject res = new JObject();
            var dict = GetReq(req);
            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update(TableName, dict, condi);
                res["id"] = req["id"];
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                res["id"] = this.db.Insert(TableName, dict);
            }

            
            res["status"] = 200;
            res["msg"] = "提交成功";
            return res;
        }

        public virtual JObject Del(JObject req)
        {
            JObject res = new JObject();
            var dict = new Dictionary<string, object>();
            dict["IsDeleted"] = 1;
            var keys = new Dictionary<string, object>();
            keys["id"] = req["id"]?.ToObject<int>();
            var count = db.Update(TableName, dict, keys);
            if (count > 0)
            {
                res["status"] = 200;
                res["msg"] = "操作成功";
                return res;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "操作失败";
                return res;
            }
        }
    }
}
