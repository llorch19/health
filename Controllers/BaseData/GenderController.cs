using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class GenderController : ControllerBase
    {
        private readonly ILogger<GenderController> _logger;
        dbfactory db = new dbfactory();
        public GenderController(ILogger<GenderController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“性别”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“性别”数组</returns>
        [HttpGet]
        [Route("GetGenderList")]
        public JObject GetGenderList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select id,Code,GenderName from data_gender");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“性别”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“性别”信息</returns>
        [HttpGet]
        [Route("GetGender")]
        public JObject GetGender(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,Code,GenderName from data_gender where id=?p1", id);
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
        /// 修改“性别”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“性别”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetGender")]
        public JObject SetGender([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["GenderName"] = req["gendername"]?.ToObject<string>();


            if (req["id"].ToObject<int>() > 0)
            {
                dict["LastUpdatedBy"] = HttpContext.User.ToString();
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                var tmp = this.db.Update("data_domitype", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = HttpContext.User.ToString();
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("data_domitype", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }


        /// <summary>
        /// 删除“性别”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“性别”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelGender")]
        public JObject DelGender([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_gender", dict);
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
        public JObject GetGenderInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,GenderName text from data_gender where id=?p1", id);
            return res;
        }
    }
}
