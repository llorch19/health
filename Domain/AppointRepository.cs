using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class AppointRepository : BaseLockableRepository
    {
        public AppointRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_appoint";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT   
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
AND t_appoint.IsDeleted=0
LIMIT ?p2,?p3", orgid, offset, pageSize);
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT   
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
AND t_appoint.IsDeleted=0
LIMIT ?p2,?p3", personid, offset, pageSize);
        }

        public override JArray GetListJointImp(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            throw new NotImplementedException();
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT   
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
AND t_appoint.IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["PatientID"] = data.ToInt("patientid");
            dict["Name"] = data["name"]?.ToObject<string>();
            dict["Code"] = data["code"]?.ToObject<string>();
            dict["Vaccine"] = data["vaccine"]?.ToObject<string>();
            dict["VaccinationDateStart"] = data.ToDateTime("vaccinationdatestart");
            dict["VaccinationDateEnd"] = data.ToDateTime("vaccinationdateend");
            dict["InjectionTimes"] = data.ToInt("injectiontimes");
            dict["IDCardNO"] = data["idcardno"]?.ToObject<string>();
            dict["Tel"] = data["tel"]?.ToObject<string>();
            dict["BirthDate"] = data.ToDateTime("birthdate");
            dict["Tstatus"] = data["tstatus"]?.ToObject<string>();
            dict["AppointmentCreatedTime"] = data.ToDateTime("appointmentcreatedtime");
            dict["IsCancel"] = data.ToInt("iscancel");
            dict["CancelTime"] = data.ToDateTime("canceltime");
            dict["IsComplete"] = data.ToInt("iscomplete");
            dict["CompleteTime"] = data.ToDateTime("completetime");
            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["IsDeleted"] = 0;
            dict["IsActive"] = 1;  // IsActive=1 的记录可以被修改和删除
            return dict;
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }

        public override JObject GetAltInfo(int? id)
        {
            throw new NotImplementedException();
        }
    }
}
