﻿/*
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
    public class RecipeDetailsService
    {
        private readonly ILogger<RecipeDetailsService> _logger;
        dbfactory db = new dbfactory();
        public RecipeDetailsService(ILogger<RecipeDetailsService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取个人的“用药记录明细”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“用药记录明细”数组</returns>
        [NonAction]
        public JArray GetPersonRecipeDetails(int personid)
        {
            return db.GetArray(@"
SELECT 
t_treat.ID
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersionIDCard
,t_patient.GenderID
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
LEFT JOIN t_treat
ON t_treatitem.TreatID=t_treat.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN data_gender
ON t_patient.GenderID=data_gender.ID
LEFT JOIN t_medication
ON t_treatitem.MedicationID=t_medication.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
WHERE t_treat.PatientID=?p1
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
IFNULL(t_treatitem.ID,'') AS  ID
,IFNULL(MedicationName,'') AS MedicationName
,IFNULL(MedicationID,'') AS MedicationID
,IFNULL(t_medication.Specification,'') AS Specification
,IFNULL(MedicationFreqCategoryID,'') AS MedicationFreqCategoryID
,IFNULL(data_medicationfreqcategory.ValueMessage,'') AS  Freq
,IFNULL(MedicationDosageFormID,'') AS MedicationDosageFormID
,IFNULL(data_medicationdosageform.`Name`,'') AS DosageName
,IFNULL(MedicationPathwayID,'') AS MedicationPathwayID
,IFNULL(data_medicationpathway.`Name`,'') AS PathwayName
,IFNULL(Type,'') AS Type
,IFNULL(SingleDoseAmount,'') AS SingleDoseAmount
,IFNULL(SingleDoseUnit,'') AS SingleDoseUnit
,IFNULL(TotalDoseAmount,'') AS TotalDoseAmount
,IFNULL(Remarks,'') AS Remarks
FROM t_treatitem
LEFT JOIN t_medication
ON t_treatitem.MedicationID=t_medication.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
WHERE t_treatitem.TreatID=?p1
AND t_treatitem.IsDeleted=0
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
,MedicationName
,MedicationID
,MedicationFreqCategoryID
,MedicationDosageFormID
,MedicationPathwayID
,Type
,SingleDoseAmount
,SingleDoseUnit
,TotalDoseAmount
,Remarks
FROM t_treatitem
WHERE ID=?p1
AND IsDeleted=0
", id);
            var medication = new MedicationController(null);
            res["medication"] = medication.GetMedicationInfo(res.ToInt("medicationid"));

            var dosage = new MedicationDosageFormController(null);
            res["dosage"] = dosage.GetDosageInfo(res.ToInt("medicationdosageformid"));

            var freq = new MedicationFreqCategoryController(null);
            res["freq"] = freq.GetFreqInfo(res.ToInt("medicationfreqcategoryid"));

            var pathway = new MedicationPathwayController(null);
            res["pathway"] = pathway.GetPathwayInfo(res.ToInt("medicationpathwayid"));

            return res;
        }


        /// <summary>
        /// 更改“用药记录明细”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="items">在请求body中JSON形式的“用药记录明细”信息</param>
        /// <param name="httpContext">请求上下文对象</param>
        /// <returns>响应状态信息</returns>
        [NonAction]
        public int[] SetTreatItem([FromBody] JObject[] items,Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            int[] res = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["TreatID"] = items[i]["treatid"]?.ToObject<int>();
                //dict["PatientID"] = items[i]["patientid"]?.ToObject<int>();
                //dict["GenderID"] = items?[i]?["genderid"]?.ToObject<int>();
                dict["MedicationFreqCategoryID"] = items[i].ToInt("medicationfreqcategoryid");
                dict["MedicationDosageFormID"] = items[i].ToInt("medicationdosageformid");
                dict["MedicationPathwayID"] = items[i].ToInt("medicationpathwayid");
                dict["MedicationID"] = items[i].ToInt("medicationid");
                //dict["ICDCode"] = items[i]["icdcode"]?.ToObject<int>();
                dict["Type"] = items[i]["type"]?.ToObject<int>();
                dict["SingleDoseAmount"] = items[i]["singledoseamount"]?.ToObject<int>();
                dict["SingleDoseUnit"] = items[i]["singledoseunit"]?.ToObject<string>();
                dict["TotalDoseAmount"] = items[i]["totaldoseamount"]?.ToObject<int>();
                //dict["Prescriber"] = items[i]["prescriber"]?.ToObject<int>();
                //dict["PrescribeTime"] = items[i]["prescribetime"]?.ToObject<int>();
                //dict["ReviewPharmacist"] = items[i]["reviewpharmacist"]?.ToObject<int>();
                //dict["ReviewTime"] = items[i]["reviewtime"]?.ToObject<int>();
                //dict["AllocationPharmacist"] = items[i]["allocationpharmacist"]?.ToObject<int>();
                //dict["AllocationTime"] = items[i]["allocationtime"]?.ToObject<int>();
                //dict["VerifyPharmacist"] = items[i]["verifypharmacist"]?.ToObject<int>();
                //dict["VerifyTime"] = items[i]["verifytime"]?.ToObject<int>();
                //dict["DispensePharmacist"] = items[i]["dispensepharmacist"]?.ToObject<int>();
                //dict["DispenseTime"] = items[i]["dispensetime"]?.ToObject<int>();
                dict["Remarks"] = items[i]["remarks"]?.ToObject<int>();

                if (items[i]["id"]?.ToObject<int>() > 0)
                {
                    Dictionary<string, object> keys = new Dictionary<string, object>();
                    keys["id"] = items[i]["id"].ToObject<int>();
                    dict["LastUpdatedBy"] = StampUtil.Stamp(httpContext);
                    dict["LastUpdatedTime"] = DateTime.Now;
                    res[i] = db.Update("t_treatitem", dict, keys);
                }
                else
                {
                    dict["CreatedBy"] = StampUtil.Stamp(httpContext);
                    dict["CreatedTime"] = DateTime.Now;
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