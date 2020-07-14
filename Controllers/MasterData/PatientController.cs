/*
 * Title : 个人信息管理控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - Get返回值应该同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi 2020-07-14 09:07
 * - Post提交值将同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi  2020-07-14 09:08
 * - 使用JObject["personinfo"]=db.GetOne() 这种形式逐个添加所需信息             @norway 2020-07-14 10:24
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly ILogger<PatientController> _logger;
        public PatientController(ILogger<PatientController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取个人列表，[人员转诊]菜单
        /// </summary>
        /// <returns>JSON数组形式的个人信息</returns>
        [HttpGet]
        [Route("GetPersonList")]
        public JObject GetPersonList(int pageIndex)
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray(
                @"select 
IFNULL(t_patient.ID,0) as ID
,IFNULL(t_attandent.IsTransfer,'') as IsTransfer
,IFNULL(FamilyName,'') as FamilyName
,IFNULL(GenderName,'') as GenderName
,IFNULL(OrgName,'') as OrgName
,IFNULL(t_patient.RegisterNO,'') as RegisterNO
,IFNULL(t_patient.Tel,'') as Tel
,IFNULL(t_patient.IDCardNO,'') as IDCardNO  
from t_patient
LEFT JOIN data_gender
on t_patient.GenderID=data_gender.ID
LEFT JOIN t_orgnization
ON t_patient.OrgnizationID=t_orgnization.ID
LEFT JOIN t_attandent
ON t_patient.ID=t_attandent.PatientID
LIMIT ?p1,10"
                , pageIndex);

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取人员信息，[人员信息录入]菜单
        /// </summary>
        /// <returns>JSON形式的某位个人信息，包括个人信息，</returns>
        [HttpGet]
        [Route("GetPerson")]
        public JObject GetPerson(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();

            // 个人信息
            res["personinfo"] = db.GetOne(
                @"select 
IFNULL(t_patient.ID,'') as ID
,IFNULL(OrgName,'') as OrgName,IFNULL(OrgCode,'') as OrgCode,IFNULL(RegisterNO,'') as RegisterNO
,IFNULL(FamilyName,'') as FamilyName,IFNULL(t_patient.Tel,'') as Tel,IFNULL(t_patient.IDCardNO,'') as IDCardNO,IFNULL(GenderName,'') as GenderName
,IFNULL(t_patient.Birthday,'') as Birthday,IFNULL(t_patient.Nation,'') as Nation,IFNULL(DomicileType,'') as DomicileType,IFNULL(t_patient.DomicileDetail,'') as DomicileDetail
,IFNULL(WorkUnitName,'') as WorkUnitName,IFNULL(data_occupation.OccupationName,'') as OccupationName,IFNULL(Detainees,'') as Detainees,IFNULL(data_addresscategory.AddressCategory,'') as AddressCategory
,IFNULL(t_patient.Address,'') as Address
,IFNULL(GuardianName,'') as GuardianName,IFNULL(GuardianContact,'') as GuardianContact
from t_patient 
LEFT JOIN t_orgnization
ON t_patient.OrgnizationID=t_orgnization.ID
LEFT JOIN data_gender
ON t_patient.GenderID=data_gender.ID
LEFT JOIN data_occupation
ON t_patient.OccupationCategoryID=data_occupation.ID
LEFT JOIN data_addresscategory
ON t_patient.AddressCategoryID=data_addresscategory.ID
where t_patient.ID=?p1"
                , id);

            // 检查信息
            res["checkinfo"] = db.GetArray(@"select 
IFNULL(t_detectionrecord.ID,'') as ID
,IFNULL(ReportTime,'') as ReportTime
,IFNULL(data_detectionresulttype.ResultName,'') as ResultName
from t_detectionrecord
LEFT JOIN data_detectionresulttype
ON t_detectionrecord.DiagnoticsTypeID=data_detectionresulttype.ID
where IsReexam = 0
and PatientID=?p1"
                ,id);

            res["check"] = db.GetOne(@"select 
IFNULL(t_detectionrecord.ID,'') as ID
,IFNULL(t_detectionproduct.`Name`,'') as ProductName
,IFNULL(t_detectionproduct.Specification,'') as Specification
,IFNULL(t_detectionproduct.BatchNumber,'') as BatchNumber
,IFNULL(t_detectionrecorditem.InjectTime,'') as InjectTime
,IFNULL(t_detectionrecorditem.ResultTime,'') as ResultTime
,IFNULL(r.`Name`,'') as Recommend
,IFNULL(c.`Name`,'') as Chosen
,IFNULL(t_detectionrecord.Pics,'') as Pics
,IFNULL(t_detectionrecord.Pdf,'') as Pdf
from t_detectionrecorditem
LEFT JOIN t_detectionrecord
ON t_detectionrecord.ID=t_detectionrecorditem.DetectionRecordID
LEFT JOIN t_detectionproduct
ON t_detectionrecorditem.DetectionProductID=t_detectionproduct.ID
LEFT JOIN data_treatmentoption r
ON t_detectionrecord.RecommendedTreatID=r.ID
LEFT JOIN data_treatmentoption c
ON t_detectionrecord.ChosenTreatID=c.ID
where  t_detectionrecord.IsReexam=0
and t_detectionrecorditem.PatientID = ?p1 
order by ResultTime desc limit 1"
                , id);

            res["recheck"] = db.GetArray(@"select 
IFNULL(t_detectionrecord.ID, '') as ID
, IFNULL(t_detectionproduct.`Name`, '') as ProductName
, IFNULL(t_detectionproduct.Specification, '') as Specification
, IFNULL(t_detectionproduct.BatchNumber, '') as BatchNumber
, IFNULL(t_detectionrecorditem.InjectTime, '') as InjectTime
, IFNULL(t_detectionrecorditem.ResultTime, '') as ResultTime
, IFNULL(r.`Name`, '') as Recommend
, IFNULL(c.`Name`, '') as Chosen
, IFNULL(t_detectionrecord.Pics, '') as Pics
, IFNULL(t_detectionrecord.Pdf, '') as Pdf
from t_detectionrecorditem
LEFT JOIN t_detectionrecord
ON t_detectionrecord.ID = t_detectionrecorditem.DetectionRecordID
LEFT JOIN t_detectionproduct
ON t_detectionrecorditem.DetectionProductID = t_detectionproduct.ID
LEFT JOIN data_treatmentoption r
ON t_detectionrecord.RecommendedTreatID = r.ID
LEFT JOIN data_treatmentoption c
ON t_detectionrecord.ChosenTreatID = c.ID
where  t_detectionrecord.IsReexam = 1
and t_detectionrecorditem.PatientID = ?p1
order by ResultTime desc limit 1"
                    ,id);

            // 随访信息
            res["followup"] = db.GetArray(
                @"select 
IFNULL(Time,'') as Time
,IFNULL(PersonList,'') as PersonList
,IFNULL(Abstract,'') as Abstract
,IFNULL(Detail,'') as Detail from t_followup
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
where PatientID=?p1", id);

            if (res["personinfo"].HasValues)
            {
                res["status"] = 200;
                res["msg"] = "读取数据成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }

            return res;
        }

        /// <summary>
        /// 更改个人信息。如果id=0新增个人信息，如果id>0修改个人信息。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的个人信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetPerson")]
        public JObject SetPerson([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_orgnization", dict);
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
                    var rows = db.Update("t_orgnization", dict, keys);
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
        /// 删除个人信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的个人信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelPerson")]
        public JObject DelPerson([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_orgnization", dict);
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

        /// <summary>
        /// 转诊
        /// </summary>
        /// <param name="req">在请求body中JSON形式的转诊信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("Transfer")]
        public JObject Transfer([FromBody] JObject req)
        {
            JObject res = new JObject();
            res["status"] = 201;
            res["msg"] = "功能正在开发中";
            return res;
        }
    }
}
