/*
 * Title : “机构”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“机构”信息的增删查改
 * Comments
 * - 多行，SQL字符串可以用@符号来写，这样可以有效减少+号的拼接。 @norway 2020-07-14 09:56
 * - Org需要 ParentID以及 上级机构的名字 ParentName           @xuedi  2020-07-16 15:59
 * - Post Org不提交ProvinceAddr,CityAddr,CountyAddr          @xuedi,norway 2020-07-17  09:10
 * - 个人档案中读取组织机构时，需要读取id,text(OrgName),code(OrgCode),register(CertCode) @xuedi  2020-07-20  09:20
 * 
 * */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class OrganizationController : AbstractBLLController
    {
        private readonly ILogger<OrganizationController> _logger;
        public override string TableName => "t_orgnization";

        public OrganizationController(ILogger<OrganizationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgListD")]
        public override JObject GetList()
        {
            return GetOrgList(10,0);
        }

        /// <summary>
        /// 获取“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgList")]
        public JObject GetOrgList(int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray(
                @"SELECT 
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
LIMIT ?p1,?p2"
                , offset, pageSize);

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取指定区县级行政地址下的“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgListv2")]
        public JObject GetOrgList(int provinceid = 0, int cityid = 0, int countyid = 0)
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray(
                @"SELECT 
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
AND one.IsDeleted=0
"
                , provinceid, cityid, countyid);

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“机构”信息
        /// </summary>
        /// <returns>JSON形式的某个“机构”信息</returns>
        [HttpGet]
        [Route("GetOrg")]
        public override JObject Get(int id)
        {

            dbfactory db = new dbfactory();
            JObject res = db.GetOne(
                @"SELECT 
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
AND IsDeleted=0"
                , id);

            if (res["id"]==null)
            {
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }
            else
            {
                AreaController area = new AreaController(null);
                res["province"] = area.GetAreaInfo(res.ToInt("provinceid"));
                res["city"] = area.GetAreaInfo(res.ToInt("cityid"));
                res["county"] = area.GetAreaInfo(res.ToInt("countyid"));
                res["parent"] = this.GetOrgInfo(res.ToInt("parentid")); 
                res["status"] = 200;
                res["msg"] = "读取成功";
            }

            return res;
        }

        /// <summary>
        /// 更改“机构”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“机构”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetOrg")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }

        /// <summary>
        /// 删除“机构”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“机构”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelOrg")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetOrgInfo(int? id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,OrgName text,OrgCode code,CertCode register from t_orgnization where id=?p1", id);
            return res;
        }

       

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgName"] = req["orgname"]?.ToObject<string>();
            dict["OrgCode"] = req["orgcode"]?.ToObject<string>();
            dict["CertCode"] = req["certcode"]?.ToObject<string>();
            dict["LegalName"] = req["legalname"]?.ToObject<string>();
            dict["LegalIdCode"] = req["legalidcode"]?.ToObject<string>();
            dict["Address"] = req["address"]?.ToObject<string>();
            dict["Tel"] = req["tel"]?.ToObject<string>();
            dict["Coordinates"] = req["coordinates"]?.ToObject<string>();
            dict["ParentID"] = req.ToInt("parentid");
            dict["ProvinceID"] = req.ToInt("provinceid");
            dict["CityID"] = req.ToInt("cityid");
            dict["CountyID"] = req.ToInt("countyid");


            return dict;
        }
    }
}
