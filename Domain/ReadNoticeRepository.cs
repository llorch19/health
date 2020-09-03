using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class ReadNoticeRepository : BaseRepository
    {
        public ReadNoticeRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_noticeread";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListJointImp(int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
-- 根据UserID查询未读通知
SELECT 
IFNULL(t_notice.ID,'') AS ID
,IFNULL(t_notice.OrgnizationID,'') as OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName 
,IFNULL(t_user.ID,'') AS PublishUserID
,IFNULL(t_user.FamilyName,'') AS Publish 
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(t_notice.Title,'') AS Title
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') AS Attachment
,IFNULL(t_notice.IsActive,'') AS IsActive
FROM t_notice
LEFT JOIN t_user
ON t_user.ID=t_notice.PublishUserID
LEFT JOIN t_orgnization
ON t_orgnization.ID=t_notice.OrgnizationID
WHERE t_notice.ID NOT IN(
SELECT NoticeID AS ID FROM t_noticeread
WHERE UserID=?p1
AND IsDeleted=0
AND IsRead=1)
AND t_notice.IsDeleted = 0
LIMIT ?p1,?p2
", offset,pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT 
IFNULL(ID,'') AS ID
,IFNULL(NoticeID,'') AS NoticeID
,IFNULL(UserID,'') AS UserID
,IFNULL(OpenTime,'') AS OpenTime
,IFNULL(FinishTime,'') AS FinishTime
,IFNULL(IsRead,'') AS IsRead 
,IFNULL(IsActive,'') AS IsActive 
FROM t_noticeread
WHERE ID=?p1
AND IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["NoticeID"] = data.ToInt("noticeid");
            dict["UserID"] = data.ToInt("userid");
            dict["OpenTime"] = DateTime.Now;
            dict["IsRead"] = 1;
            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
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
            throw new NotImplementedException();
        }

        public override Dictionary<string, object> GetPostDelSetting(JObject data)
        {
            return new Dictionary<string, object>();
        }
    }
}
