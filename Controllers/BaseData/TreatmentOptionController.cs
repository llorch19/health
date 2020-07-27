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
    public class TreatmentOptionController : AbstractBLLController
    {

        private readonly ILogger<TreatmentOptionController> _logger;
        public override string TableName => "data_treatmentoption";

        public TreatmentOptionController(ILogger<TreatmentOptionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“治疗方案”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“治疗方案”数组</returns>
        [HttpGet]
        [Route("GetTreatmentOptionList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            JArray rows = db.GetArray("select ID,Name,Introduction,IsActive from data_treatmentoption where  IsDeleted=0");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“治疗方案”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“治疗方案”信息</returns>
        [HttpGet]
        [Route("GetTreatmentOption")]
        public override JObject Get(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = db.GetOne("select ID,Name,Introduction,IsActive from data_treatmentoption where id=?p1 and IsDeleted=0", id);
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
        /// 修改“治疗方案”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“治疗方案”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetTreatmentOption")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“治疗方案”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“治疗方案”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelTreatmentOption")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetTreatOptionInfo(int? id)
        {
            JObject res = db.GetOne("select id,Name text from data_treatmentoption where id=?p1 and IsDeleted=0", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Introduction"] = req["introduction"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();


            return dict;
        }
    }
}
