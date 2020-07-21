/*
 * Title : “就诊”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“就诊”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class AttandentController : ControllerBase
    {
        private readonly ILogger<AttandentController> _logger;
        dbfactory db = new dbfactory();
        public AttandentController(ILogger<AttandentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“就诊”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetOrgAttandentList")]
        public JObject GetOrgAttandentList(int orgid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"SELECT 
IFNULL(t_attandent.ID, '') AS ID
, IFNULL(PatientID, '') AS PersonID
, IFNUll(t_attandent.OrgnizationID, '') AS OrgnizationID
, IFNULL(t_orgnization.OrgName,'') AS OrgName
, IFNULL(SrcOrgID, '') AS SrcOrgID
, IFNULL(src.OrgName,'') AS SrcOrgName
, IFNULL(DesOrgID, '') AS DesOrgID
, IFNULL(des.OrgName,'') AS DesOrgName
, IFNULL(AdmissionTime, '') AS AdmissionTime
, IFNULL(AdmissionType, '') AS AdmissionType
, IFNULL(IsDischarged, '') AS IsDischarged
, IFNULL(DischargeTime, '') AS DischargeTime
, IFNULL(IsReferral, '') AS IsReferral
, IFNULL(DesStatus, '') AS DesStatus
, IFNULL(DesTime, '') AS DesTime
, IFNULL(IsReferralCancel, '') AS IsReferralCancel
, IFNULL(IsReferralFinish, '') AS IsReferralFinish
FROM t_attandent
LEFT JOIN t_patient
ON t_attandent.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_attandent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_orgnization src
ON t_attandent.SrcOrgID=src.ID
LEFT JOIN t_orgnization des
ON t_attandent.DesOrgID=des.id
WHERE t_attandent.OrgnizationID=?p1", orgid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“就诊”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetPersonAttandentList")]
        public JObject GetPersonAttandentList(int personid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"SELECT 
IFNULL(t_attandent.ID, '') AS ID
, IFNULL(PatientID, '') AS PersonID
, IFNUll(t_attandent.OrgnizationID, '') AS OrgnizationID
, IFNULL(t_orgnization.OrgName,'') AS OrgName
, IFNULL(SrcOrgID, '') AS SrcOrgID
, IFNULL(src.OrgName,'') AS SrcOrgName
, IFNULL(DesOrgID, '') AS DesOrgID
, IFNULL(des.OrgName,'') AS DesOrgName
, IFNULL(AdmissionTime, '') AS AdmissionTime
, IFNULL(AdmissionType, '') AS AdmissionType
, IFNULL(IsDischarged, '') AS IsDischarged
, IFNULL(DischargeTime, '') AS DischargeTime
, IFNULL(IsReferral, '') AS IsReferral
, IFNULL(DesStatus, '') AS DesStatus
, IFNULL(DesTime, '') AS DesTime
, IFNULL(IsReferralCancel, '') AS IsReferralCancel
, IFNULL(IsReferralFinish, '') AS IsReferralFinish
FROM t_attandent
LEFT JOIN t_patient
ON t_attandent.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_attandent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_orgnization src
ON t_attandent.SrcOrgID=src.ID
LEFT JOIN t_orgnization des
ON t_attandent.DesOrgID=des.id
WHERE t_attandent.PatientID=?p1", personid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“就诊”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“就诊”信息</returns>
        [HttpGet]
        [Route("GetAttandent")]
        public JObject GetAttandent(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne(@"SELECT 
IFNULL(ID, '') AS ID
, IFNULL(PatientID, '') AS PersonID
, IFNUll(OrgnizationID, '') AS OrgnizationID
, IFNULL(SrcOrgID, '') AS SrcOrgID
, IFNULL(DesOrgID, '') AS DesOrgID
, IFNULL(AdmissionTime, '') AS AdmissionTime
, IFNULL(AdmissionType, '') AS AdmissionType
, IFNULL(IsDischarged, '') AS IsDischarged
, IFNULL(DischargeTime, '') AS DischargeTime
, IFNULL(IsReferral, '') AS IsReferral
, IFNULL(DesStatus, '') AS DesStatus
, IFNULL(DesTime, '') AS DesTime
, IFNULL(IsReferralCancel, '') AS IsReferralCancel
, IFNULL(IsReferralFinish, '') AS IsReferralFinish
FROM
t_attandent
WHERE ID=?p1", id);

            if (res["id"]!=null)
            {
               
                res["person"] = new PersonController(null,null)
                               .GetPersonInfo(res["personid"].ToObject<int>());
                OrgnizationController org = new OrgnizationController(null);
                res["orgnization"] = org
                    .GetOrgInfo(res["orgnizationid"].ToObject<int>());
                res["srcorg"] = org
                   .GetOrgInfo(res["srcorgid"].ToObject<int>());
                res["desorg"] = org
                   .GetOrgInfo(res["desorgid"].ToObject<int>());
                res["status"] = 200;
                res["msg"] = "读取成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
            }
           
            return res;
        }


        /// <summary>
        /// 更改“就诊”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAttandent")]
        public JObject SetAttandent([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req["personid"]?.ToObject<int>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["SrcOrgID"] = req["srcorgid"]?.ToObject<int>();
            dict["DesOrgID"] = req["desorgid"]?.ToObject<int>();
            dict["AdmissionTime"] = req["admissiontime"]?.ToObject<DateTime>();
            dict["AdmissionType"] = req["admissiontype"]?.ToObject<string>();
            dict["IsDischarged"] = req["isdischarged"]?.ToObject<int>();
            dict["DisChargeTime"] = req["dischargetime"]?.ToObject<DateTime>();
            dict["IsReferral"] = req["isreferral"]?.ToObject<int>();
            dict["DesStatus"] = req["desstatus"]?.ToObject<string>();
            dict["DesTime"] = req["destime"]?.ToObject<DateTime>();
            dict["IsReferralCancel"] = req["isreferralcancel"]?.ToObject<int>();
            dict["IsReferralFinish"] = req["isreferralfinish"]?.ToObject<int>();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_attandent", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
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
        /// 删除“就诊”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAttandent")]
        public JObject DelAttandent([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_attandent", dict);
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