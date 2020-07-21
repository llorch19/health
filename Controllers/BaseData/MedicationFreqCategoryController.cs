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
    public class MedicationFreqCategoryController : ControllerBase
    {

        private readonly ILogger<MedicationFreqCategoryController> _logger;
        dbfactory db = new dbfactory();
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
        public JObject GetMedicationFreqCategoryList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,Value,ValueMessage from data_medicationfreqcategory");

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
        public JObject GetMedicationFreqCategory(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Code,Value,ValueMessage from data_medicationfreqcategory where id=?p1", id);
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
        public JObject SetMedicationFreqCategory([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["Value"] = req["value"]?.ToObject<string>();
            dict["ValueMessage"] = req["valuemessage"]?.ToObject<string>();

            if (req["id"].ToObject<int>() > 0)
            {
                dict["LastUpdatedBy"] = HttpContext.Connection.RemoteIpAddress.ToString();
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                var tmp = this.db.Update("data_medicationfreqcategory", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = HttpContext.Connection.RemoteIpAddress.ToString();
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("data_medicationfreqcategory", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }


        /// <summary>
        /// 删除“用药频次”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“用药频次”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelMedicationFreqCategory")]
        public JObject DelMedicationFreqCategory([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_medicationfreqcategory", dict);
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
        public JObject GetFreqInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,ValueMessage text from data_medicationfreqcategory where id=?p1", id);
            return res;
        }
    }
}
