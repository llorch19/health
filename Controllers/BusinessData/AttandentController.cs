/*
 * Title : “就诊”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“就诊”信息的增删查改
 * Comments
 */
using health.common;
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
    [Route("api")]
    public class AttandentController : AbstractBLLController
    {
        private readonly ILogger<AttandentController> _logger;
        public override string TableName => "t_attandent";

        public AttandentController(ILogger<AttandentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“就诊”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetOrgAttandentList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
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
, IFNULL(t_attandent.IsActive,'') AS IsActive
FROM t_attandent
LEFT JOIN t_patient
ON t_attandent.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_attandent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_orgnization src
ON t_attandent.SrcOrgID=src.ID
LEFT JOIN t_orgnization des
ON t_attandent.DesOrgID=des.id
WHERE t_attandent.OrgnizationID=?p1
AND t_attandent.IsDeleted=0", HttpContext.GetUserInfo<int>("orgnizationid"));
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“就诊”历史
        /// </summary>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetPersonAttandentList")]
        public JObject GetPersonAttandentList()
        {
            int? personid = HttpContext.GetPersonInfo<int?>("id");
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
, IFNULL(t_attandent.IsActive,'') AS IsActive
FROM t_attandent
LEFT JOIN t_patient
ON t_attandent.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_attandent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_orgnization src
ON t_attandent.SrcOrgID=src.ID
LEFT JOIN t_orgnization des
ON t_attandent.DesOrgID=des.id
WHERE t_attandent.PatientID=?p1
AND t_attandent.IsDeleted=0
AND t_attandent.IsActive=1", personid);
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
        public override JObject Get(int id)
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
, IFNULL(t_attandent.IsActive,'') AS IsActive
FROM
t_attandent
WHERE ID=?p1
AND t_attandent.IsDeleted=0", id);

            if (res["id"]!=null)
            {
               
                res["person"] = new PersonController(null,null)
                               .GetPersonInfo(res["personid"].ToObject<int>());
                OrganizationController org = new OrganizationController(null);
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
        public override JObject Set([FromBody] JObject req)
        {
            // 如果存在未转诊及出院信息，不允许新增就诊记录
            JObject attand = db.GetOne(@"
SELECT ID,IsDischarged,IsReferral,IsReferralFinish FROM t_attandent
WHERE ID=?p1
AND IsActive=1
AND IsDeleted=0
",req.ToInt("id"));
            if (attand["id"]!=null)
            {
                // 存在只更新
            }
            return base.Set(req);
        }




        /// <summary>
        /// 删除“就诊”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAttandent")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req.ToInt("personid");
            dict["OrgnizationID"] = req.ToInt("orgnizationid");
            dict["SrcOrgID"] = req.ToInt("srcorgid");
            dict["DesOrgID"] = req.ToInt("desorgid");
            dict["AdmissionTime"] = req.ToDateTime("admissiontime");
            dict["AdmissionType"] = req["admissiontype"]?.ToObject<string>();
            dict["IsDischarged"] = req.ToInt("isdischarged");
            dict["DisChargeTime"] = req.ToDateTime("dischargetime");
            dict["IsReferral"] = req.ToInt("isreferral");
            dict["DesStatus"] = req["desstatus"]?.ToObject<string>();
            dict["DesTime"] = req.ToDateTime("destime");
            dict["IsReferralCancel"] = req.ToInt("isreferralcancel");
            dict["IsReferralFinish"] = req.ToInt("isreferralfinish");


            return dict;
        }
    }
}