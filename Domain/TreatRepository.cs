using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class TreatRepository : BaseLockableRepository
    {
        public TreatRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_treat";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(t_treat.PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(t_treat.Department,'') AS PrescribeDepartment
,IFNULL(t_treat.IsCancel,'') AS IsCancel
,IFNULL(t_treat.CancelTime,'') AS CancelTime
,IFNULL(t_treat.CompleteTime,'') AS CompleteTime
,IFNULL(t_treat.Prescriber,'') AS Prescriber
,IFNULL(t_treat.PrescribeTime,'') AS PrescribeTime
,IFNULL(t_treat.VerifyPharmacist,'') AS VerifyPharmacist
,IFNULL(t_treat.VerifyTime,'') AS VerifyTime
,IFNULL(t_treat.ReviewPharmacist,'') AS ReviewPharmacist
,IFNULL(t_treat.ReviewTime,'') AS ReviewTime
,IFNULL(t_treat.AllocationPharmacist,'') AS AllocationPharmacist
,IFNULL(t_treat.AllocationTime,'') AS AllocationTime
,IFNULL(t_treat.DispensePharmacist,'') AS DispensePharmacist
,IFNULL(t_treat.DispenseTime,'') AS DispenseTime
,IFNULL(t_treat.IsActive,'') AS IsActive
FROM t_treat
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
WHERE t_treat.IsDeleted=0
AND t_treat.OrgnizationID=?p1
LIMIT ?p2,?p3", orgid, offset, pageSize);
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_orgnization.OrgCode,'') AS OrgCode
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(t_treat.PatientID,'') AS PersonID
,IFNULL(t_patient.FamilyName,'') AS PersonName
,IFNULL(t_patient.IDCardNO,'') AS PersonIDCard
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(t_treat.Department,'') AS PrescribeDepartment
,IFNULL(t_treat.IsCancel,'') AS IsCancel
,IFNULL(t_treat.CancelTime,'') AS CancelTime
,IFNULL(t_treat.CompleteTime,'') AS CompleteTime
,IFNULL(t_treat.Prescriber,'') AS Prescriber
,IFNULL(t_treat.PrescribeTime,'') AS PrescribeTime
,IFNULL(t_treat.VerifyPharmacist,'') AS VerifyPharmacist
,IFNULL(t_treat.VerifyTime,'') AS VerifyTime
,IFNULL(t_treat.ReviewPharmacist,'') AS ReviewPharmacist
,IFNULL(t_treat.ReviewTime,'') AS ReviewTime
,IFNULL(t_treat.AllocationPharmacist,'') AS AllocationPharmacist
,IFNULL(t_treat.AllocationTime,'') AS AllocationTime
,IFNULL(t_treat.DispensePharmacist,'') AS DispensePharmacist
,IFNULL(t_treat.DispenseTime,'') AS DispenseTime
FROM t_treat
LEFT JOIN t_orgnization
ON t_treat.OrgnizationID=t_orgnization.ID
LEFT JOIN t_patient
ON t_treat.PatientID=t_patient.ID
LEFT JOIN t_user prescribe
ON t_treat.Prescriber=prescribe.ID
WHERE t_treat.IsDeleted=0
AND t_treat.PatientID=?p1
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
,IFNULL(PrescriptionCode,'') AS PrescriptionCode
,IFNULL(PatientID,'') AS PatientID
,IFNULL(DiseaseCode,'') AS DiseaseCode
,IFNULL(TreatName,'') AS TreatName
,IFNULL(DrugGroupNumber,'') AS DrugGroupNumber
,IFNULL(Tstatus,'') AS Tstatus
,IFNULL(PrescribeDepartment,'') AS PrescribeDepartment
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(CancelTime,'') AS CancelTime
,IFNULL(CompleteTime,'') AS CompleteTime
,IFNULL(Prescriber,'') AS Prescriber
,IFNULL(PrescribeTime,'') AS PrescribeTime
,IFNULL(t_treat.VerifyPharmacist,'') AS VerifyPharmacist
,IFNULL(t_treat.VerifyTime,'') AS VerifyTime
,IFNULL(t_treat.ReviewPharmacist,'') AS ReviewPharmacist
,IFNULL(t_treat.ReviewTime,'') AS ReviewTime
,IFNULL(t_treat.AllocationPharmacist,'') AS AllocationPharmacist
,IFNULL(t_treat.AllocationTime,'') AS AllocationTime
,IFNULL(t_treat.DispensePharmacist,'') AS DispensePharmacist
,IFNULL(t_treat.DispenseTime,'') AS DispenseTime
,IFNULL(IsActive,'') AS IsActive
FROM t_treat
WHERE ID=?p1
AND IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["PatientID"] = data.ToInt("patientid");
            dict["TreatName"] = data["treatname"]?.ToObject<string>();
            dict["DiseaseCode"] = data["diseasecode"]?.ToObject<string>();
            dict["PrescriptionCode"] = data["prescriptioncode"]?.ToObject<string>();
            dict["DrugGroupNumber"] = data.ToInt("druggroupnumber");
            dict["Tstatus"] = data["tstatus"]?.ToObject<string>();
            dict["Prescriber"] = data.ToInt("prescriber");
            dict["PrescribeTime"] = data.ToDateTime("prescribetime");
            dict["PrescribeDepartment"] = data["prescribedepartment"]?.ToObject<string>();
            dict["IsCancel"] = data.ToInt("iscancel");
            dict["CancelTime"] = data.ToDateTime("canceltime");
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
