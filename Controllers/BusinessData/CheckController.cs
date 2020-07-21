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
,PatientID
,t_patient.FamilyName AS PatientName
,t_detectionrecord.OrgnizationID
,t_orgnization.OrgName AS OrgName
,RecommendedTreatID
,recom.`Name` AS Recommend
,ChosenTreatID
,chosen.`Name` AS Chosen
,PatientTel
,IsReexam
,t_detectionrecord.GenderID
,data_gender.GenderName 
,SubmitBy
,submit.ChineseName AS Submitter
,SubmitTime
,t_detectionrecord.OrgCode
,DetectionNO
,ClinicalNO
,PatientName
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
,PatientID
,t_patient.FamilyName AS PatientName
,t_detectionrecord.OrgnizationID
,t_orgnization.OrgName AS OrgName
,RecommendedTreatID
,recom.`Name` AS Recommend
,ChosenTreatID
,chosen.`Name` AS Chosen
,PatientTel
,IsReexam
,t_detectionrecord.GenderID
,data_gender.GenderName 
,SubmitBy
,submit.ChineseName AS Submitter
,SubmitTime
,t_detectionrecord.OrgCode
,DetectionNO
,ClinicalNO
,PatientName
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
,PatientTel
,IsReexam
,GenderID
,SubmitBy
,SubmitTime
,OrgCode
,DetectionNO
,ClinicalNO
,PatientName
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
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    req.Remove("publish");
                    req["OrgnizationID"] = null;
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_detectionrecord", dict);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "新增成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "无法新增数据";
                    }
                }
                else if (id > 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    dict.Remove("id");
                    var keys = new Dictionary<string, object>();
                    keys["id"] = req["id"];
                    var rows = db.Update("t_detectionrecord", dict, keys);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "修改成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "修改失败";
                    }
                }
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "非法的请求";
            }
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
,ESC
,Name
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
                item["person"] = new PersonController(null, null)
                    .GetPersonInfo(item["personid"]?.ToObject<int>() ?? 0);
                item["orgnization"] = new OrgnizationController(null)
                    .GetOrgInfo(item["orgnizationid"]?.ToObject<int>() ?? 0);
                item["product"] = new CheckProductController(null)
                    .GetCheckProductInfo(item["detectionproductid"]?.ToObject<int>() ?? 0);
                item["result"] = new DetectionResultTypeController(null)
                    .GetResultTypeInfo(item["detectionresulttypeid"]?.ToObject<int>() ?? 0);
                //item["tester"]
            }

            return res;
        }
    }
}