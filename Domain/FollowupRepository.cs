using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class FollowupRepository : BaseLockableRepository
    {
        public FollowupRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_followup";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
t_followup.ID
,t_followup.PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonCode
,t_followup.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,t_followup.TIME
,t_followup.PersonList
,t_followup.Abstract
,t_followup.Detail
,t_followup.IsActive
FROM t_followup
LEFT JOIN t_patient
ON t_followup.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
WHERE t_followup.OrgnizationID=?p1
AND t_followup.IsDeleted=0
LIMIT ?p2,?p3", orgid, offset, pageSize);
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
t_followup.ID
,t_followup.PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonCode
,t_followup.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,t_followup.Time
,t_followup.PersonList
,t_followup.Abstract
,t_followup.Detail
,t_followup.IsActive
FROM t_followup
LEFT JOIN t_patient
ON t_followup.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_followup.OrgnizationID=t_orgnization.ID
WHERE t_followup.PatientID=?p1
AND t_followup.IsDeleted=0
ORDER BY Time DESC
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
,TIME
,PersonList
,Abstract
,Detail
FROM t_followup
WHERE ID=?p1
AND t_followup.IsDeleted=0", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = data.ToInt("patientid");
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["Time"] = data.ToDateTime("time");
            dict["PersonList"] = data["personlist"]?.ToObject<string>();
            dict["Abstract"] = data["abstract"]?.ToObject<string>();
            dict["Detail"] = data["detail"]?.ToObject<string>();
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
