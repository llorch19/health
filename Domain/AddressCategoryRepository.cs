using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class AddressCategoryRepository : BaseRepository
    {
        public AddressCategoryRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "data_addresscategory";
        public override Func<JObject, bool> IsLockAction => req => req.ContainsKey("isactive");

        public override JArray GetListByOrgJointImp(int orgid)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListByPersonJointImp(int personid)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListJointImp()
        {
            return _db.GetArray(@"
select ID,Code,AddressCategory,IsActive from data_addresscategory WHERE IsDeleted=0
");
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
select ID,Code,AddressCategory,IsActive from data_addresscategory where id=?p1 AND IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = data["code"]?.ToObject<string>();
            dict["AddressCategory"] = data["addresscategory"]?.ToObject<string>();
            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["IsDeleted"] = 0;
            return dict;
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }
    }
}
