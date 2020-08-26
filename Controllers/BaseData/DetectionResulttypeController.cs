using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class DetectionResultTypeController : AbstractBLLController
    {
        private readonly ILogger<DetectionResultTypeController> _logger;
        public override string TableName => "data_detectionresulttype";

        public DetectionResultTypeController(ILogger<DetectionResultTypeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“检测结果”列表
        /// </summary>
        /// <returns>JSON对象，包含“检测结果”的数组</returns>
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
            JArray rows = db.GetArray(@"
select 
ID
,ResultName
,IFNULL(data_detectionresulttype.control1,'') AS CType
,IFNULL(data_detectionresulttype.control2,'') AS CValue
,IsActive 
from data_detectionresulttype 
where isdeleted=0
");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“检测结果”信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = db.GetOne(@"
select 
ID
,ResultName
,IFNULL(data_detectionresulttype.control1,'') AS CType
,IFNULL(data_detectionresulttype.control2,'') AS CValue
,IsActive 
from data_detectionresulttype 
where id=?p1 
and isdeleted=0", id);
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
        [HttpPost("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“检测结果”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“检测结果”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelDetectionResultType")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetResultTypeInfo(int? id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne(@"
select id
,ResultName text 
,control1 CType
,control2 CValue
from data_detectionresulttype where id=?p1 and isdeleted=0"
, id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["ResultName"] = req["resultname"]?.ToObject<string>();
            dict["control1"] = req["ctype"]?.ToObject<string>();
            dict["control2"] = req["cvalue"]?.ToObject<string>();
            return dict;
        }
    }
}
