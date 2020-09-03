using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class NoticeRepository : BaseRepository
    {
        public NoticeRepository(dbfactory db) : base(db)
        {
        }

        public override string TableName => "t_notice";

        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;

        public override Func<JObject, bool> IsLockAction => req => false;

        public override JObject GetAltInfo(int? id)
        {
            throw new NotImplementedException();
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["IsDeleted"] = 0;
            dict["IsActive"] = 1;  // IsActive=1 的记录可以被修改和删除
            return dict;
        }

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
SELECT 
IFNULL(t_notice.ID,'') AS ID
,IFNULL(t_notice.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_notice.PublishUserID,'') AS PublishUserID
,IFNULL(t_user.FamilyName,'') AS Publish
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(t_notice.Title,'') AS Title
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') AS Attachment
,IFNULL(t_notice.IsActive,'') AS IsActive
,IFNULL(t_noticeread.IsRead,0) AS IsRead
FROM t_notice
LEFT JOIN t_orgnization
ON t_notice.OrgnizationID=t_orgnization.ID
LEFT JOIN t_user
ON t_notice.PublishUserID=t_user.ID
LEFT JOIN t_noticeread
ON t_notice.ID=t_noticeread.NoticeID
AND t_noticeread.UserID=?p1
AND t_noticeread.IsDeleted=0
WHERE t_notice.IsDeleted=0
ORDER BY t_notice.ID
LIMIT ?p2,?p3
"
, offset, pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT 
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PublishUserID,'') AS PublishUserID
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(Title,'') AS Title
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') AS Attachment
,IFNULL(t_notice.IsActive,'') AS IsActive
FROM t_notice
WHERE ID=?p1
AND t_notice.IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetPostDelSetting(JObject data)
        {
            return base.GetPostDelSetting(data);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (IsAddAction(data))
            {
                dict["OrgnizationID"] = data.ToInt("orgnizationid");
                dict["PublishUserID"] = data.ToInt("id");
                dict["PublishTime"] = data.ToDateTime("publishtime");
            }

            dict["Content"] = data["content"]?.ToObject<string>();
            dict["Attachment"] = data["attachment"]?.ToObject<string>();
            dict["Title"] = data["title"]?.ToObject<string>();

            return dict;
        }
    }
}
