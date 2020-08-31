﻿using IdGen;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class PersonRepository : BaseRepository
    {
        public PersonRepository(dbfactory db) : base(db) { }

        public override string TableName => "t_patient";

        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;

        public override Func<JObject, bool> IsLockAction => req => false;

        public override JObject GetAltInfo(int? id)
        {
            JObject res = _db.GetOne(@"
select 
id
,FamilyName text
,IDCardNO code 
,InviteCode invite 
from t_patient 
where id=?p1 
and t_patient.IsDeleted=0
", id);
            return res;
        }

        public JObject GetUserAltInfo(int? id)
        {
            JObject res = _db.GetOne(@"
select 
id
,ChineseName text
,IDCardNO code 
from t_user 
where id=?p1 
and t_user.IsDeleted=0
", id);
            return res;
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }

        public string CreateId([FromServices] IdGenerator idgen)
        {
            return idgen.CreateId().ToString();
        }

       

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            JArray array = _db.GetArray(@"
SELECT 
IFNULL(t_patient.ID,'') as ID
,IFNULL(t_attandent.IsReferral,'') as IsReferral
,IFNULL(t_patient.OrgnizationID,'') as OrgnizationID
,IFNULL(PrimaryOrgnizationID,'') as PrimaryOrgnizationID
,IFNULL(OrgName,'') as OrgName
,IFNULL(OrgCode,'') as OrgCode
,IFNULL(InviteCode,'') as InviteCode
,IFNULL(RegisterNO,'') as RegisterNO
,IFNULL(FamilyName,'') as FamilyName
,IFNULL(t_patient.Tel,'') as Tel
,IFNULL(t_patient.IDCardNO,'') as IDCardNO
,IFNULL(GenderID,'') as GenderID
,IFNULL(GenderName,'') as GenderName
,IFNULL(t_patient.Birthday,'') as Birthday
,IFNULL(t_patient.Nation,'') as Nation
,IFNULL(DomicileType,'') as DomicileType
,IFNULL(t_patient.DomicileDetail,'') as DomicileDetail
,IFNULL(WorkUnitName,'') as WorkUnitName
,IFNULL(OccupationCategoryID,'') as OccupationCategoryID
,IFNULL(data_occupation.OccupationName,'') as OccupationName
,IFNULL(Detainees,'') as Detainees
,IFNULL(AddressCategoryID,'') as AddressCategoryID
,IFNULL(data_addresscategory.AddressCategory,'') as AddressCategory
,IFNULL(t_patient.Address,'') as Address
,IFNULL(GuardianName,'') as GuardianName
,IFNULL(GuardianContact,'') as GuardianContact
,IFNULL(t_patient.ProvinceID,'') as ProvinceID
,IFNULL(Province.AreaName,'') as Province
,IFNULL(t_patient.CityID,'') as CityID
,IFNULL(City.AreaName,'') as City
,IFNULL(t_patient.CountyID,'') as CountyID
,IFNULL(County.AreaName,'') as County
,IFNULL(t_patient.IsActive,'') as IsActive
,IFNULL(t_transfer.IsCancel,'') as TransferCancel
,IFNULL(t_transfer.IsFinish,'') as TransferFinish
FROM t_patient 
LEFT JOIN t_orgnization
ON t_patient.PrimaryOrgnizationID=t_orgnization.ID
LEFT JOIN data_gender
ON t_patient.GenderID=data_gender.ID
LEFT JOIN data_occupation
ON t_patient.OccupationCategoryID=data_occupation.ID
LEFT JOIN data_addresscategory
ON t_patient.AddressCategoryID=data_addresscategory.ID
LEFT JOIN data_area Province
ON t_patient.ProvinceID=Province.ID
LEFT JOIN data_area City
ON t_patient.CityID=City.ID
LEFT JOIN data_area County
ON t_patient.CountyID=County.ID
LEFT JOIN t_transfer
ON t_patient.ID = t_transfer.PatientID
LEFT JOIN t_attandent
ON t_attandent.PatientID=t_patient.ID
WHERE t_patient.OrgnizationID=?p1
AND t_patient.IsDeleted=0
LIMIT ?p2,?p3
"
, orgid
,offset
,pageSize);
            return array;
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListJointImp(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            throw new NotImplementedException();
        }

        public override JObject GetOneRawImp(int id)
        {
            var res = _db.GetOne(
                @"SELECT 
IFNULL(t_patient.ID,'') as ID
,IFNULL(t_attandent.IsReferral,'') as IsReferral
,IFNULL(t_patient.OrgnizationID,'') as OrgnizationID
,IFNULL(PrimaryOrgnizationID,'') as PrimaryOrgnizationID
,IFNULL(RegisterNO,'') as RegisterNO
,IFNULL(InviteCode,'') as InviteCode
,IFNULL(FamilyName,'') as FamilyName
,IFNULL(t_patient.Tel,'') as Tel
,IFNULL(t_patient.IDCardNO,'') as IDCardNO
,IFNULL(GenderID,'') as GenderID
,IFNULL(t_patient.Birthday,'') as Birthday
,IFNULL(t_patient.Nation,'') as Nation
,IFNULL(DomicileType,'') as DomicileType
,IFNULL(t_patient.DomicileDetail,'') as DomicileDetail
,IFNULL(WorkUnitName,'') as WorkUnitName
,IFNULL(OccupationCategoryID,'') as OccupationCategoryID
,IFNULL(Detainees,'') as Detainees
,IFNULL(AddressCategoryID,'') as AddressCategoryID
,IFNULL(t_patient.Address,'') as Address
,IFNULL(GuardianName,'') as GuardianName
,IFNULL(GuardianContact,'') as GuardianContact
,IFNULL(t_patient.ProvinceID,'') as ProvinceID
,IFNULL(t_patient.CityID,'') as CityID
,IFNULL(t_patient.CountyID,'') as CountyID
,IFNULL(t_patient.IsActive,'') as IsActive
FROM t_patient 
LEFT JOIN t_attandent
ON t_patient.ID=t_attandent.PatientID
where t_patient.ID=?p1
and t_patient.IsDeleted=0"
                , id);
            return res;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["IsDeleted"] = 0; // IsDeleted=0 的记录可以被查看
            dict["IsActive"] = 1;  // IsActive=1 的记录可以被修改和删除
            return dict;
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["PrimaryOrgnizationID"] = data.ToInt("primaryorgnizationid");
            dict["Tel"] = data["tel"]?.ToObject<string>();
            dict["IDCardNO"] = data["idcardno"]?.ToObject<string>();
            dict["GenderID"] = data.ToInt("genderid");
            dict["FamilyName"] = data["familyname"]?.ToObject<string>();
            dict["Nation"] = data["nation"]?.ToObject<string>();
            dict["DomicileType"] = data["domiciletype"]?.ToObject<string>();
            dict["DomicileDetail"] = data["domiciledetail"]?.ToObject<string>();
            dict["WorkUnitName"] = data["workunitname"]?.ToObject<string>();
            dict["OccupationCategoryID"] = data.ToInt("occupationcategoryid");
            dict["Detainees"] = data["detainees"]?.ToObject<string>();
            dict["AddressCategoryID"] = data.ToInt("addresscategoryid");
            dict["Address"] = data["address"]?.ToObject<string>();
            dict["GuardianName"] = data["guardianname"]?.ToObject<string>();
            dict["GuardianContact"] = data["guardiancontact"]?.ToObject<string>();
            dict["ProvinceID"] = data.ToInt("provinceid");
            dict["CityID"] = data.ToInt("cityid");
            dict["CountyID"] = data.ToInt("countyid");
            dict["TownAddr"] = data["townaddr"]?.ToObject<string>();
            dict["VillageAddr"] = data["villageaddr"]?.ToObject<string>();
            dict["HouseNumberAddr"] = data["housenumberaddr"]?.ToObject<string>();
            dict["PostalCode"] = data["postalcode"]?.ToObject<string>();
            dict["AreaCode"] = data["areacode"]?.ToObject<string>();
            dict["DomicileType"] = data.ToInt("domiciletype");
            dict["DomicileChosen"] = data["domicilechosen"]?.ToObject<string>();
            dict["DomicileStandard"] = data["domicilestandard"]?.ToObject<string>();
            dict["DomicileDetail"] = data["domiciledetail"]?.ToObject<string>();
            dict["WorkUnitName"] = data["workunitname"]?.ToObject<string>();
            dict["WorkUnitContact"] = data["workunitcontact"]?.ToObject<string>();
            dict["Email"] = data["email"]?.ToObject<string>();
            dict["GuardianContact"] = data["guardiancontact"]?.ToObject<string>();
            dict["GuardianName"] = data["guardianname"]?.ToObject<string>();
            dict["GuardianEmail"] = data["guardianemail"]?.ToObject<string>();
            dict["Birthday"] = data.ToDateTime("birthday");
            return dict;
        }

        public override Dictionary<string, object> GetPostDelSetting(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["IDCardNO"] = null;
            return dict;
        }


        public override int AddOrUpdateRaw(JObject data, string username)
        {
            if (IsAddAction(data))
            {
                var dict = GetValue(data);
                dict["InviteCode"] = data["invitecode"]?.ToObject<string>();
                dict["RegisterNO"] = data["registerno"]?.ToObject<string>();
                dict["CreatedBy"] = username;
                dict["CreatedTime"] = DateTime.Now;
                dict["IsActive"] = 1;  // 新增的默认是激活的,如果Repository需要自动锁定新增，在AddOrUpdate之后调用SetLock()
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
                    valuedata["IsActive"] = 1;  // 修改后为 IsActive = true
                    valuedata["IsDeleted"] = 0;
                    return _db.Update(TableName, valuedata, keydata) > 0
                        ? GetId(data)
                        : 0;
                }
            }
        }
    }
}