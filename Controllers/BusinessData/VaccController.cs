/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“接种记录”信息的增删查改
 * Comments
 * - GetOrgVaccList 应该和GetPeron["vacc"]字段一致     @xuedi      2020-07-22      15:20
 */
using health.common;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace health.Controllers
{
    [Route("api")]
    public class VaccController : AbstractBLLController
    {
        private readonly ILogger<VaccController> _logger;
        PersonController _person;
        OrganizationController _org;
        MedicationController _med;
        MedicationDosageFormController _dosage;
        MedicationPathwayController _pathway;
        public override string TableName => "t_vacc";

        public VaccController(ILogger<VaccController> logger
            ,PersonController person
            ,OrganizationController org
            ,MedicationController med
            ,MedicationDosageFormController dosage
            ,MedicationPathwayController pathway)
        {
            _logger = logger;
            _person = person;
            _org = org;
            _med = med;
            _dosage = dosage;
            _pathway = pathway;
        }

        /// <summary>
        /// 获取机构的“接种记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            JObject res = GetListImp();
            return res;
        }

        [NonAction]
        public JObject GetListImp()
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
AND t_vacc.IsDeleted=0", HttpContext.GetIdentityInfo<int?>("orgnizationid"));
            return res;
        }


        /// <summary>
        /// 获取个人的“接种记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
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
            return res;
        }

        /// <summary>
        /// 获取“接种记录”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
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
            if (!res.HasValues)
                return Response_201_read.GetResult();

            res["person"] = _person.GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["org"] = _org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["operator"] = _person.GetUserInfo(res["operationuserid"]?.ToObject<int>() ?? 0);
            res["medication"] = _med.GetMedicationInfo(res["medicationid"]?.ToObject<int>() ?? 0);
            res["dosage"] = _dosage.GetDosageInfo(res["medicationdosageformid"]?.ToObject<int>() ?? 0);
            res["pathway"] = _pathway.GetPathwayInfo(res["medicationpathwayid"]?.ToObject<int>() ?? 0);

            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“接种记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            req["orgnizationid"] = orgid;
            return base.Set(req);
        }




        /// <summary>
        /// 删除“接种记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Del[controller]")]
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
            dict["OperationUserID"] = req.ToInt("operationuserid");
            dict["MedicationID"] = req.ToInt("medicationid");
            dict["MedicationDosageFormID"] = req.ToInt("medicationdosageformid");
            dict["MedicationPathwayID"] = req.ToInt("medicationpathwayid");
            dict["OperationTime"] = req.ToDateTime("operationtime");
            dict["LeaveTime"] = req.ToDateTime("leavetime");
            dict["NextTime"] = req.ToDateTime("nexttime");
            dict["Fstatus"] = req["fstatus"]?.ToObject<string>();
            dict["Ftime"] = req.ToInt("ftime");


            return dict;
        }
    }
}