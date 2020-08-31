using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class MenuRepository : BaseRepository
    {
        public MenuRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_menu";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }


        public void BuildMenu(JObject[] flat, int parentid, ref JObject[] tree)
        {
            var selfAndBro = flat.Where(t => t.Value<int>("pid") == parentid);
            JObject parent = flat.FirstOrDefault(t => t.Value<int>("id") == parentid);
            var childrenOrNephew = flat.Except(selfAndBro).ToArray();

            if (selfAndBro.Count() == 0) return;

            foreach (var cur in selfAndBro)
            {
                var children = flat.Where(t => t.Value<int>("pid") == cur.Value<int>("id"));

                // add <children> to <cur>
                JArray array = new JArray();
                foreach (var child in children)
                    array.Add(child);

                if (cur.ContainsKey("children"))
                    cur.Remove("children");
                cur.Add("children", array);

                var anchor = parent?
                    .Value<JArray>("children")?
                    .ToArray<JToken>()?
                    .FirstOrDefault(t => t.Value<int>("id") == cur.Value<int>("id"));
                if (anchor == null)
                    tree = tree.Union(new JObject[] { cur }).ToArray();// only unanchored <cur> should be unioned

                foreach (var child in children)
                {
                    var pidChild = cur.Value<int>("id");
                    var flatChild = childrenOrNephew.Union(new JObject[] { cur }).ToArray();
                    BuildMenu(flatChild, pidChild, ref tree);
                }
            }
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }


        public JArray GetListJointImp(int groupid, int pageSize, int pageIndex)
        {
            JArray list = new JArray();
            var dbArray = _db.GetArray(@"
select id,name,icon,label,pid,seq 
from t_menu 
where isdeleted=0 and usergroup=?p1
", groupid);
            JObject[] menus = new JObject[0];
            var input = dbArray.ToObject<JObject[]>();
            BuildMenu(input.ToArray(), 0, ref menus);
            foreach (var item in menus)
                list.Add(item);

            return list;
        }

        public override JArray GetListJointImp(int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public JArray GetListJointImp(int groupid,int pid,int pageSize, int pageIndex)
        {
            JArray list = new JArray();
            var dbArray = _db.GetArray(@"
select id,name,icon,label,pid,seq 
from t_menu 
where isdeleted=0 and usergroup=?p1
", groupid);
            JObject[] menus = new JObject[0];
            var input = dbArray.ToObject<JObject[]>();
            BuildMenu(input.ToArray(), pid, ref menus);
            foreach (var item in menus)
                list.Add(item);

            return list;
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
select id,name,icon,label,pid,seq,usergroup 
from t_menu 
where id=?p1 and isdeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["name"] = data["name"]?.ToString();
            dict["icon"] = data["icon"]?.ToString();
            dict["label"] = data["label"]?.ToString();
            dict["pid"] = data["pid"]?.ToString();
            dict["usergroup"] = data["usergroup"]?.ToString();
            dict["seq"] = data["seq"]?.ToString();
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
