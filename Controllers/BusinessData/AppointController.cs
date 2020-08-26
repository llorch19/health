/*
 * Title : “预约”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“预约”信息的增删查改
 * Comments
 */
using health.common;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class AppointController : AbstractBLLController
    {
        private readonly ILogger<AppointController> _logger;
        OrganizationController _org;
        PersonController _person;
        public override string TableName => "t_appoint";

        public AppointController(ILogger<AppointController> logger
            ,OrganizationController org
            ,PersonController person)
        {
            _logger = logger;
            _org = org;
            _person = person;
        }

        /// <summary>
        /// 获取机构的“预约”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetAppointList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = GetListImp();
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JArray GetListImp()
        {
            return db.GetArray(@"SELECT   
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
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(AppointmentCreatedTime,'') AS AppointmentCreatedTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(IsComplete,'') AS IsComplete
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(t_appoint.Description,'') AS Description
,IFNULL(t_appoint.IsActive,'') AS IsActive
FROM t_appoint
LEFT JOIN t_orgnization
ON t_appoint.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_appoint.PatientID=t_patient.ID
WHERE t_appoint.OrgnizationID=?p1
AND t_appoint.IsDeleted=0", HttpContext.GetIdentityInfo<int?>("orgnizationid"));
        }

        /// <summary>
        /// 获取个人的“预约”列表
        /// </summary>
        /// <param name="personid">请求的个人id</param>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetAppointListP")]
        public JObject GetAppointListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = GetAppointListPImp(personid);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JArray GetAppointListPImp(int personid)
        {
            return db.GetArray(@"SELECT   
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
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(AppointmentCreatedTime,'') AS AppointmentCreatedTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(IsComplete,'') AS IsComplete
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(t_appoint.Description,'') AS Description
,IFNULL(t_appoint.IsActive,'') AS IsActive
FROM t_appoint
LEFT JOIN t_orgnization
ON t_appoint.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_appoint.PatientID=t_patient.ID
WHERE t_appoint.PatientID=?p1
AND t_appoint.IsDeleted=0", personid);
        }

        /// <summary>
        /// 获取“预约”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“预约”信息</returns>
        [HttpGet]
        [Route("GetAppoint")]
        public override JObject Get(int id)
        {
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
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(AppointmentCreatedTime,'') AS AppointmentCreatedTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(IsComplete,'') AS IsComplete
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(Description,'') AS Description
,IFNULL(t_appoint.IsActive,'') AS IsActive
FROM t_appoint
WHERE ID=?p1
AND t_appoint.IsDeleted=0", id);
            var canread = res.Challenge(r=>r["id"]!=null);
            if (!canread)
                return Response_201_read.GetResult();

            res["orgnization"] = _org.GetOrgInfo(res.ToInt("orgnizationid"));
            res["person"] = _person.GetPersonInfo(res.ToInt("patientid"));
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“预约”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAppoint")]
        public override JObject Set([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var id = req.ToInt("id");
            if (id == 0) // 新增
                req["orgnizationid"] = orgid;
            else
            {
                var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
                if (!canwrite)
                    return Response_201_write.GetResult();
            }

            return base.Set(req);
        }




        /// <summary>
        /// 删除“预约”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAppoint")]
        public override JObject Del([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var objDatabase = db.GetOne("SELECT OrgnizationID FROM " + TableName + " WHERE ID=?p1 AND IsDeleted=0", id);
            var canwrite = req.Challenge(r => objDatabase.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();
            return base.Del(req);
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = req.ToInt("orgnizationid");
            dict["PatientID"] = req.ToInt("patientid");
            dict["Name"] = req["name"]?.ToObject<string>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["Vaccine"] = req["vaccine"]?.ToObject<string>();
            dict["VaccinationDateStart"] = req.ToDateTime("vaccinationdatestart");
            dict["VaccinationDateEnd"] = req.ToDateTime("vaccinationdateend");
            dict["InjectionTimes"] = req.ToInt("injectiontimes");
            dict["IDCardNO"] = req["idcardno"]?.ToObject<string>();
            dict["Tel"] = req["tel"]?.ToObject<string>();
            dict["BirthDate"] = req.ToDateTime("birthdate");
            dict["Tstatus"] = req["tstatus"]?.ToObject<string>();
            dict["AppointmentCreatedTime"] = req.ToDateTime("appointmentcreatedtime");
            dict["IsCancel"] = req.ToInt("iscancel");
            dict["CancelTime"] = req.ToDateTime("canceltime");
            dict["IsComplete"] = req.ToInt("iscomplete");
            dict["CompleteTime"] = req.ToDateTime("completetime");


            return dict;
        }
    }
}