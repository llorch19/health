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
    public class MedicationFreqCategoryController : AbstractBLLController
    {

        private readonly ILogger<MedicationFreqCategoryController> _logger;
        public override string TableName => "data_medicationfreqcategory";

        public MedicationFreqCategoryController(ILogger<MedicationFreqCategoryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“用药频次”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“用药频次”数组</returns>
        [HttpGet]
        [Route("GetMedicationFreqCategoryList")]
        public override JObject GetList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,Value,ValueMessage from data_medicationfreqcategory where IsActive=1 and IsDeleted=0");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“用药频次”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“用药频次”信息</returns>
        [HttpGet]
        [Route("GetMedicationFreqCategory")]
        public override JObject Get(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Code,Value,ValueMessage from data_medicationfreqcategory where id=?p1 and IsDeleted=0", id);
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
        /// 修改“用药频次”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“用药频次”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetMedicationFreqCategory")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“用药频次”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“用药频次”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelMedicationFreqCategory")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetFreqInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,ValueMessage text from data_medicationfreqcategory where id=?p1 and IsDeleted=0", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["Value"] = req["value"]?.ToObject<string>();
            dict["ValueMessage"] = req["valuemessage"]?.ToObject<string>();


            return dict;
        }
    }
}
