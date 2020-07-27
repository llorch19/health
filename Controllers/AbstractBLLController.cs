/*
 * Title : 个人信息管理控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * -  GetList 和 Get返回 IsDeleted
 *
 */

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
                //  只修改激活状态 Or 只修改业务数据
                if (req.ContainsKey("isactive"))
                {
                    Dictionary<string, object> activeonly = new Dictionary<string, object>();
                    activeonly["id"] = req["id"];  // 指定了id才可以修改

                    dict.Clear();
                    dict["isactive"] = req.ToInt("isactive");
                    this.db.Update(TableName, dict, activeonly);
                    res["id"] = req["id"];
                }
                else
                {
                    Dictionary<string, object> condi = new Dictionary<string, object>();
                    condi["id"] = req["id"];  // 指定了id才可以修改
                    condi["IsDeleted"] = 0;  // 未删除才可以修改
                    dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                    dict["LastUpdatedTime"] = DateTime.Now;
                    var tmp = this.db.Update(TableName, dict, condi);
                    res["id"] = req["id"];
                }
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                dict["IsActive"] = 0;
                dict["IsDeleted"] = 0;
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
            dict["IsActive"] = 0;
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
