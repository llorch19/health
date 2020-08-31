using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class MedicationRepository : BaseRepository
    {
        public MedicationRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_medication";
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
IFNULL(ID,'') AS ID
,IFNULL(`Name`,'') AS `Name`
,IFNULL(CommonName,'') AS CommonName
,IFNULL(Specification,'') AS Specification
,IFNULL(ESC,'') AS ESC
,IFNULL(ProductionDate,'') AS ProductionDate
,IFNULL(ExpiryDate,'') AS ExpiryDate
,IFNULL(Manufacturer,'') AS Manufacturer
,IFNULL(t_medication.IsActive,'') AS IsActive
FROM t_medication
WHERE t_medication.IsDeleted=0
LIMIT ?p1,?p2
", offset,pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT
IFNULL(ID,'') AS ID
,IFNULL(`Name`,'') AS `Name`
,IFNULL(CommonName,'') AS CommonName
,IFNULL(Specification,'') AS Specification
,IFNULL(ESC,'') AS ESC
,IFNULL(ProductionDate,'') AS ProductionDate
,IFNULL(ExpiryDate,'') AS ExpiryDate
,IFNULL(Manufacturer,'') AS Manufacturer
,IFNULL(t_medication.IsActive,'') AS IsActive
FROM t_medication
WHERE ID=?p1
AND t_medication.IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Name"] = data["name"]?.ToObject<string>();
            dict["CommonName"] = data["commonname"]?.ToObject<string>();
            dict["Specification"] = data["specification"]?.ToObject<string>();
            dict["ESC"] = data["esc"]?.ToObject<string>();
            dict["ProductionDate"] = data["productiondate"]?.ToObject<string>();
            dict["ExpiryDate"] = data["expirydate"]?.ToObject<string>();
            dict["Manufacturer"] = data["manufacturer"]?.ToObject<string>();
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
SELECT id,Name text,ESC code 
FROM t_medication 
where id=?p1
AND IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetPostDelSetting(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ESC"] = null;  // 删除检测产品时，清空其ESC
            return dict;
        }
    }
}
