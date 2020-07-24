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
    public class GenderController : AbstractBLLController
    {
        private readonly ILogger<GenderController> _logger;
        public override string TableName => "data_gender";

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
        public override JObject GetList()
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
        public override JObject Get(int id)
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
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“性别”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“性别”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelGender")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetGenderInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,GenderName text from data_gender where id=?p1", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["GenderName"] = req["gendername"]?.ToObject<string>();

            return dict;
        }
    }
}
