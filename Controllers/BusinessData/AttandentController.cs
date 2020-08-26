/*
 * Title : “就诊”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“就诊”信息的增删查改
 * Comments
 */
using health.common;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class AttandentController : AbstractBLLController
    {
        private readonly ILogger<AttandentController> _logger;
        OrganizationController _org;
        PersonController _person;
        public override string TableName => "t_attandent";

        public AttandentController(ILogger<AttandentController> logger
            ,OrganizationController org
            ,PersonController person)
        {
            _logger = logger;
            _org = org;
            _person = person;
        }

        /// <summary>
        /// 获取机构的“就诊”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetAttandentList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = GetListImp();
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JArray GetListImp()
        {
            return db.GetArray(@"
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
AND t_attandent.IsDeleted=0", HttpContext.GetIdentityInfo<int?>("orgnizationid"));
        }

        /// <summary>
        /// 获取个人的“就诊”列表
        /// </summary>
        /// <param name="personid">请求的个人id</param>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetAttandentListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"]=GetListPImp(personid);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JArray GetListPImp(int personid)
        {
            return db.GetArray(@"
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
WHERE t_attandent.PatientID=?p1
AND t_attandent.IsDeleted=0", personid);
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

            var canread = res.Challenge(r=>r["id"]!=null);
            if (canread)
                return Response_201_read.GetResult();

            res["person"] = _person.GetPersonInfo(res["personid"].ToObject<int>());
            res["orgnization"] = _org.GetOrgInfo(res["orgnizationid"].ToObject<int>());
            res["srcorg"] = _org.GetOrgInfo(res["srcorgid"].ToObject<int>());
            res["desorg"] = _org.GetOrgInfo(res["desorgid"].ToObject<int>());

            return Response_200_read.GetResult(res);
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


            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var id = req.ToInt("id");
            if (id == 0) // 新增
                req["orgnizationid"] = orgid;
            else
            {
                var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
                if (!canwrite)
                    return Response_201_write.GetResult();
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
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var objDatabase = db.GetOne("SELECT OrgnizationID FROM " + TableName + " WHERE ID=?p1 AND IsDeleted=0", id);
            var canwrite = req.Challenge(r => objDatabase.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();
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