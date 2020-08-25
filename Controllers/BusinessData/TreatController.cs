/*
 * Title : “用药记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“用药记录”信息的增删查改
 * Comments
 * - - GetOrgTreatList 应该和GetPeron["treat"]字段一致     @xuedi      2020-07-22 
 * - 新增“治疗用药记录”     @xuedi      2020-07-22      16:30
  */
using health.common;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class TreatController : AbstractBLLController
    {
        private readonly ILogger<TreatController> _logger;
        TreatItemController _items;
        OrganizationController _org;
        PersonController _person;
        public override string TableName => "t_treat";

        public TreatController(ILogger<TreatController> logger
            ,TreatItemController items
            ,OrganizationController org
            ,PersonController person)
        {
            _logger = logger;
            _items = items;
            _org = org;
            _person = person;
        }

        /// <summary>
        /// 获取机构的“治疗记录”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetTreatList2")]
        public JObject GetOrgTreatList2(int orgid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(t_treatitem.MedicationID,'') AS MedicationID
,IFNULL(t_medication.`Name`,'') AS MedicationName
,IFNULL(t_treat.PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(t_treat.Prescriber,'') AS Prescriber
,IFNULL(t_treat.PrescribeTime,'') AS PrescribeTime
,IFNULL(t_treat.PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(t_treat.IsCancel,'') AS IsCancel
,IFNULL(t_treat.CancelTime,'') AS CancelTime
,IFNULL(t_treat.CompleteTime,'') AS CompleteTime
,IFNULL(t_treatitem.MedicationDosageFormID,'') AS MedicationDosageFormID
,IFNULL(data_medicationdosageform.`Name`,'') AS Dosage
,IFNULL(t_treatitem.MedicationFreqCategoryID,'') AS MedicationFreqCategoryID
,IFNULL(data_medicationfreqcategory.ValueMessage,'') AS Freq
,IFNULL(t_treatitem.MedicationPathwayID,'') AS MedicationPathwayID
,IFNULL(data_medicationpathway.`Name`,'') AS Pathway
,IFNULL(t_treatitem.SingleDoseAmount,'') AS SingleDoseAmount
,IFNULL(t_treatitem.SingleDoseUnit,'') AS SingleDoseUnit
,IFNULL(t_treatitem.TotalDoseAmount,'') AS TotalDoseAmount
,IFNULL(t_treat.IsActive,'') AS IsActive
FROM t_treat
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
LEFT JOIN t_treatitem
ON t_treat.ID=t_treatitem.TreatID
LEFT JOIN t_medication
ON t_medication.ID=t_treatitem.MedicationID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
AND t_treat.OrgnizationID=?p1
WHERE t_treat.IsDeleted=0
", orgid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取机构的“治疗用药记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetTreatList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(t_treatitem.MedicationID,'') AS MedicationID
,IFNULL(t_medication.`Name`,'') AS MedicationName
,IFNULL(t_treat.PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(t_treat.Prescriber,'') AS Prescriber
,IFNULL(t_treat.PrescribeTime,'') AS PrescribeTime
,IFNULL(t_treat.PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(t_treat.IsCancel,'') AS IsCancel
,IFNULL(t_treat.CancelTime,'') AS CancelTime
,IFNULL(t_treat.CompleteTime,'') AS CompleteTime
,IFNULL(t_treatitem.MedicationDosageFormID,'') AS MedicationDosageFormID
,IFNULL(data_medicationdosageform.`Name`,'') AS Dosage
,IFNULL(t_treatitem.MedicationFreqCategoryID,'') AS MedicationFreqCategoryID
,IFNULL(data_medicationfreqcategory.ValueMessage,'') AS Freq
,IFNULL(t_treatitem.MedicationPathwayID,'') AS MedicationPathwayID
,IFNULL(data_medicationpathway.`Name`,'') AS Pathway
,IFNULL(t_treatitem.SingleDoseAmount,'') AS SingleDoseAmount
,IFNULL(t_treatitem.SingleDoseUnit,'') AS SingleDoseUnit
,IFNULL(t_treatitem.TotalDoseAmount,'') AS TotalDoseAmount
,IFNULL(t_treatitem.IsActive,'') AS IsActive
FROM t_treatitem
LEFT JOIN t_treat
ON t_treat.ID=t_treatitem.TreatID
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
LEFT JOIN t_medication
ON t_medication.ID=t_treatitem.MedicationID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
AND t_treat.OrgnizationID=?p1
WHERE t_treatitem.IsDeleted=0
", HttpContext.GetIdentityInfo<int?>("orgnizationid"));
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取个人的“治疗用药记录”列表
        /// </summary>
        /// <param name="personid">个人id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetTreatListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(t_treatitem.MedicationID,'') AS MedicationID
,IFNULL(t_medication.`Name`,'') AS MedicationName
,IFNULL(t_treat.PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(t_treat.Prescriber,'') AS Prescriber
,IFNULL(t_treat.PrescribeTime,'') AS PrescribeTime
,IFNULL(t_treat.PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(t_treat.IsCancel,'') AS IsCancel
,IFNULL(t_treat.CancelTime,'') AS CancelTime
,IFNULL(t_treat.CompleteTime,'') AS CompleteTime
,IFNULL(t_treatitem.MedicationDosageFormID,'') AS MedicationDosageFormID
,IFNULL(data_medicationdosageform.`Name`,'') AS Dosage
,IFNULL(t_treatitem.MedicationFreqCategoryID,'') AS MedicationFreqCategoryID
,IFNULL(data_medicationfreqcategory.ValueMessage,'') AS Freq
,IFNULL(t_treatitem.MedicationPathwayID,'') AS MedicationPathwayID
,IFNULL(data_medicationpathway.`Name`,'') AS Pathway
,IFNULL(t_treatitem.SingleDoseAmount,'') AS SingleDoseAmount
,IFNULL(t_treatitem.SingleDoseUnit,'') AS SingleDoseUnit
,IFNULL(t_treatitem.TotalDoseAmount,'') AS TotalDoseAmount
,IFNULL(t_treatitem.IsActive,'') AS IsActive
FROM t_treatitem
LEFT JOIN t_treat
ON t_treat.ID=t_treatitem.TreatID
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
LEFT JOIN t_medication
ON t_medication.ID=t_treatitem.MedicationID
LEFT JOIN data_medicationdosageform
ON t_treatitem.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationfreqcategory
ON t_treatitem.MedicationFreqCategoryID=data_medicationfreqcategory.ID
LEFT JOIN data_medicationpathway
ON t_treatitem.MedicationPathwayID=data_medicationpathway.ID
AND t_treat.PatientID=?p1
WHERE t_treatitem.IsDeleted=0
", personid );
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }

        /// <summary>
        /// 获取“治疗记录”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(PatientID,'') AS PatientID
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
,IFNULL(IsActive,'') AS IsActive
FROM t_treat
WHERE ID=?p1
AND IsDeleted=0
", id);

            var canread = res.Challenge(r=>r["id"]!=null);

            if (!canread)
                return Response_201_read.GetResult();

            res["orgnization"] = _org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["person"] = _person.GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["items"] = _items.GetTreatItemList(res["id"].ToObject<int>());
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“治疗记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = req.ToInt("orgnizationid");
            dict["PatientID"] = req.ToInt("patientid");
            //dict["TreatName"] = req["treatname"]?.ToObject<string>();
            //dict["DiseaseCode"] = req["diseasecode"]?.ToObject<string>();
            //dict["PrescriptionCode"] = req["prescriptioncode"]?.ToObject<string>();
            //dict["DrugGroupNumber"] = req.ToInt("druggroupnumber");
            //dict["Tstatus"] = req["tstatus"]?.ToObject<string>();
            //dict["Prescriber"] = req.ToInt("prescriber");
            //dict["PrescribeTime"] = req.ToDateTime("prescribetime");
            //dict["PrescribeDepartment"] = req["prescribedepartment"]?.ToObject<string>();
            //dict["IsCancel"] = req.ToInt("iscancel");
            //dict["CancelTime"] = req["canceltime"]?.ToObject<DateTime>();
            //dict["CompleteTime"] = req.ToDateTime("completetime");
            // TODO: 在这里添加add item逻辑

            JObject res = new JObject();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req.ToInt("id");
                dict["LastUpdatedBy"] = StampUtil.Stamp(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_treat", dict, condi);

                JArray list = _items.GetTreatItemList(req["id"].ToObject<int>());
                var item = list.FirstOrDefault();
                if (item==null)
                {
                    JObject itemReq = new JObject();
                    itemReq["id"] = 0;
                    itemReq["treatid"] = req.ToInt("id");
                    itemReq["medicationid"] = req.ToInt("medicationid");
                    itemReq["medicationpathwayid"] = req.ToInt("medicationpathwayid");
                    itemReq["medicationdosageformid"] = req.ToInt("medicationdosageformid");
                    itemReq["medicationfreqcategoryid"] = req.ToInt("medicationfreqcategoryid");
                    var rows = _items.SetTreatItem(new JObject[] { itemReq },HttpContext).Aggregate((sum, p) => sum += p);
                }
                else
                {
                    Dictionary<string, object> subcondi = new Dictionary<string, object>();
                    subcondi["id"] = ((JObject)item).ToInt("id");

                    Dictionary<string, object> subdict = new Dictionary<string, object>();
                    subdict["medicationid"] = req.ToInt("medicationid");
                    subdict["medicationpathwayid"] = req.ToInt("medicationpathwayid");
                    subdict["medicationdosageformid"] = req.ToInt("medicationdosageformid");
                    subdict["medicationfreqcategoryid"] = req.ToInt("medicationfreqcategoryid");

                    db.Update("t_treatitem",subdict,subcondi);
                }
                res["id"] = req["id"];
            }
            else
            {
                dict["CreatedBy"] = StampUtil.Stamp(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                var newId = this.db.Insert("t_treat", dict);
                res["id"] = newId;
                JObject itemReq = new JObject();
                itemReq["id"] = 0;
                itemReq["treatid"] = newId;
                itemReq["medicationid"] = req.ToInt("medicationid");
                itemReq["medicationpathwayid"] = req.ToInt("medicationpathwayid");
                itemReq["medicationdosageformid"] = req.ToInt("medicationdosageformid");
                itemReq["medicationfreqcategoryid"] = req.ToInt("medicationfreqcategoryid");
                var rows = _items.SetTreatItem(new JObject[] { itemReq },HttpContext).Aggregate((sum, p) => sum += p);
               
            }

           
            res["status"] = 200;
            res["msg"] = "提交成功";
            
            return res;
        }




        /// <summary>
        /// 删除“治疗记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            throw new NotImplementedException();
        }
    }
}