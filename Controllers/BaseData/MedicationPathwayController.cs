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
    public class MedicationPathwayController : AbstractBLLController
    {

        private readonly ILogger<MedicationPathwayController> _logger;
        public override string TableName => "data_medicationpathway";

        public MedicationPathwayController(ILogger<MedicationPathwayController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“用药途径”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“用药途径”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,Name,Introduction,IsActive from data_medicationpathway where IsActive=1 and IsDeleted=0");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“用药途径”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“用药途径”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Code,Name,Introduction,IsActive from data_medicationpathway where id=?p1 and IsDeleted=0", id);
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
        /// 修改“用药途径”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“用药途径”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“用药途径”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“用药途径”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetPathwayInfo(int? id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,Name text from data_medicationpathway where id=?p1 and IsDeleted=0", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();
            dict["Introduction"] = req["introduction"]?.ToObject<string>();


            return dict;
        }
    }
}
