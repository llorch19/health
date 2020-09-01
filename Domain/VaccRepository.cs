using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class VaccRepository : BaseRepository
    {
        public VaccRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_vacc";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
t_vacc.ID
,PatientID
,t_patient.FamilyName AS Person
,t_vacc.OrgnizationID
,t_orgnization.OrgName AS OrgName
,OperatorName 
,MedicationID
,t_medication.`Name` AS Medication
,t_medication.`CommonName` AS CommonName
,MedicationDosageFormID
,data_medicationdosageform.`Name` AS Dosage
,MedicationPathwayID
,data_medicationpathway.`Name` AS Pathway
,Ftime
,OperationTime
,LeaveTime
,NextTime
,Fstatus
,TempratureP
,TempratureN
,Effect
,t_vacc.IsActive AS IsActive
FROM t_vacc
LEFT JOIN t_patient
ON t_vacc.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_vacc.OrgnizationID=t_orgnization.ID
LEFT JOIN t_medication
ON t_vacc.MedicationID=t_medication.ID
LEFT JOIN data_medicationdosageform
ON t_vacc.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_vacc.MedicationPathwayID=data_medicationpathway.ID
WHERE t_vacc.OrgnizationID=?p1
AND t_vacc.IsDeleted=0
LIMIT ?p2,?p3", orgid, offset, pageSize);
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
t_vacc.ID
,OperationTime
,t_medication.`CommonName` AS CommonName
,IFNULL(t_medication.ESC,'') AS ESC
,t_orgnization.OrgName AS OrgName
,OperatorName 
,PatientID
,t_patient.FamilyName AS Person
,t_vacc.OrgnizationID
,OperationUserID
,MedicationID
,t_medication.`Name` AS Medication
,MedicationDosageFormID
,data_medicationdosageform.`Name` AS Dosage
,MedicationPathwayID
,data_medicationpathway.`Name` AS Pathway
,Ftime
,LeaveTime
,NextTime
,Fstatus
,TempratureP
,TempratureN
,Effect
,t_vacc.IsActive AS IsActive
FROM t_vacc
LEFT JOIN t_patient
ON t_vacc.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_vacc.OrgnizationID=t_orgnization.ID
LEFT JOIN t_medication
ON t_vacc.MedicationID=t_medication.ID
LEFT JOIN data_medicationdosageform
ON t_vacc.MedicationDosageFormID=data_medicationdosageform.ID
LEFT JOIN data_medicationpathway
ON t_vacc.MedicationPathwayID=data_medicationpathway.ID
WHERE t_vacc.PatientID=?p1
AND t_vacc.IsDeleted=0
ORDER BY OperationTime DESC
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
ID
,PatientID
,OrgnizationID
,OperatorName
,MedicationID
,MedicationDosageFormID
,MedicationPathwayID
,Ftime
,OperationTime
,LeaveTime
,NextTime
,Fstatus
,TempratureP
,TempratureN
,Effect
,IsActive
FROM t_vacc
WHERE ID=?p1
and IsDeleted=0", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = data.ToInt("patientid");
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            //dict["OperationUserID"] = req.ToInt("operationuserid");
            dict["OperatorName"] = data["operatorname"]?.ToObject<string>();
            dict["MedicationID"] = data.ToInt("medicationid");
            dict["MedicationDosageFormID"] = data.ToInt("medicationdosageformid");
            dict["MedicationPathwayID"] = data.ToInt("medicationpathwayid");
            dict["OperationTime"] = data.ToDateTime("operationtime");
            dict["LeaveTime"] = data.ToDateTime("leavetime");
            dict["NextTime"] = data.ToDateTime("nexttime");
            dict["Fstatus"] = data["fstatus"]?.ToObject<string>();
            dict["Ftime"] = data.ToInt("ftime");
            dict["Effect"] = data["effect"]?.ToObject<string>();
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
