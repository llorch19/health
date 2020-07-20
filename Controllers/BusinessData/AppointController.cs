﻿/*
 * Title : “预约”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“预约”信息的增删查改
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
    public class AppointController : ControllerBase
    {
        private readonly ILogger<AppointController> _logger;
        public AppointController(ILogger<AppointController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“预约”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetOrgAppointList")]
        public JObject GetOrgAppointList(int orgid)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            JArray list = db.GetArray(@"SELECT   
IFNULL(t_appoint.ID,'') AS ID
,IFNULL(t_appoint.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(`Name`,'') AS `Name`
,IFNULL(Code,'') AS Code
,IFNULL(Vaccine,'') AS Vaccine
,IFNULL(VaccinationDateStart,'') AS VaccinationDateStart
,IFNULL(VaccinationDateEnd,'') AS VaccinationDateEnd
,IFNULL(InjectionTimes,'') AS InjectionTimes
,IFNULL(t_appoint.IDCardNO,'') AS IDCardNO
,IFNULL(t_appoint.Tel,'') AS Tel
,IFNULL(BirthDate,'') AS BirthDate
,IFNULL(Status,'') AS Status
,IFNULL(AppointmentCreatedTime,'') AS AppointmentCreatedTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(IsComplete,'') AS IsComplete
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(t_appoint.Description,'') AS Description
FROM t_appoint
LEFT JOIN t_orgnization
ON t_appoint.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_appoint.PatientID=t_patient.ID
WHERE t_appoint.OrgnizationID=?p1", orgid);
            if (list.HasValues)
            {
                res["status"] = 200;
                res["msg"] = "读取成功";
                res["list"] = list;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
            }
            return res;
        }

        /// <summary>
        /// 获取个人的“预约”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetPersonAppointList")]
        public JObject GetPersonAppointList(int personid)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            JArray list = db.GetArray(@"SELECT   
IFNULL(t_appoint.ID,'') AS ID
,IFNULL(t_appoint.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(`Name`,'') AS `Name`
,IFNULL(Code,'') AS Code
,IFNULL(Vaccine,'') AS Vaccine
,IFNULL(VaccinationDateStart,'') AS VaccinationDateStart
,IFNULL(VaccinationDateEnd,'') AS VaccinationDateEnd
,IFNULL(InjectionTimes,'') AS InjectionTimes
,IFNULL(t_appoint.IDCardNO,'') AS IDCardNO
,IFNULL(t_appoint.Tel,'') AS Tel
,IFNULL(BirthDate,'') AS BirthDate
,IFNULL(Status,'') AS Status
,IFNULL(AppointmentCreatedTime,'') AS AppointmentCreatedTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(IsComplete,'') AS IsComplete
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(t_appoint.Description,'') AS Description
FROM t_appoint
LEFT JOIN t_orgnization
ON t_appoint.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_appoint.PatientID=t_patient.ID
WHERE t_appoint.PatientID=?p1",personid);
            if (list.HasValues)
            {
                res["status"] = 200;
                res["msg"] = "读取成功";
                res["list"] = list;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
            }
            return res;
        }


        /// <summary>
        /// 获取“预约”信息，点击[科普公告]中的一个项目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“预约”信息</returns>
        [HttpGet]
        [Route("GetAppoint")]
        public JObject GetAppoint(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne(@"SELECT   
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PatientID,'') AS PatientID
,IFNULL(`Name`,'') AS `Name`
,IFNULL(Code,'') AS Code
,IFNULL(Vaccine,'') AS Vaccine
,IFNULL(VaccinationDateStart,'') AS VaccinationDateStart
,IFNULL(VaccinationDateEnd,'') AS VaccinationDateEnd
,IFNULL(InjectionTimes,'') AS InjectionTimes
,IFNULL(IDCardNO,'') AS IDCardNO
,IFNULL(Tel,'') AS Tel
,IFNULL(BirthDate,'') AS BirthDate
,IFNULL(Status,'') AS Status
,IFNULL(AppointmentCreatedTime,'') AS AppointmentCreatedTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(IsComplete,'') AS IsComplete
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(Description,'') AS Description
FROM t_appoint
WHERE ID=?p1", id);
            if (res["id"] != null)
            {
                res["orgnization"] = new OrgnizationController(null)
                    .GetOrgInfo(res["orgnizationid"].ToObject<int>());
                res["person"] = new PersonController(null,null)
                    .GetPersonInfo(res["patientid"].ToObject<int>());
                res["status"] = 200;
                res["msg"] = "读取成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
            }
            return res;
        }


        /// <summary>
        /// 更改“预约”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAppoint")]
        public JObject SetAppoint([FromBody] JObject req)
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
                    var rows = db.Insert("t_messagesent", dict);
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
                    var rows = db.Update("t_appoint", dict, keys);
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
        /// 删除“预约”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAppoint")]
        public JObject DelAppoint([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_appoint", dict);
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