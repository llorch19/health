﻿/*
 * Title : “用药记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“用药记录”信息的增删查改
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
    public class TreatController : ControllerBase
    {
        private readonly ILogger<TreatController> _logger;
        dbfactory db = new dbfactory();
        public TreatController(ILogger<TreatController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“用药记录”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetOrgTreatList")]
        public JObject GetOrgTreatList(int orgid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(t_treat.GenderID,'') AS GenderID
,IFNULL(data_gender.GenderName,'') AS GenderName
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(Prescriber,'') AS Prescriber
,IFNULL(PrescribeTime,'') AS PrescribeTime
,IFNULL(PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(CompleteTime,'') AS CompleteTime
FROM t_treat
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN data_gender
ON t_treat.GenderID=data_gender.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
WHERE t_treat.OrgnizationID=?p1
", orgid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“用药记录”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetPersonTreatList")]
        public JObject GetPersonTreatList(int personid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(t_treat.GenderID,'') AS GenderID
,IFNULL(data_gender.GenderName,'') AS GenderName
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(Prescriber,'') AS Prescriber
,IFNULL(PrescribeTime,'') AS PrescribeTime
,IFNULL(PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(CompleteTime,'') AS CompleteTime
FROM t_treat
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN data_gender
ON t_treat.GenderID=data_gender.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
WHERE t_treat.PatientID=?p1
", personid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“用药记录”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”信息</returns>
        [HttpGet]
        [Route("GetTreat")]
        public JObject GetTreat(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(PatientID,'') AS PatientID
,IFNULL(GenderID,'') AS GenderID
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(Prescriber,'') AS Prescriber
,IFNULL(PrescribeTime,'') AS PrescribeTime
,IFNULL(PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(CompleteTime,'') AS CompleteTime
FROM t_treat
WHERE ID=?p1
", id);
            if (res["id"]==null)
            {
                res["status"] = 201;
                res["msg"] = "无法获取相应的数据";
            }
            else
            {
                OrgnizationController org = new OrgnizationController(null);
                res["orgnization"] = org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
                PersonController person = new PersonController(null, null);
                res["person"] = person.GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
                GenderController gender = new GenderController(null);
                res["gender"] = gender.GetGenderInfo(res["genderid"]?.ToObject<int>() ?? 0);
                TreatItemController items = new TreatItemController(null);
                res["items"] = items.GetTreatItemList(res["id"].ToObject<int>());
                res["status"] = 200;
                res["msg"] = "读取成功";
            }

            return res;
        }


        /// <summary>
        /// 更改“用药记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetTreat")]
        public JObject SetTreat([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["PrescriptionCode"] = req["prescriptioncode"]?.ToObject<string>();
            dict["PatientID"] = req["patientid"]?.ToObject<int>();
            dict["GenderID"] = req["genderid"]?.ToObject<int>();
            dict["DiseaseCode"] = req["diseasecode"]?.ToObject<string>();
            dict["TreatName"] = req["treatname"]?.ToObject<string>();
            dict["DrugGroupNumber"] = req["druggroupnumber"]?.ToObject<int>();
            dict["Tstatus"] = req["tstatus"]?.ToObject<string>();
            dict["Prescriber"] = req["prescriber"]?.ToObject<int>();
            dict["PrescribeTime"] = req["prescribetime"]?.ToObject<DateTime>();
            dict["PrescribeDepartment"] = req["prescribedepartment"]?.ToObject<string>();
            dict["IsCancel"] = req["iscancel"]?.ToObject<int>();
            dict["CancelTime"] = req["canceltime"]?.ToObject<DateTime>();
            dict["CompleteTime"] = req["completetime"]?.ToObject<DateTime>();
            // TODO: 在这里添加add item逻辑


            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_treat", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("t_treat", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }




        /// <summary>
        /// 删除“用药记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelTreat")]
        public JObject DelTreat([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_treat", dict);
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