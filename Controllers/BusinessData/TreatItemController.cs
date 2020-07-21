/*
 * Title : “用药记录明细”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“用药记录明细”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class TreatItemController : ControllerBase
    {
        private readonly ILogger<TreatItemController> _logger;
        dbfactory db = new dbfactory();
        public TreatItemController(ILogger<TreatItemController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取个人的“用药记录明细”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“用药记录明细”数组</returns>
        [NonAction]
        public JArray GetPersonTreatItemList(int personid)
        {
            return db.GetArray(@"
SELECT 
t_treatitem.ID
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersionIDCard
,t_treatitem.GenderID
,data_gender.GenderName
,MedicationID
,t_medication.`Name` AS MedicationName
,t_medication.Specification 
,MedicationFreqCategoryID
,data_medicationfreqcategory.ValueMessage AS Freq
,MedicationDosageFormID
,data_medicationdosageform.`Name` AS DosageName
,MedicationPathwayID
,data_medicationpathway.`Name` AS PathwayName
,t_treatitem.AgeY
,t_treatitem.AgeM
,ICDCode
,Type
,SingleDoseAmount
,SingleDoseUnit
,TotalDoseAmount
,Prescriber
,PrescribeTime
,ReviewPharmacist
,ReviewTime
,AllocationPharmacist
,AllocationTime
,VerifyPharmacist
,VerifyTime
,DispensePharmacist
,DispenseTime
,Remarks
FROM t_treatitem
LEFT JOIN t_patient
ON t_treatitem.PatientID=t_patient.ID
LEFT JOIN data_gender
ON t_treatitem.GenderID=data_gender.ID
LEFT JOIN t_medication
ON t_treatitem.MedicationID=t_medication.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
WHERE t_treatitem.PatientID=?p1
", personid);
        }

        /// <summary>
        /// 获取关联的“用药记录明细”历史
        /// </summary>
        /// <param name="treatid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“用药记录明细”数组</returns>
        [NonAction]
        public JArray GetTreatItemList(int treatid)
        {
            return db.GetArray(@"
SELECT 
t_treatitem.ID
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersionIDCard
,t_treatitem.GenderID
,data_gender.GenderName
,MedicationID
,t_medication.`Name` AS MedicationName
,t_medication.Specification 
,MedicationFreqCategoryID
,data_medicationfreqcategory.ValueMessage AS Freq
,MedicationDosageFormID
,data_medicationdosageform.`Name` AS DosageName
,MedicationPathwayID
,data_medicationpathway.`Name` AS PathwayName
,t_treatitem.AgeY
,t_treatitem.AgeM
,ICDCode
,Type
,SingleDoseAmount
,SingleDoseUnit
,TotalDoseAmount
,Prescriber
,PrescribeTime
,ReviewPharmacist
,ReviewTime
,AllocationPharmacist
,AllocationTime
,VerifyPharmacist
,VerifyTime
,DispensePharmacist
,DispenseTime
,Remarks
FROM t_treatitem
LEFT JOIN t_patient
ON t_treatitem.PatientID=t_patient.ID
LEFT JOIN data_gender
ON t_treatitem.GenderID=data_gender.ID
LEFT JOIN t_medication
ON t_treatitem.MedicationID=t_medication.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
WHERE t_treatitem.TreatID=?p1
", treatid);
        }

        /// <summary>
        /// 获取“用药记录明细”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“用药记录明细”信息</returns>
        [NonAction]
        public JObject GetTreatItem(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
ID
,PatientID
,GenderID
,MedicationID
,MedicationFreqCategoryID
,MedicationDosageFormID
,MedicationPathwayID
,AgeY
,AgeM
,ICDCode
,Type
,SingleDoseAmount
,SingleDoseUnit
,TotalDoseAmount
,Prescriber
,PrescribeTime
,ReviewPharmacist
,ReviewTime
,AllocationPharmacist
,AllocationTime
,VerifyPharmacist
,VerifyTime
,DispensePharmacist
,DispenseTime
,Remarks
FROM t_treatitem
WHERE ID=?p1
",id);
            return res;
        }


        /// <summary>
        /// 更改“用药记录明细”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="items">在请求body中JSON形式的“用药记录明细”信息</param>
        /// <returns>响应状态信息</returns>
        [NonAction]
        public int[] SetTreatItem([FromBody] JObject[] items)
        {
            int[] res = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict[""] = items[i][""]?.ToObject<int>();

                if (items[i]["id"]?.ToObject<int>() > 0)
                {
                    Dictionary<string, object> keys = new Dictionary<string, object>();
                    keys["id"] = items[i]["id"].ToObject<int>();
                    res[i] = db.Update("t_treatitem", dict, keys);
                }
                else
                {
                    res[i] = db.Insert("t_treatitem", dict);
                }
            }

            return res;
        }




        /// <summary>
        /// 删除“用药记录明细”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录明细”信息</param>
        /// <returns>响应状态信息</returns>
        [NonAction]
        public JObject DelTreatItem([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_treatitem", dict);
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