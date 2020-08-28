using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class AreaRepository : BaseRepository
    {
        public AreaRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "data_area";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid,int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public JArray GetListJointImp(int parentid, int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            var res =  _db.GetArray(@"
select id,AreaCode,AreaName,parentID,cs,AreaCodeV2 from data_area where parentID=?p1 limit ?p2,?p3
", parentid,offset,pageSize);
            return res;
        }

        public override JArray GetListJointImp(int pageSize, int pageIndex)
        {
            return GetListJointImp(0,pageSize,pageIndex);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
select id,AreaCode,AreaName,parentID,cs,AreaCodeV2 from data_area where id=?p1
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["AreaCode"] = data["areacode"]?.ToObject<string>();
            dict["AreaName"] = data["areaname"]?.ToObject<string>();
            dict["ParentID"] = data.ToInt("parentid");
            dict["dingdingDept"] = data["dingdingdept"]?.ToObject<string>();
            dict["cs"] = data.ToInt("cs");
            dict["AreaCodeV2"] = data["areacodev2"]?.ToObject<string>();

            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["id"] = data.ToInt("id");
            return dict;
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }

        public override JObject GetAltInfo(int? id)
        {
            return _db.GetOne(@"
select id,AreaName text from data_area where id=?p1
", id);
        }

        public override int AddOrUpdateRaw(JObject data, string username)
        {
            // 需要针对表格定制新增修改方法时，重写基类的此方法
            throw new NotImplementedException();
        }

        public override int DelRaw(JObject data, string username)
        {
            // 需要针对表格定制删除方法时，重写基类的此方法
            throw new NotImplementedException();
        }

    }
}
