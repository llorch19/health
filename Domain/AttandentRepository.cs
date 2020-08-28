using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class AttandentRepository : BaseRepository
    {
        public AttandentRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_attandent";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
IFNULL(t_attandent.ID, '') AS ID
, IFNULL(PatientID, '') AS PersonID
, IFNUll(t_attandent.OrgnizationID, '') AS OrgnizationID
, IFNULL(t_orgnization.OrgName,'') AS OrgName
, IFNULL(SrcOrgID, '') AS SrcOrgID
, IFNULL(src.OrgName,'') AS SrcOrgName
, IFNULL(DesOrgID, '') AS DesOrgID
, IFNULL(des.OrgName,'') AS DesOrgName
, IFNULL(AdmissionTime, '') AS AdmissionTime
, IFNULL(AdmissionType, '') AS AdmissionType
, IFNULL(IsDischarged, '') AS IsDischarged
, IFNULL(DischargeTime, '') AS DischargeTime
, IFNULL(IsReferral, '') AS IsReferral
, IFNULL(DesStatus, '') AS DesStatus
, IFNULL(DesTime, '') AS DesTime
, IFNULL(IsReferralCancel, '') AS IsReferralCancel
, IFNULL(IsReferralFinish, '') AS IsReferralFinish
, IFNULL(t_attandent.IsActive,'') AS IsActive
FROM t_attandent
LEFT JOIN t_patient
ON t_attandent.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_attandent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_orgnization src
ON t_attandent.SrcOrgID=src.ID
LEFT JOIN t_orgnization des
ON t_attandent.DesOrgID=des.id
WHERE t_attandent.OrgnizationID=?p1
AND t_attandent.IsDeleted=0
LIMIT ?p2,?p3", orgid, offset, pageSize);
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
IFNULL(t_attandent.ID, '') AS ID
, IFNULL(PatientID, '') AS PersonID
, IFNUll(t_attandent.OrgnizationID, '') AS OrgnizationID
, IFNULL(t_orgnization.OrgName,'') AS OrgName
, IFNULL(SrcOrgID, '') AS SrcOrgID
, IFNULL(src.OrgName,'') AS SrcOrgName
, IFNULL(DesOrgID, '') AS DesOrgID
, IFNULL(des.OrgName,'') AS DesOrgName
, IFNULL(AdmissionTime, '') AS AdmissionTime
, IFNULL(AdmissionType, '') AS AdmissionType
, IFNULL(IsDischarged, '') AS IsDischarged
, IFNULL(DischargeTime, '') AS DischargeTime
, IFNULL(IsReferral, '') AS IsReferral
, IFNULL(DesStatus, '') AS DesStatus
, IFNULL(DesTime, '') AS DesTime
, IFNULL(IsReferralCancel, '') AS IsReferralCancel
, IFNULL(IsReferralFinish, '') AS IsReferralFinish
, IFNULL(t_attandent.IsActive,'') AS IsActive
FROM t_attandent
LEFT JOIN t_patient
ON t_attandent.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_attandent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_orgnization src
ON t_attandent.SrcOrgID=src.ID
LEFT JOIN t_orgnization des
ON t_attandent.DesOrgID=des.id
WHERE t_attandent.PatientID=?p1
AND t_attandent.IsDeleted=0
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
IFNULL(ID, '') AS ID
, IFNULL(PatientID, '') AS PersonID
, IFNUll(OrgnizationID, '') AS OrgnizationID
, IFNULL(SrcOrgID, '') AS SrcOrgID
, IFNULL(DesOrgID, '') AS DesOrgID
, IFNULL(AdmissionTime, '') AS AdmissionTime
, IFNULL(AdmissionType, '') AS AdmissionType
, IFNULL(IsDischarged, '') AS IsDischarged
, IFNULL(DischargeTime, '') AS DischargeTime
, IFNULL(IsReferral, '') AS IsReferral
, IFNULL(DesStatus, '') AS DesStatus
, IFNULL(DesTime, '') AS DesTime
, IFNULL(IsReferralCancel, '') AS IsReferralCancel
, IFNULL(IsReferralFinish, '') AS IsReferralFinish
, IFNULL(t_attandent.IsActive,'') AS IsActive
FROM
t_attandent
WHERE ID=?p1
AND t_attandent.IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = data.ToInt("personid");
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["SrcOrgID"] = data.ToInt("srcorgid");
            dict["DesOrgID"] = data.ToInt("desorgid");
            dict["AdmissionTime"] = data.ToDateTime("admissiontime");
            dict["AdmissionType"] = data["admissiontype"]?.ToObject<string>();
            dict["IsDischarged"] = data.ToInt("isdischarged");
            dict["DisChargeTime"] = data.ToDateTime("dischargetime");
            dict["IsReferral"] = data.ToInt("isreferral");
            dict["DesStatus"] = data["desstatus"]?.ToObject<string>();
            dict["DesTime"] = data.ToDateTime("destime");
            dict["IsReferralCancel"] = data.ToInt("isreferralcancel");
            dict["IsReferralFinish"] = data.ToInt("isreferralfinish");
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


        public JArray GetTransOut(int orgid)
        {
            var res = _db.GetArray(@"
SELECT 
PatientID

");
            return res;
        }
    }
}
