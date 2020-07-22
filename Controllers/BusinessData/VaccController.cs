/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“接种记录”信息的增删查改
 * Comments
 * - GetOrgVaccList 应该和GetPeron["vacc"]字段一致     @xuedi      2020-07-22      15:20
 */
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
    [ApiController]
    [Route("api")]
    public class VaccController : ControllerBase
    {
        private readonly ILogger<VaccController> _logger;
        dbfactory db = new dbfactory();
        public VaccController(ILogger<VaccController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“接种记录”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("GetOrgVaccList")]
        public JObject GetOrgVaccList(int orgid)
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
WHERE t_vacc.OrgnizationID=?p1", orgid);
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
        public JObject GetPersonVaccList(int personid)
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
WHERE t_vacc.PatientID=?p1", personid);
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
        public JObject GetVacc(int id)
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
FROM t_vacc
WHERE ID=?p1", id);
            PersonController person = new PersonController(null,null);
            res["person"] = person
                .GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["org"] = new OrgnizationController(null)
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
        public JObject SetVacc([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req["patientid"]?.ToObject<int>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["OperationUserID"] = req["operationuserid"]?.ToObject<int>();
            dict["MedicationID"] = req["medicationid"]?.ToObject<int>();
            dict["MedicationDosageFormID"] = req["medicationdosageformid"]?.ToObject<int>();
            dict["MedicationPathwayID"] = req["medicationpathwayid"]?.ToObject<int>();
            dict["OperationTime"] = req["operationtime"]?.ToObject<DateTime>();
            dict["LeaveTime"] = req["leavetime"]?.ToObject<DateTime>();
            dict["NextTime"] = req["nexttime"]?.ToObject<DateTime>();
            dict["Fstatus"] = req["fstatus"]?.ToObject<string>();
            dict["Ftime"] = req["ftime"]?.ToObject<int>();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_vacc", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("t_vacc", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }




        /// <summary>
        /// 删除“接种记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelVacc")]
        public JObject DelVacc([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            var count = db.del("t_vacc", dict);
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