﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class GenderRepository : BaseRepository
    {
        public GenderRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "data_gender";
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
select id,Code,GenderName,IsActive 
from data_gender 
where IsDeleted=0
limit ?p1,?p2
", offset,pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne("select id,Code,GenderName,IsActive  from data_gender where id=?p1 and IsDeleted = 0", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = data["code"]?.ToObject<string>();
            dict["GenderName"] = data["gendername"]?.ToObject<string>();

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
            return _db.GetOne(@"
select id,GenderName text 
from data_gender 
where id=?p1 
and IsDeleted=0", id);
        }
    }
}