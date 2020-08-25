/*
 * Title : 个人信息管理控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - Get返回值应该同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi 2020-07-14 09:07
 * - Post提交值将同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi  2020-07-14 09:08
 * - 使用JObject["personinfo"]=db.GetOne() 这种形式逐个添加所需信息             @norway 2020-07-14 10:24
 * - GetList 需要附带返回Person列表的总条数。                                   @xuedi  2020-07-17  10:47
 * - Post提交值将只包含：个人信息。检测及随访的针对各自的接口进行POST。            @xuedi  2020-07-17  17:21
 * - 先不检查复诊字段       @xuedi  2020-07-22  10:35
 */
using health.BaseData;
using health.common;
using IdGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class PersonController : AbstractBLLController
    {
        private readonly ILogger<PersonController> _logger;
        IdGenerator idGenerator;
        public override string TableName => "t_patient";

        public PersonController(ILogger<PersonController> logger, IdGenerator generator)
        {
            _logger = logger;
            idGenerator = generator;
        }

        /// <summary>
        /// 获取个人列表，[人员转诊]菜单
        /// </summary>
        /// <returns>JSON数组形式的个人信息</returns>
        [HttpGet]
        [Route("GetPersonListD")]
        public override JObject GetList()
        {
            return GetPersonList(10,0);
        }


        /// <summary>
        /// 获取个人列表，[人员转诊]菜单
        /// </summary>
        /// <returns>JSON数组形式的个人信息</returns>
        [HttpGet]
        [Route("GetPersonList")]
        public JObject GetPersonList(int pageSize = 10, int pageIndex = 0)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

            JObject res = new JObject();


            JArray rows = db.GetArray(
                @"SELECT 
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
LEFT JOIN t_attandent
ON t_patient.ID=t_attandent.PatientID
WHERE t_patient.OrgnizationID=?p1
AND t_patient.IsDeleted=0
LIMIT ?p2,?p3"
                , HttpContext.GetIdentityInfo<int?>("orgnizationid"), offset, pageSize);

            // TODO: BUGs here, can not read COUNT(*) which returns Int64
            
            if (rows.HasValues)
            {
                res["total"] = db.GetOne("SELECT COUNT(*) as TOTAL FROM t_patient")["total"];
                res["status"] = 200;
                res["msg"] = "读取成功";
                res["list"] = rows;
                return res;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
                return res;
            }
            
        }

        /// <summary>
        /// 获取人员信息，初始化[人员信息录入]菜单
        /// </summary>
        /// <returns>JSON形式的某位个人信息，包括个人信息，</returns>
        [HttpGet]
        [Route("GetPerson")]
        public override JObject Get(int id)
        {
            common.BaseConfig conf = new common.BaseConfig();
            JObject res = new JObject();

            // 个人信息
            JObject personinfo =
             db.GetOne(
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
            OrganizationController org = new OrganizationController(null);
            personinfo["primaryorg"] = org.GetOrgInfo(personinfo["primaryorgnizationid"].ToObject<int>());
            personinfo["orgnization"] = org.GetOrgInfo(personinfo["orgnizationid"].ToObject<int>());

            GenderController gender = new GenderController(null);
            personinfo["gender"] = gender.GetGenderInfo(personinfo["genderid"].ToObject<int>());
            OccupationController occupation = new OccupationController(null);
            personinfo["occupation"] = occupation.GetOccupationInfo(personinfo["occupationcategoryid"].ToObject<int>());
            AddressCategoryController addresscategory = new AddressCategoryController(null);
            personinfo["addresscategory"] = addresscategory.GetAddressCategoryInfo(personinfo["addresscategoryid"].ToObject<int>());

            personinfo["province"] = conf.GetAreaInfo(personinfo["provinceid"].ToObject<int>());
            personinfo["city"] = conf.GetAreaInfo(personinfo["cityid"].ToObject<int>());
            personinfo["county"] = conf.GetAreaInfo(personinfo["countyid"].ToObject<int>());


            res["personinfo"] = personinfo;
            // 检查信息
            //where IsReexam = 0
            res["checkinfo"] = db.GetArray(@"
SELECT 
IFNULL(t_check.ID,'') as ID
,IFNULL(ReportTime,'') as ReportTime
,IFNULL(data_detectionresulttype.ResultName,'') as ResultName
,IFNULL(t_check.IsRexam,'') as IsReexam
from t_check
LEFT JOIN data_detectionresulttype
ON t_check.ResultTypeID=data_detectionresulttype.ID
WHERE PatientID=?p1
and t_check.IsDeleted=0
ORDER BY ReportTime,ID DESC"
                , id);

            res["treatinfo"] = db.GetArray(@"
SELECT
IFNULL(t_treat.ID,'') AS ID
,IFNULL(t_treat.PrescribeTime,'') AS PrescribeTime
,IFNULL(t_medication.`Name`,'') AS MedicationName
FROM t_treatitem
LEFT JOIN t_medication
ON t_treatitem.MedicationID=t_medication.ID
LEFT JOIN t_treat
ON t_treatitem.TreatID=t_treat.ID
WHERE t_treat.PatientID=?p1
AND t_treatitem.IsDeleted=0", id);

            // 随访信息
            res["followupinfo"] = db.GetArray(@"
SELECT
IFNULL(ID,'') as ID
,IFNULL(Time,'') as Time
,IFNULL(PersonList,'') as PersonList
,IFNULL(Abstract,'') as Abstract
FROM t_followup
WHERE PatientID=?p1
AND t_followup.IsDeleted=0
", id);

            res["vaccinfo"] = db.GetArray(@"
SELECT 
IFNULL(t_vacc.ID,'') AS ID
,IFNULL(t_vacc.OperationTime,'') AS OperationTime
,IFNULL(t_medication.CommonName,'') AS CommonName
,IFNULL(t_medication.ESC,'') AS ESC
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_user.ChineseName,'') AS Operator
,IFNULL(t_vacc.IsActive,'') AS IsActive
FROM t_vacc
LEFT JOIN t_medication
ON t_vacc.MedicationID=t_medication.ID
LEFT JOIN t_orgnization
ON t_vacc.OrgnizationID=t_orgnization.ID
LEFT JOIN t_user
ON t_vacc.OperationUserID=t_user.ID
WHERE t_vacc.PatientID=?p1
AND t_vacc.IsDeleted=0
", id);

            if (res["personinfo"].HasValues)
            {
                res["status"] = 200;
                res["msg"] = "读取数据成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }

            return res;
        }

        /// <summary>
        /// 更改个人信息。如果id=0新增个人信息，如果id>0修改个人信息。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的个人信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetPerson")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }

        /// <summary>
        /// 删除个人信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的个人信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelPerson")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        /// <summary>
        /// TODO: 转诊
        /// </summary>
        /// <param name="req">在请求body中JSON形式的转诊信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("Transfer")]
        public JObject Transfer([FromBody] JObject req)
        {
            JObject res = new JObject();
            res["status"] = 201;
            res["msg"] = "功能正在开发中";
            return res;
        }


        [HttpGet]
        [Route("GetId")]
        public string GetId()
        {
            return idGenerator.CreateId().ToString();
        }


        [NonAction]
        public JObject GetPersonInfo(int? id)
        {
            JObject res = db.GetOne("select id,FamilyName text,IDCardNO code ,InviteCode invite from t_patient where id=?p1 and t_patient.IsDeleted=0", id);
            return res;
        }


        [NonAction]
        public JObject GetUserInfo(int? id)
        {
            JObject res = db.GetOne("select id,ChineseName text,IDCardNO code from t_user where id=?p1 and t_user.IsDeleted=0", id);
            return res;
        }

        
        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = req.ToInt("orgnizationid");
            dict["PrimaryOrgnizationID"] = req.ToInt("primaryorgnizationid");
            dict["Tel"] = req["tel"]?.ToObject<string>();
            dict["IDCardNO"] = req["idcardno"]?.ToObject<string>();
            dict["GenderID"] = req.ToInt("genderid");
            dict["FamilyName"] = req["familyname"]?.ToObject<string>();
            DateTime dt;
            if (DateTime.TryParse(req["birthday"].ToObject<string>(), out dt))
            {
                dict["Birthday"] = dt;
            }


            if (req.ToInt("id")==0)
            {
                // 新增人员生成邀请码和档案号
                dict["InviteCode"] = ShareCodeUtils.New();
                dict["RegisterNO"] = idGenerator.CreateId();
            }


            dict["Nation"] = req["nation"]?.ToObject<string>();
            dict["DomicileType"] = req["domiciletype"]?.ToObject<string>();
            dict["DomicileType"] = req["domiciletype"]?.ToObject<string>();
            dict["DomicileDetail"] = req["domiciledetail"]?.ToObject<string>();
            dict["WorkUnitName"] = req["workunitname"]?.ToObject<string>();
            dict["OccupationCategoryID"] = req.ToInt("occupationcategoryid");
            dict["Detainees"] = req["detainees"]?.ToObject<string>();
            dict["AddressCategoryID"] = req.ToInt("addresscategoryid");
            dict["Address"] = req["address"]?.ToObject<string>();
            dict["GuardianName"] = req["guardianname"]?.ToObject<string>();
            dict["GuardianContact"] = req["guardiancontact"]?.ToObject<string>();
            dict["ProvinceID"] = req.ToInt("provinceid");
            dict["CityID"] = req.ToInt("cityid");
            dict["CountyID"] = req.ToInt("countyid");
            dict["TownAddr"] = req["townaddr"]?.ToObject<string>();
            dict["VillageAddr"] = req["villageaddr"]?.ToObject<string>();
            dict["HouseNumberAddr"] = req["housenumberaddr"]?.ToObject<string>();
            dict["PostalCode"] = req["postalcode"]?.ToObject<string>();
            dict["AreaCode"] = req["areacode"]?.ToObject<string>();
            dict["DomicileType"] = req.ToInt("domiciletype");
            dict["DomicileChosen"] = req["domicilechosen"]?.ToObject<string>();
            dict["DomicileStandard"] = req["domicilestandard"]?.ToObject<string>();
            dict["DomicileDetail"] = req["domiciledetail"]?.ToObject<string>();
            dict["WorkUnitName"] = req["workunitname"]?.ToObject<string>();
            dict["WorkUnitContact"] = req["workunitcontact"]?.ToObject<string>();
            dict["Email"] = req["email"]?.ToObject<string>();
            dict["GuardianName"] = req["guardianname"]?.ToObject<string>();
            dict["GuardianContact"] = req["guardiancontact"]?.ToObject<string>();
            dict["GuardianName"] = req["guardianname"]?.ToObject<string>();
            dict["GuardianEmail"] = req["guardianemail"]?.ToObject<string>();




            return dict;
        }

    }
}
