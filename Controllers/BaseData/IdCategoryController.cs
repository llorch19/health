using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class IdCategoryController : ControllerBase
    {

        private readonly ILogger<IdCategoryController> _logger;
        dbfactory db = new dbfactory();
        public IdCategoryController(ILogger<IdCategoryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“身份证件类型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“身份证件类型”数组</returns>
        [HttpGet]
        [Route("GetIdCategoryList")]
        public JObject GetIdCategoryList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            JArray rows = db.GetArray(@"
select id,Code,Name from data_idcategory
");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“身份证件类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“身份证件类型”信息</returns>
        [HttpGet]
        [Route("GetIdCategory")]
        public JObject GetIdCategory(int id)
        {
            JObject res = db.GetOne("select id,Code,Name from data_idcategory where id=?p1", id);
            if (res["id"] != null)
            {
                res["status"] = 200;
                res["msg"] = "读取成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }
            return res;
        }

        /// <summary>
        /// 修改“身份证件类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“身份证件类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetIdCategory")]
        public JObject SetIdCategory([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();


            if (req["id"].ToObject<int>() > 0)
            {
                dict["LastUpdatedBy"] = HttpContext.Connection.RemoteIpAddress.ToString();
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                var tmp = this.db.Update("data_idcategory", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = HttpContext.Connection.RemoteIpAddress.ToString();
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("data_idcategory", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }



        /// <summary>
        /// 删除“身份证件类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“身份证件类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelIdCategory")]
        public JObject DelIdCategory([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = new Dictionary<string, object>();
            dict["IsDeleted"] = 1;
            var keys = new Dictionary<string, object>();
            keys["id"] = req["id"]?.ToObject<int>();
            var count = db.Update("data_idcategory", dict, keys);
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

        [NonAction]
        public JObject GetIdCategoryInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,Name text from data_idcategory where id=?p1", id);
            return res;
        }
    }
}
