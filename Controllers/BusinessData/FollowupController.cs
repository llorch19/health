/*
 * Title : “随访”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“随访”信息的增删查改
 * Comments
 */
using health.common;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class FollowupController : AbstractBLLController
    {
        private readonly ILogger<FollowupController> _logger;
        public override string TableName => "t_followup";

        public FollowupController(ILogger<FollowupController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“随访”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“随访”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            JObject res = GetListImp();
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JObject GetListImp()
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_followup.ID
,t_followup.PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonCode
,t_followup.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,t_followup.TIME
,t_followup.PersonList
,t_followup.Abstract
,t_followup.Detail
,t_followup.IsActive
FROM t_followup
LEFT JOIN t_patient
ON t_followup.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
WHERE t_followup.OrgnizationID=?p1
AND t_followup.IsDeleted=0", HttpContext.GetIdentityInfo<int?>("orgnizationid"));
            return res;
        }

        /// <summary>
        /// 获取机构的“随访”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“随访”数组</returns>
        [HttpGet]
        [Route("Get[controller]ListP")]
        public JObject GetListP(int personid)
        {
            JObject res = GetListPImp(personid);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JObject GetListPImp(int personid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_followup.ID
,t_followup.PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonCode
,t_followup.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,t_followup.TIME
,t_followup.PersonList
,t_followup.Abstract
,t_followup.Detail
,t_followup.IsActive
FROM t_followup
LEFT JOIN t_patient
ON t_followup.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
WHERE t_followup.PatientID=?p1
AND t_followup.IsDeleted=0", personid);
            return res;
        }

        /// <summary>
        /// 获取“随访”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“随访”信息</returns>
        [HttpGet]
        [Route("GetFollowup")]
        public override JObject Get(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
ID
,PatientID
,OrgnizationID
,TIME
,PersonList
,Abstract
,Detail
FROM t_followup
WHERE ID=?p1
AND t_followup.IsDeleted=0
", id);
            res["person"] = new PersonController(null, null)
                .GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["orgnization"] = new OrganizationController(null)
                .GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 更改“随访”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“随访”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetFollowup")]
        public override JObject Set([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Set(req);
        }




        /// <summary>
        /// 删除“随访”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“随访”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelFollowup")]
        public override JObject Del([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Del(req);
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {

            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req.ToInt("patientid");
            dict["OrgnizationID"] = req.ToInt("orgnizationid");
            dict["Time"] = req.ToDateTime("time");
            dict["PersonList"] = req["personlist"]?.ToObject<string>();
            dict["Abstract"] = req["abstract"]?.ToObject<string>();
            dict["Detail"] = req["detail"]?.ToObject<string>();


            return dict;
        }
    }
}