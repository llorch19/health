using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class MessageForPersonRepository : BaseRepository
    {
        public MessageForPersonRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_messagesent";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListJointImp(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

            return _db.GetArray(@"
SELECT
IFNULL(t_messagesent.ID,'') as ID
,IFNULL(t_messagesent.OrgnizationID,'') as OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName 
,IFNULL(t_user.ID,'') as PublishUserID
,IFNULL(t_user.FamilyName,'') as Publish 
,IFNULL(t_messagesent.PublishTime,'') as PublishTime
,IFNULL(t_messagesent.Title,'') as Title
,IFNULL(t_messagesent.Abstract,'') as Abstract
,IF(t_messagesent.Thumbnail IS NOT NULL, CONCAT(IFNULL(t_option.`value`,''),IFNULL(t_messagesent.Thumbnail,'')) ,'') as Thumbnail
,IFNULL(Content,'') as Content
,IF(t_messagesent.Attachment IS NOT NULL ,CONCAT(IFNULL(t_option.`value`,''),IFNULL(t_messagesent.Attachment,'')),'') as Attachment
,IFNULL(IsPublic,'') as IsPublic
,IFNULL(t_messagesent.IsActive,'') AS IsActive
FROM t_messagesent 
LEFT JOIN t_user
ON t_user.ID=t_messagesent.PublishUserID
LEFT JOIN t_orgnization
ON t_messagesent.OrgnizationID=t_orgnization.ID
LEFT JOIN t_option
ON t_option.`name`='fileserver'
WHERE t_messagesent.IsDeleted=0
AND t_messagesent.IsPublic=1
LIMIT ?p1,?p2
"
, offset,pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PublishUserID,'') AS PublishUserID
,IFNULL(PublishTime,'') as PublishTime
,IFNULL(Title,'') AS Title
,IFNULL(Abstract,'') AS Abstract
,IFNULL(Thumbnail,'') AS Thumbnail
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') as Attachment
,IFNULL(IsPublic,'') as IsPublic
,IFNULL(IsActive,'') AS IsActive
FROM t_messagesent 
WHERE t_messagesent.ID=?p1
AND t_messagesent.IsDeleted=0
AND t_messagesent.IsPublic=1
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["PublishUserID"] = data.ToInt("publishuserid");
            dict["Title"] = data["title"]?.ToObject<string>();
            dict["Abstract"] = data["abstract"]?.ToObject<string>();
            dict["Thumbnail"] = data["thumbnail"]?.ToObject<string>();
            dict["Content"] = data["content"]?.ToObject<string>();
            dict["Attachment"] = data["attachment"]?.ToObject<string>();
            dict["IsPublic"] = data["ispublic"]?.ToObject<string>();
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
