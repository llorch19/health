using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class TreatmentOptionController : AbstractBLLController
    {

        private readonly ILogger<TreatmentOptionController> _logger;
        DetectionResultTypeController _rtype;
        public override string TableName => "data_treatmentoption";

        public TreatmentOptionController(ILogger<TreatmentOptionController> logger
            , DetectionResultTypeController rtype)
        {
            _logger = logger;
            _rtype = rtype;
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
            res["list"] = db.GetArray(@"
SELECT 
data_treatmentoption.ID
,Name
,data_treatmentoption.ResultTypeID
,data_detectionresulttype.ResultName AS ResultType
,data_treatmentoption.IsActive 
,Introduction
FROM data_treatmentoption 
LEFT JOIN data_detectionresulttype
ON data_treatmentoption.ResultTypeID=data_detectionresulttype.ID
WHERE  data_treatmentoption.IsDeleted=0
");
            return Response_200_read.GetResult(res);
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
            JObject res = db.GetOne(@"
select ID,Name,ResultTypeID,Introduction,IsActive from data_treatmentoption where id=?p1 and IsDeleted=0
", id);
            res["resulttype"] = _rtype.GetResultTypeInfo(res.ToInt("resulttypeid"));
            var canread = res.Challenge(r=>r["id"]!=null);
            if (!canread)
                return Response_201_read.GetResult();
            else
                return Response_200_read.GetResult(res);
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
            JObject res = db.GetOne(@"
select data_treatmentoption.id,Name text ,ResultName
from data_treatmentoption 
inner join data_detectionresulttype
on data_treatmentoption.ResultTypeID = data_detectionresulttype.ID
where data_treatmentoption.id=?p1 and data_treatmentoption.IsDeleted=0
", id);
            return res;
        }

        [NonAction]
        public JArray GetTreatOptionInfoArray(int[] idArray)
        {
            JArray array = new JArray();
            for (int i = 0; i < idArray.Length; i++)
            {
                var to = GetTreatOptionInfo(idArray[i]);
                if (to.HasValues)
                {
                    array.Add(to);
                }
            }
            return array;
        }

        [NonAction]
        public JArray GetTreatOptionInfoList(int? resultid)
        {
            var res = db.GetArray(@"
SELECT ID,Name Text,Introduction Intro FROM data_treatmentoption
WHERE data_treatmentoption.ID
IN (SELECT TreatmentOptionID FROM rel_resulttreat WHERE rel_resulttreat.DetectionResultTypeID=?p1 AND IsDeleted=0)
AND data_treatmentoption.IsDeleted=0
", resultid);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Introduction"] = req["introduction"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();
            dict["ResultTypeID"] = req.ToInt("resulttypeid");

            return dict;
        }
    }
}
