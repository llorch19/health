using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class OptionRepository : BaseRepository
    {
        public OptionRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_option";
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
SELECT 
IFNULL(id,'') AS id
,IFNULL(section,'') AS section
,IFNULL(`name`,'') AS `name`
,IFNULL(`value`,'') AS `value`
,IFNULL(`description`,'') AS `description`
FROM t_option
WHERE t_option.IsDeleted = 0
LIMIT ?p1,?p2
", offset,pageSize);
        }

        public JArray GetListJointImp(string section,int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
IFNULL(id,'') AS id
,IFNULL(section,'') AS section
,IFNULL(`name`,'') AS `name`
,IFNULL(`value`,'') AS `value`
,IFNULL(`description`,'') AS `description`
FROM t_option
WHERE section=?p1
AND t_option.IsDeleted = 0
LIMIT ?p2,?p3
", section,  offset, pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT 
IFNULL(id,'') AS id
,IFNULL(section,'') AS section
,IFNULL(`name`,'') AS `name`
,IFNULL(`value`,'') AS `value`
,IFNULL(`description`,'') AS `description`
FROM t_option
WHERE id=?p1
AND t_option.IsDeleted = 0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["section"] = data["orgname"]?.ToObject<string>();
            dict["name"] = data["orgcode"]?.ToObject<string>();
            dict["value"] = data["certcode"]?.ToObject<string>();
            dict["description"] = data["legalname"]?.ToObject<string>();
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
    }
}
