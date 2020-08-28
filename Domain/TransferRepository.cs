using health.common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class TransferRepository : BaseRepository
    {
        public TransferRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_transfer";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid)
        {
            var array = _db.GetArray(@"
SELECT 
t.ID
,src.OrgName AS FromOrg
,des.OrgName AS ToOrg
,person.FamilyName AS PName
,t.OrgnizationID
,t.PatientID
,t.DestOrgID
,t.StartTime
,t.IsCancel
,t.IsFinish
,t.EndTime
,t.IsActive
FROM t_transfer t
LEFT JOIN t_orgnization src
ON t.OrgnizationID = src.ID
LEFT JOIN t_orgnization des
ON t.DestOrgID = des.ID
LEFT JOIN t_patient person
ON t.PatientID=person.ID
WHERE t.IsDeleted=0
AND t.OrgnizationID=?p1
",orgid);
            return array;
        }

        public  JArray GetListByRecvOrgJointImp(int orgid)
        {
            var array = _db.GetArray(@"
SELECT 
t.ID
,src.OrgName AS FromOrg
,des.OrgName AS ToOrg
,person.FamilyName AS PName
,t.OrgnizationID
,t.PatientID
,t.DestOrgID
,t.StartTime
,t.IsCancel
,t.IsFinish
,t.EndTime
,t.IsActive
FROM t_transfer t
LEFT JOIN t_orgnization src
ON t.OrgnizationID = src.ID
LEFT JOIN t_orgnization des
ON t.DestOrgID = des.ID
LEFT JOIN t_patient person
ON t.PatientID=person.ID
WHERE t.IsDeleted=0
AND t.DestOrgID=?p1
", orgid);
            return array;
        }

        public override JArray GetListByPersonJointImp(int personid)
        {
            var array = _db.GetArray(@"
SELECT 
t.ID
,src.OrgName AS FromOrg
,des.OrgName AS ToOrg
,person.FamilyName AS PName
,t.OrgnizationID
,t.PatientID
,t.DestOrgID
,t.StartTime
,t.IsCancel
,t.IsFinish
,t.EndTime
,t.IsActive
FROM t_transfer t
LEFT JOIN t_orgnization src
ON t.OrgnizationID = src.ID
LEFT JOIN t_orgnization des
ON t.DestOrgID = des.ID
LEFT JOIN t_patient person
ON t.PatientID=person.ID
WHERE t.IsDeleted=0
AND t.PatientID=?p1
", personid);
            return array;
        }

        public override JArray GetListJointImp()
        {
            throw new NotImplementedException();
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT 
t.ID
,t.OrgnizationID
,t.PatientID
,t.DestOrgID
,t.StartTime
,t.IsCancel
,t.IsFinish
,t.EndTime
,t.IsActive
FROM t_transfer t
WHERE t.IsDeleted=0
AND t.ID=?p1
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = data.ToInt("OrgnizationID");
            dict["PatientID"] = data.ToInt("PatientID");
            dict["DestOrgID"] = data.ToInt("DestOrgID");
            dict["StartTime"] = data.ToDateTime("StartTime");
            dict["Remarks"] = data["Remarks"]?.ToObject<string>();
            dict["IsCancel"] = data.ToInt("IsCancel");
            dict["IsFinish"] = data.ToInt("IsFinish");
            dict["EndTime"] = data.ToInt("EndTime");
            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["OrgnizationID"] = data.ToInt("OrgnizationID");
            dict["PatientID"] = data.ToInt("PatientID");
            dict["DestOrgID"] = data.ToInt("DestOrgID");
            dict["IsDeleted"] = 0; // IsDeleted=0 的记录可以被查看
            dict["IsActive"] = 1;  // IsActive=1 的记录可以被修改和删除
            return dict;
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }

        public override JObject GetAltInfo(int? id)
        {
            return _db.GetOne(@"
SELECT 
t.ID
,src.OrgName AS FromOrg
,des.OrgName AS ToOrg
,person.FamilyName AS PName
,IsCancel
,IsFinish
FROM t_transfer t
LEFT JOIN t_orgnization src
ON t.OrgnizationID = src.ID
LEFT JOIN t_orgnization des
ON t.DestOrgID = des.ID
LEFT JOIN t_patient person
ON t.PatientID=person.ID
WHERE t.IsDeleted=0
AND t.ID=1
", id);
        }


        public int Transfer(HttpContext ctx,int patientid,int desorgid,string remarks)
        {
            /* todo from service get 
             * - Appoint
             * - Attandent
             * - Check
             * - Followup
             * - Treat
             * - Vacc
            */
            JObject data = new JObject();
            data["ID"] = 0;
            data["OrgnizationID"] = ctx.GetIdentityInfo<int?>("orgnizationid");
            data["PatientID"] = patientid;
            data["DestOrgID"] = desorgid;
            data["StartTime"] = DateTime.Now;
            data["Remarks"] = remarks;
            data["IsCancel"] = 0;
            data["IsFinish"] = 0;
            data["EndTime"] = null;
            // 失活所有的业务数据
            return AddOrUpdateRaw(data , StampUtil.Stamp(ctx));
        }


        public int CancelTransfer(HttpContext ctx, int transferId,string remarks)
        {
            JObject data = GetOneRawImp(transferId);
            data["ID"] = transferId;
            data["Remarks"] = remarks;
            data["IsCancel"] = 1;
            data["IsFinish"] = 0;
            data["IsActive"] = 0;  // 取消转诊后，该条转诊记录不再可写更新
            data["EndTime"] = DateTime.Now;
            return AddOrUpdateRaw(data, StampUtil.Stamp(ctx));
        }


        public int AcceptTransfer(HttpContext ctx, int transferId, string remarks)
        {
            JObject data = GetOneRawImp(transferId);
            data["ID"] = transferId;
            data["Remarks"] = remarks;
            data["IsCancel"] = 0;
            data["IsFinish"] = 1;
            data["IsActive"] = 0; // 接收转诊后，该条转诊记录不再可写更新
            data["EndTime"] = DateTime.Now;
            return AddOrUpdateRaw(data, StampUtil.Stamp(ctx));
        }
    }
}
