using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class OrgnizationRepository : BaseRepository
    {
        public OrgnizationRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_orgnization";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public JArray GetListJointImp(int provinceid = 0, int cityid = 0, int countyid = 0)
        {
            JArray rows = _db.GetArray(
               @"
SELECT 
one.ID
,IFNULL(one.OrgName,'') as OrgName
,IFNULL(one.OrgCode,'') as OrgCode
,IFNULL(one.CertCode,'') as CertCode
,IFNULL(one.LegalName,'') as LegalName
,IFNULL(one.LegalIDCode,'') as LegalIDCode
,IFNULL(one.Address,'') as Address
,IFNULL(one.Tel,'') as Tel
,IFNULL(one.Coordinates,'') as Coordinates
,IFNULL(one.ParentID,'') as ParentID
,IFNULL(parent.OrgName,'') as Parent
,IFNULL(one.ProvinceID,'') as ProvinceID
,IFNULL(province.AreaName,'') as Province
,IFNULL(one.CityID,'') as CityID
,IFNULL(city.AreaName,'') as City
,IFNULL(one.CountyID,'') as CountyID
,IFNULL(county.AreaName,'') as County
,IFNULL(one.IsActive,'') as IsActive
FROM t_orgnization one 
LEFT JOIN t_orgnization parent
ON one.ParentID=parent.ID
LEFT JOIN data_area province
ON one.ProvinceID=province.ID
LEFT JOIN data_area city
ON one.CityID=city.ID
LEFT JOIN data_area county
ON one.CountyID=county.ID
WHERE one.ProvinceID=?p1
AND one.CityID=?p2
AND one.CountyID=?p3
AND one.IsDeleted=0", provinceid, cityid, countyid);
            return rows;
        }

        public override JArray GetListJointImp(int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
one.ID
,IFNULL(one.OrgName,'') as OrgName
,IFNULL(one.OrgCode,'') as OrgCode
,IFNULL(one.CertCode,'') as CertCode
,IFNULL(one.LegalName,'') as LegalName
,IFNULL(one.LegalIDCode,'') as LegalIDCode
,IFNULL(one.Address,'') as Address
,IFNULL(one.Tel,'') as Tel
,IFNULL(one.Coordinates,'') as Coordinates
,IFNULL(one.ParentID,'') as ParentID
,IFNULL(parent.OrgName,'') as Parent
,IFNULL(one.ProvinceID,'') as ProvinceID
,IFNULL(province.AreaName,'') as Province
,IFNULL(one.CityID,'') as CityID
,IFNULL(city.AreaName,'') as City
,IFNULL(one.CountyID,'') as CountyID
,IFNULL(county.AreaName,'') as County
,IFNULL(one.IsActive,'') as IsActive
FROM t_orgnization one 
LEFT JOIN t_orgnization parent
ON one.ParentID=parent.ID
LEFT JOIN data_area province
ON one.ProvinceID=province.ID
LEFT JOIN data_area city
ON one.CityID=city.ID
LEFT JOIN data_area county
ON one.CountyID=county.ID
WHERE one.IsDeleted=0
LIMIT ?p1,?p2
", offset,pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT 
ID
,IFNULL(OrgName,'') as OrgName
,IFNULL(OrgCode,'') as OrgCode
,IFNULL(CertCode,'') as CertCode
,IFNULL(LegalName,'') as LegalName
,IFNULL(LegalIDCode,'') as LegalIDCode
,IFNULL(Address,'') as Address
,IFNULL(Tel,'') as Tel
,IFNULL(Coordinates,'') as Coordinates
,IFNULL(ParentID,'') as ParentID
,IFNULL(ProvinceID,'') as ProvinceID
,IFNULL(CityID,'') as CityID
,IFNULL(CountyID,'') as CountyID
,IFNULL(IsActive,'') as IsActive
FROM t_orgnization 
WHERE ID=?p1
AND IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgName"] = data["orgname"]?.ToObject<string>();
            dict["OrgCode"] = data["orgcode"]?.ToObject<string>();
            dict["CertCode"] = data["certcode"]?.ToObject<string>();
            dict["LegalName"] = data["legalname"]?.ToObject<string>();
            dict["LegalIdCode"] = data["legalidcode"]?.ToObject<string>();
            dict["Address"] = data["address"]?.ToObject<string>();
            dict["Tel"] = data["tel"]?.ToObject<string>();
            dict["Coordinates"] = data["coordinates"]?.ToObject<string>();
            dict["ParentID"] = data.ToInt("parentid");
            dict["ProvinceID"] = data.ToInt("provinceid");
            dict["CityID"] = data.ToInt("cityid");
            dict["CountyID"] = data.ToInt("countyid");
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
select id,OrgName text,OrgCode code,CertCode register 
from t_orgnization 
where id=?p1
and IsDeleted = 0
", id);
        }
    }
}
