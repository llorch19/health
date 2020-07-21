/*
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
t_treat.ID
,t_treat.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,PrescriptionCode
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonIDCard
,t_treat.GenderID
,data_gender.GenderName
,t_treat.AgeY
,t_treat.AgeM
,DiseaseCode
,TreatName
,DrugGroupNumber
,Tstatus
,Prescriber
,PrescribeTime
,PrescribeDepartment
,IsCancel
,CancelTime
,CompleteTime
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
",orgid);
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
t_treat.ID
,t_treat.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,PrescriptionCode
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonIDCard
,t_treat.GenderID
,data_gender.GenderName
,t_treat.AgeY
,t_treat.AgeM
,DiseaseCode
,TreatName
,DrugGroupNumber
,Tstatus
,Prescriber
,PrescribeTime
,PrescribeDepartment
,IsCancel
,CancelTime
,CompleteTime
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
        /// 获取“用药记录”信息，点击[科普公告]中的一个项目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”信息</returns>
        [HttpGet]
        [Route("GetTreat")]
        public JObject GetTreat(int id)
        {
            JObject res = db.GetOne(@"
SELECT 
ID
,OrgnizationID
,PrescriptionCode
,PatientID
,GenderID
,AgeY
,AgeM
,DiseaseCode
,TreatName
,DrugGroupNumber
,Tstatus
,Prescriber
,PrescribeTime
,PrescribeDepartment
,IsCancel
,CancelTime
,CompleteTime
FROM t_treat
WHERE ID=?p1
",id);
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
                res["prescriber"] = person.GetUserInfo(res["prescriber"]?.ToObject<int>() ?? 0);
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
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    req.Remove("publish");
                    req["OrgnizationID"] = null;
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_treat", dict);
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
                    var rows = db.Update("t_treat", dict, keys);
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