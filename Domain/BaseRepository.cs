using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public abstract class BaseRepository : IRepository
    {
        protected dbfactory _db;
        public BaseRepository(dbfactory db)
        {
            _db = db;
        }

        public abstract string TableName { get; }
        public abstract Func<JObject, bool> IsAddAction { get; }
        public abstract Func<JObject,bool> IsLockAction { get; }

        public virtual int AddOrUpdateRaw(JObject data,string username)
        {
            if (IsAddAction(data))
            {
                var dict = GetValue(data);
                dict["CreatedBy"] = username;
                dict["CreatedTime"] = DateTime.Now;
                dict["IsActive"] = 0;
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
                    valuedata["IsActive"] = 0;
                    valuedata["IsDeleted"] = 0;
                    return _db.Update(TableName, valuedata, keydata)>0
                        ?GetId(data)
                        :0;
                }
            }
        }

        public virtual int DelRaw(JObject data,string username)
        {
            var valuedata = new Dictionary<string,object>();
            valuedata["IsDeleted"] = 1;
            valuedata["IsActive"] = 0;
            valuedata["LastUpdatedBy"] = username;
            valuedata["LastUpdatedTime"] = DateTime.Now;
            return _db.Update(TableName, valuedata, GetKey(data)) > 0
                        ? GetId(data)
                        : 0;
        }

        public virtual int SetLock(JObject data,string username)
        {
            var value = new Dictionary<string, object>();
            value["IsActive"] = data.ToInt("isactive");
            value["LastUpdatedBy"] = username;
            value["LastUpdatedTime"] = DateTime.Now;
            var keys = GetKey(data);
            var rc = _db.Update(TableName, value, keys);
            return rc;
        }

        public abstract JArray GetListByOrgJointImp(int orgid);
        public abstract JArray GetListByPersonJointImp(int personid);
        public abstract JArray GetListJointImp();
        public abstract JObject GetOneRawImp(int id);
        public abstract Dictionary<string, object> GetValue(JObject data);
        public abstract Dictionary<string, object> GetKey(JObject data);
        public abstract int GetId(JObject data);

        public abstract JObject GetAltInfo(int? id);
    }
}
