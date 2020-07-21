/*
 * Title : “随访”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“随访”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class FollowupController : ControllerBase
    {
        private readonly ILogger<FollowupController> _logger;
        dbfactory db = new dbfactory();
        public FollowupController(ILogger<FollowupController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“随访”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“随访”数组</returns>
        [HttpGet]
        [Route("GetOrgFollowupList")]
        public JObject GetOrgFollowupList(int orgid)
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
FROM t_followup
LEFT JOIN t_patient
ON t_followup.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
WHERE t_followup.OrgnizationID=?p1", orgid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“随访”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“随访”数组</returns>
        [HttpGet]
        [Route("GetPersonFollowupList")]
        public JObject GetPersonFollowupList(int personid)
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
FROM t_followup
LEFT JOIN t_patient
ON t_followup.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
WHERE t_followup.PatientID=?p1",personid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“随访”信息，点击[科普公告]中的一个项目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“随访”信息</returns>
        [HttpGet]
        [Route("GetFollowup")]
        public JObject GetFollowup(int id)
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
",id);
            res["person"] = new PersonController(null, null)
                .GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["orgnization"] = new OrgnizationController(null)
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
        public JObject SetFollowup([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ParentID"] = req["parentid"]?.ToObject<int>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["Time"] = req["time"]?.ToObject<DateTime>();
            dict["PersonList"] = req["personlist"]?.ToObject<string>();
            dict["Abstract"] = req["abstract"]?.ToObject<string>();
            dict["Detail"] = req["detail"]?.ToObject<string>();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = HttpContext.User.ToString();
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_attandent", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = HttpContext.User.ToString();
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("t_attandent", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }




        /// <summary>
        /// 删除“随访”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“随访”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelFollowup")]
        public JObject DelFollowup([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            var count = db.del("t_followup", dict);
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