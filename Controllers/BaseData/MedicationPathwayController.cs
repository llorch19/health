using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class MedicationPathwayController : ControllerBase
    {

        private readonly ILogger<MedicationPathwayController> _logger;

        public MedicationPathwayController(ILogger<MedicationPathwayController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“用药途径”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“用药途径”数组</returns>
        [HttpGet]
        [Route("GetMedicationPathwayList")]
        public JObject GetMedicationPathwayList(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,Name,Introduction from data_medicationpathway");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“用药途径”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“用药途径”信息</returns>
        [HttpGet]
        [Route("GetMedicationPathway")]
        public JObject GetMedicationPathway(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Code,Name,Introduction from data_medicationpathway where id=?p1", id);
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
        [HttpPost("SetMedicationPathway")]
        public JObject SetMedicationPathway([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("data_medicationpathway", dict);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "新增成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "无法新增数据";
                    }
                }
                else if (id > 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    dict.Remove("id");
                    var keys = new Dictionary<string, object>();
                    keys["id"] = req["id"];
                    var rows = db.Update("data_medicationpathway", dict, keys);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "修改成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "修改失败";
                    }
                }
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "非法的请求";
            }
            return res;
        }


        /// <summary>
        /// 删除“用药途径”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“用药途径”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelMedicationPathway")]
        public JObject DelMedicationPathway([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_medicationpathway", dict);
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
