using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using util.mysql;
using System.Data;

namespace health.common
{
    /// <summary>
    /// 基础公用类
    /// </summary>
    public class BaseConfig
    {
        public JArray GetAreaTree()
        {
            Object area = new JObject();
            JArray topNode = new JArray();
            IDictionary<int, JArray> node = new Dictionary<int, JArray>();
            string sql = @"SELECT id,AreaName, parentID FROM data_area  ORDER BY AreaCode";
            dbfactory db = new dbfactory();

            using (IDataReader reader = db.Dbhelper.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    JObject obj = new JObject();
                    obj["id"] = reader.GetInt32(0);
                    obj["text"] = reader.GetString(1);
                    obj["name"] = reader.GetString(1);
                    //obj["jb"] = reader.GetInt32(3) / 2;

                    if (reader.GetInt32(2) == 0)
                    {
                        topNode.Add(obj);
                        continue;
                    }
                    if (node.ContainsKey(reader.GetInt32(2)))
                    {
                        JArray arr = node[reader.GetInt32(2)];
                        arr.Add(obj);
                    }
                    else
                    {
                        JArray arr = new JArray();
                        arr.Add(obj);
                        node.Add(reader.GetInt32(2), arr);
                    }
                }
            }
            foreach (JObject obj in topNode)
                getArea(obj["id"].ToObject<int>(), obj, node);
            return topNode;
        }
        /// <summary>
        /// 递归调用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obj"></param>
        /// <param name="node"></param>
        void getArea(int id, JObject obj, IDictionary<int, JArray> node)
        {
            if (node.ContainsKey(id))
            {
                JArray arr = node[id];
                obj["children"] = arr;
                foreach (JObject _obj in arr)
                    getArea(_obj["id"].ToObject<int>(), _obj, node);
            }
        }
        public JObject GetUserGroup(int gid)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,cname text from t_user_group where id=?p1", gid);
            return res;
        }
        public JObject GetAreaInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,AreaName text from data_area where id=?p1", id);
            return res;
        }
        public JObject GetOrg(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("SELECT id,OrgName text FROM t_orgnization where id=?p1", id);
            return res;
        }
    }
}
