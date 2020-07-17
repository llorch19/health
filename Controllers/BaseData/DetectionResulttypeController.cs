using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class DetectionResultTypeController : ControllerBase
    {
        private readonly ILogger<DetectionResultTypeController> _logger;
        public DetectionResultTypeController(ILogger<DetectionResultTypeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“检测结果”列表
        /// </summary>
        /// <returns>JSON对象，包含“检测结果”的数组</returns>
        [HttpGet]
        [Route("GetDetectionResultTypeList")]
        public JObject GetDetectionResultTypeList(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,ResultName from data_detectionresulttype");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“检测结果”信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDetectionResultType")]
        public JObject GetDetectionResultType(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,ResultName from data_detectionresulttype where id=?p1", id);
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
        /// 修改“检测结果”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“检测结果”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetDetectionResultType")]
        public JObject SetDetectionResultType([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("data_detectionresulttype", dict);
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
                    var rows = db.Update("data_detectionresulttype", dict, keys);
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
        /// 删除“检测结果”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“检测结果”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelDetectionResultType")]
        public JObject DelDetectionResultType([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_detectionresulttype", dict);
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
        public JObject GetResultTypeInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,ResultName text from data_detectionresulttype where id=?p1", id);
            return res;
        }
    }
}
