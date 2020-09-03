using health.common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

        public override JArray GetListByOrgJointImp(int orgid, int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

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
AND t.IsCancel=0
AND t.IsFinish=0
LIMIT ?p2,?p3
", orgid,offset,pageSize);
            return array;
        }

        public  JArray GetListByRecvOrgJointImp(int orgid, int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
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
AND t.IsCancel=0
AND t.IsFinish=0
LIMIT ?p2,?p3
", orgid,offset,pageSize);
            return array;
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
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
LIMIT ?p2,?p3
", personid,offset,pageSize);
            return array;
        }

        public override JArray GetListJointImp(int pageSize, int pageIndex)
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
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["PatientID"] = data.ToInt("patientid");
            dict["DestOrgID"] = data.ToInt("destorgid");
            dict["StartTime"] = data.ToDateTime("starttime");
            dict["Remarks"] = data["remarks"]?.ToObject<string>();
            dict["IsCancel"] = data.ToInt("iscancel");
            dict["IsFinish"] = data.ToInt("isfinish");
            dict["EndTime"] = data.ToDateTime("endtime");
            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["PatientID"] = data.ToInt("patientid");
            dict["DestOrgID"] = data.ToInt("destorgid");
            dict["IsCancel"] = 0;  // 未取消的转诊记录可以更新
            dict["IsFinish"] = 0;  // 未完成的转正记录可以更新
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


        public int BeginTransfer(HttpContext ctx,int patientid,int desorgid,string remarks)
        {
            /* todo from service get 
             * - Appoint
             * - Attandent
             * - Check
             * - Followup
             * - Treat
             * - Vacc
            */

            // 本质上是新增
            JObject data = new JObject();
            data["id"] = 0;
            data["orgnizationid"] = ctx.GetIdentityInfo<int?>("orgnizationid");
            data["patientid"] = patientid;
            data["destorgid"] = desorgid;
            data["starttime"] = DateTime.Now;
            data["remarks"] = remarks;
            data["iscancel"] = 0;
            data["isfinish"] = 0;
            data["endtime"] = null;
            // 失活所有的业务数据
            return this.AddOrUpdateRaw(data , StampUtil.Stamp(ctx));
        }


        public int CancelTransfer(HttpContext ctx, int transferId,string remarks)
        {
            // 本质上是更新
            JObject data = GetOneRawImp(transferId);
            data["id"] = transferId;
            data["remarks"] = remarks;
            data["iscancel"] = 1;
            data["isfinish"] = 0;
            data["isactive"] = 0;  // 取消转诊后，该条转诊记录不再可写更新
            data["endtime"] = DateTime.Now;
            return this.AddOrUpdateRaw(data, StampUtil.Stamp(ctx));
        }


        public int AcceptTransfer(HttpContext ctx, int transferId, string remarks)
        {
            // 本质上是更新
            JObject data = GetOneRawImp(transferId);
            data["id"] = transferId;
            data["remarks"] = remarks;
            data["iscancel"] = 0;
            data["isfinish"] = 1;
            data["isactive"] = 0; // 接收转诊后，该条转诊记录不再可写更新
            data["endtime"] = DateTime.Now;
            return this.AddOrUpdateRaw(data, StampUtil.Stamp(ctx));
        }


        public override int AddOrUpdateRaw(JObject data, string username)
        {
            if (IsAddAction(data))
            {
                var dict = GetValue(data);
                dict["CreatedBy"] = username;
                dict["CreatedTime"] = DateTime.Now;
                dict["IsActive"] = 1;  // 新增的默认是激活的,如果Repository需要自动锁定新增，在AddOrUpdate之后调用SetLock()
                dict["IsDeleted"] = 0;
                return _db.Insert(TableName, dict);
            }
            else
            {
                if (IsLockAction(data))
                    return SetLock(data, username) > 0
                        ? GetId(data) : 0;
                else
                {
                    var valuedata = GetValue(data);
                    var keydata = GetKey(data);
                    valuedata["LastUpdatedBy"] = username;
                    valuedata["LastUpdatedTime"] = DateTime.Now;
                    valuedata["IsActive"] = 0;  // 转诊状态一次修改后不可再修改，只能重新发起
                    valuedata["IsDeleted"] = 0;
                    return _db.Update(TableName, valuedata, keydata) > 0
                        ? GetId(data)
                        : 0;
                }
            }
        }
    }
}
