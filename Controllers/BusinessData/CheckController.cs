/*
 * Title : “检测”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class CheckController : ControllerBase
    {
        private readonly ILogger<CheckController> _logger;
        dbfactory db = new dbfactory();
        public CheckController(ILogger<CheckController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“检测”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [HttpGet]
        [Route("GetOrgCheckList")]
        public JObject GetOrgCheckList(int orgid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_detectionrecord.ID
,CheckType
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_detectionrecord.OrgnizationID
,t_orgnization.OrgName AS OrgName
,RecommendedTreatID
,recom.`Name` AS Recommend
,ChosenTreatID
,chosen.`Name` AS Chosen
,t_patient.Tel AS PersonTel
,IsReexam
,t_detectionrecord.GenderID
,data_gender.GenderName 
,SubmitBy
,submit.ChineseName AS Submitter
,SubmitTime
,t_orgnization.OrgCode
,DetectionNO
,ClinicalNO
,Pics
,Pdf
,DiagnoticsTypeID
,DiagnoticsTime
,DiagnoticsBy
,diagnotics.ChineseName AS Diagnoser
,ReportTime
,ReportBy
,report.ChineseName AS Reporter
,Reference
FROM 
t_detectionrecord
LEFT JOIN t_patient
ON t_detectionrecord.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecord.OrgnizationID=t_orgnization.ID
LEFT JOIN data_treatmentoption recom
ON t_detectionrecord.RecommendedTreatID=recom.ID
LEFT JOIN data_treatmentoption chosen
ON t_detectionrecord.ChosenTreatID=chosen.ID
LEFT JOIN data_gender
ON t_detectionrecord.GenderID=data_gender.ID
LEFT JOIN t_user submit
ON t_detectionrecord.SubmitBy=submit.ID
LEFT JOIN t_user diagnotics
ON t_detectionrecord.DiagnoticsBy=diagnotics.ID
LEFT JOIN t_user report
ON t_detectionrecord.ReportBy=report.ID
WHERE t_detectionrecord.OrgnizationID=?p1
", orgid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“检测”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [HttpGet]
        [Route("GetPersonCheckList")]
        public JObject GetPersonCheckList(int personid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_detectionrecord.ID
,CheckType
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_detectionrecord.OrgnizationID
,t_orgnization.OrgName AS OrgName
,RecommendedTreatID
,recom.`Name` AS Recommend
,ChosenTreatID
,chosen.`Name` AS Chosen
,t_patient.Tel AS PersonTel
,IsReexam
,t_detectionrecord.GenderID
,data_gender.GenderName 
,SubmitBy
,submit.ChineseName AS Submitter
,SubmitTime
,t_orgnization.OrgCode
,DetectionNO
,ClinicalNO
,Pics
,Pdf
,DiagnoticsTypeID
,DiagnoticsTime
,DiagnoticsBy
,diagnotics.ChineseName AS Diagnoser
,ReportTime
,ReportBy
,report.ChineseName AS Reporter
,Reference
FROM 
t_detectionrecord
LEFT JOIN t_patient
ON t_detectionrecord.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecord.OrgnizationID=t_orgnization.ID
LEFT JOIN data_treatmentoption recom
ON t_detectionrecord.RecommendedTreatID=recom.ID
LEFT JOIN data_treatmentoption chosen
ON t_detectionrecord.ChosenTreatID=chosen.ID
LEFT JOIN data_gender
ON t_detectionrecord.GenderID=data_gender.ID
LEFT JOIN t_user submit
ON t_detectionrecord.SubmitBy=submit.ID
LEFT JOIN t_user diagnotics
ON t_detectionrecord.DiagnoticsBy=diagnotics.ID
LEFT JOIN t_user report
ON t_detectionrecord.ReportBy=report.ID
WHERE t_detectionrecord.PatientID=?p1
", personid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“检测”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测”信息</returns>
        [HttpGet]
        [Route("GetCheck")]
        public JObject GetCheck(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
ID
,CheckType
,PatientID
,OrgnizationID
,RecommendedTreatID
,ChosenTreatID
,IsReexam
,GenderID
,SubmitBy
,SubmitTime
,DetectionNO
,ClinicalNO
,Pics
,Pdf
,DiagnoticsTypeID
,DiagnoticsTime
,DiagnoticsBy
,ReportTime
,ReportBy
,Reference
FROM 
t_detectionrecord
WHERE ID=?p1",id);
            res["person"] = new PersonController(null, null)
                .GetPersonInfo(res["patientid"]?.ToObject<int>()??0);
            res["submitby"] = new PersonController(null, null)
                .GetUserInfo(res["SubmitBy"]?.ToObject<int>() ?? 0);
            res["orgnization"] = new OrgnizationController(null)
                .GetOrgInfo(res["orgnizationid"]?.ToObject<int>()??0);
            res["recommend"] = new TreatmentOptionController(null)
                .GetTreatOptionInfo(res["recommendedtreatid"]?.ToObject<int>() ?? 0);
            res["chosen"] = new TreatmentOptionController(null)
                .GetTreatOptionInfo(res["chosentreatid"]?.ToObject<int>() ?? 0);
            res["gender"] = new GenderController(null)
                .GetGenderInfo(res["genderid"]?.ToObject<int>() ?? 0);
            res["items"] = GetCheckItems(res["id"]?.ToObject<int>() ?? 0);
            return res;
        }


        /// <summary>
        /// 更改“检测”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetCheck")]
        public JObject SetCheck([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req["patientid"]?.ToObject<int>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["RecommendedTreatID"] = req["recommendedtreatid"]?.ToObject<int>();
            dict["ChosenTreatID"] = req["chosentreatid"]?.ToObject<int>();
            dict["IsReexam"] = req["isreexam"]?.ToObject<int>();
            dict["GenderID"] = req["genderid"]?.ToObject<int>();
            dict["DetectionNO"] = req["detectionno"]?.ToObject<string>();
            dict["ClinicalNO"] = req["clinicalno"]?.ToObject<string>();
            dict["DepartmentName"] = req["departmentname"]?.ToObject<string>();
            dict["InpatientArea"] = req["inpatientarea"]?.ToObject<string>();
            dict["SickbedNO"] = req["sickbedno"]?.ToObject<string>();
            dict["SampleID"] = req["sampleid"]?.ToObject<string>();
            dict["SampleType"] = req["sampletype"]?.ToObject<string>();
            dict["SampleStatus"] = req["samplestatus"]?.ToObject<string>();
            dict["SubmitBy"] = req["submitby"]?.ToObject<string>();
            dict["SubmitTime"] = req["submittime"]?.ToObject<DateTime>();
            dict["ObjectiveResult"] = req["objectiveresult"]?.ToObject<string>();
            dict["SubjectiveResult"] = req["subjectiveresult"]?.ToObject<string>();
            dict["Pics"] = req["pics"]?.ToObject<string>();
            dict["Pdf"] = req["pdf"]?.ToObject<string>();
            dict["DiagnoticsTypeID"] = req["diagnoticstypeid"]?.ToObject<int>();
            dict["DiagnoticsTime"] = req["diagnoticstime"]?.ToObject<DateTime>();
            dict["DiagnoticsBy"] = req["diagnoticsby"]?.ToObject<int>();
            dict["ReportTime"] = req["reporttime"]?.ToObject<DateTime>();
            dict["ReportBy"] = req["reportby"]?.ToObject<string>();
            dict["Reference"] = req["reference"]?.ToObject<string>();
            // TODO: ADD CheckItem HERE


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
        /// 删除“检测”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelCheck")]
        public JObject DelCheck([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            var count = db.del("t_detectionrecord", dict);
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


        [NonAction]
        public JArray GetCheckItems(int checkid)
        {
            JArray res= db.GetArray(@"
SELECT 
ID
,PatientID AS PersonID
,OrgnizationID AS OrgnizationID
,DetectionProductID
,DetectionResultTypeID
,Result
,ResultUnit
,ResultTime
,Sugguest
,ReferenceValue
,SubmitBy
,SubmitTime
,Injecter
,InjectTime
,Observer
,ObserveTime
FROM t_detectionrecorditem
WHERE DetectionRecordID=?p1",checkid);
            foreach (JToken item in res)
            {
                PersonController person = new PersonController(null, null);
                item["person"] = person
                    .GetPersonInfo(item["personid"]?.ToObject<int>() ?? 0);
                item["submit"] = person.GetUserInfo(item["submitby"]?.ToObject<int>() ?? 0);
                item["inject"] = person.GetUserInfo(item["injecter"]?.ToObject<int>() ?? 0);
                item["observe"] = person.GetUserInfo(item["observer"]?.ToObject<int>() ?? 0);

                item["orgnization"] = new OrgnizationController(null)
                    .GetOrgInfo(item["orgnizationid"]?.ToObject<int>() ?? 0);
                item["product"] = new CheckProductController(null)
                    .GetCheckProductInfo(item["detectionproductid"]?.ToObject<int>() ?? 0);
                item["cresult"] = new DetectionResultTypeController(null)
                    .GetResultTypeInfo(item["detectionresulttypeid"]?.ToObject<int>() ?? 0);
                //item["tester"]
            }

            return res;
        }
    }
}