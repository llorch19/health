/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“接种记录”信息的增删查改
 * Comments
 * - GetOrgVaccList 应该和GetPeron["vacc"]字段一致     @xuedi      2020-07-22      15:20
 */
using health.common;
using health.Controllers.BaseData;
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
    public class VaccController : AbstractBLLController
    {
        private readonly ILogger<VaccController> _logger;
        public override string TableName => "t_vacc";

        public VaccController(ILogger<VaccController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“接种记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("GetOrgVaccList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_vacc.ID
,PatientID
,t_patient.FamilyName AS Person
,t_vacc.OrgnizationID
,t_orgnization.OrgName AS OrgName
,OperationUserID
,t_user.ChineseName AS Operator
,MedicationID
,t_medication.`Name` AS Medication
,t_medication.`CommonName` AS CommonName
,MedicationDosageFormID
,data_medicationdosageform.`Name` AS Dosage
,MedicationPathwayID
,data_medicationpathway.`Name` AS Pathway
,Ftime
,OperationTime
,LeaveTime
,NextTime
,Fstatus
,TempratureP
,TempratureN
,Effect
,t_vacc.IsActive AS IsActive
FROM t_vacc
LEFT JOIN t_patient
ON t_vacc.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_vacc.OrgnizationID=t_orgnization.ID
LEFT JOIN t_user
ON t_vacc.OperationUserID=t_user.ID
LEFT JOIN t_medication
ON t_vacc.MedicationID=t_medication.ID
LEFT JOIN data_medicationdosageform
ON t_vacc.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_vacc.MedicationPathwayID=data_medicationpathway.ID
WHERE t_vacc.OrgnizationID=?p1
AND t_vacc.IsDeleted=0", HttpContext.GetUserInfo<int>("orgnizationid"));
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“接种记录”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("GetPersonVaccList")]
        public JObject GetPersonVaccList()
        {
            int personid = HttpContext.GetPersonInfo<int>("id");
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_vacc.ID
,PatientID
,t_patient.FamilyName AS Person
,t_vacc.OrgnizationID
,t_orgnization.OrgName AS OrgName
,OperationUserID
,t_user.ChineseName AS Operator
,MedicationID
,t_medication.`Name` AS Medication
,t_medication.`CommonName` AS CommonName
,MedicationDosageFormID
,data_medicationdosageform.`Name` AS Dosage
,MedicationPathwayID
,data_medicationpathway.`Name` AS Pathway
,Ftime
,OperationTime
,LeaveTime
,NextTime
,Fstatus
,TempratureP
,TempratureN
,Effect
,t_vacc.IsActive AS IsActive
FROM t_vacc
LEFT JOIN t_patient
ON t_vacc.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_vacc.OrgnizationID=t_orgnization.ID
LEFT JOIN t_user
ON t_vacc.OperationUserID=t_user.ID
LEFT JOIN t_medication
ON t_vacc.MedicationID=t_medication.ID
LEFT JOIN data_medicationdosageform
ON t_vacc.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_vacc.MedicationPathwayID=data_medicationpathway.ID
WHERE t_vacc.PatientID=?p1
AND t_vacc.IsDeleted=0", personid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“接种记录”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”信息</returns>
        [HttpGet]
        [Route("GetVacc")]
        public override JObject Get(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
ID
,PatientID
,OrgnizationID
,OperationUserID
,MedicationID
,MedicationDosageFormID
,MedicationPathwayID
,Ftime
,OperationTime
,LeaveTime
,NextTime
,Fstatus
,TempratureP
,TempratureN
,Effect
,IsActive
FROM t_vacc
WHERE ID=?p1
and IsDeleted=0", id);
            PersonController person = new PersonController(null,null);
            res["person"] = person
                .GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["org"] = new OrganizationController(null)
                .GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["operator"] = person
                .GetUserInfo(res["operationuserid"]?.ToObject<int>() ?? 0);
            res["medication"] = new MedicationController(null)
                .GetMedicationInfo(res["medicationid"]?.ToObject<int>() ?? 0);
            res["dosage"] = new MedicationDosageFormController(null)
                .GetDosageInfo(res["medicationdosageformid"]?.ToObject<int>() ?? 0);
            res["pathway"] = new MedicationPathwayController(null)
                .GetPathwayInfo(res["medicationpathwayid"]?.ToObject<int>() ?? 0);

            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 更改“接种记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetVacc")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }




        /// <summary>
        /// 删除“接种记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelVacc")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req["patientid"]?.ToObject<int>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["OperationUserID"] = req["operationuserid"]?.ToObject<int>();
            dict["MedicationID"] = req["medicationid"]?.ToObject<int>();
            dict["MedicationDosageFormID"] = req["medicationdosageformid"]?.ToObject<int>();
            dict["MedicationPathwayID"] = req["medicationpathwayid"]?.ToObject<int>();
            dict["OperationTime"] = req.ToDateTime("operationtime");
            dict["LeaveTime"] = req.ToDateTime("leavetime");
            dict["NextTime"] = req.ToDateTime("nexttime");
            dict["Fstatus"] = req["fstatus"]?.ToObject<string>();
            dict["Ftime"] = req["ftime"]?.ToObject<int>();


            return dict;
        }
    }
}