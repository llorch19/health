/*
 * Title : “机构”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“机构”信息的增删查改
 * Comments
 * - 多行，SQL字符串可以用@符号来写，这样可以有效减少+号的拼接。 @norway 2020-07-14 09:56
 * - Org需要 ParentID以及 上级机构的名字 ParentName           @xuedi  2020-07-16 15:59
 * 
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class OrgnizationController : ControllerBase
    {

        private readonly ILogger<OrgnizationController> _logger;

        public OrgnizationController(ILogger<OrgnizationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgList")]
        public JObject GetOrgList(int pageIndex)
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray(
                @"select 
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
,IFNULL(parent.OrgName,'') as ParentName
FROM t_orgnization one 
LEFT JOIN t_orgnization parent
ON one.ParentID=parent.ID
LIMIT ?p1,10"
                , pageIndex);

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
                @"select 
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
,IFNULL(parent.OrgName,'') as ParentName
FROM t_orgnization one 
LEFT JOIN t_orgnization parent
ON one.ParentID=parent.ID
WHERE one.ProvinceID=?p1
AND one.CityID=?p2
AND one.CountyID=?p3
"
                , provinceid,cityid,countyid);

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“机构”信息
        /// </summary>
        /// <returns>JSON形式的某个“机构”信息</returns>
        [HttpGet]
        [Route("GetOrg")]
        public JObject GetOrg(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne(
                @"select 
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
,IFNULL(parent.OrgName,'') as ParentName
,IFNULL(one.ProvinceID,'') as ProvinceID
,IFNULL(one.ProvinceAddr,'') as ProvinceAddr
,IFNULL(one.CityID,'') as CityID
,IFNULL(one.CityAddr,'') as CityAddr
,IFNULL(one.CountyID,'') as CountyID
,IFNULL(one.CountyAddr,'') as CountyAddr
FROM t_orgnization one 
LEFT JOIN t_orgnization parent
ON one.ParentID=parent.ID
WHERE one.id=?p1"
                , id);
            if (res["id"] != null)
            {
                res["status"] = 200;
                res["msg"] = "读取成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
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
        public JObject SetOrg([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                req.Remove("parentname");
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_orgnization", dict);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "新增成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "无法新增数据";
                    }
                }
                else if (id > 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    dict.Remove("id");
                    var keys = new Dictionary<string, object>();
                    keys["id"] = req["id"];
                    var rows = db.Update("t_orgnization", dict, keys);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "修改成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "修改失败";
                    }
                }
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "非法的请求";
            }
            return res;
        }

        /// <summary>
        /// 删除“机构”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“机构”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelOrg")]
        public JObject DelOrg([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_orgnization", dict);
            if (count > 0)
            {
                res["status"] = 200;
                res["msg"] = "操作成功";
                return res;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "操作失败";
                return res;
            }
        }
    }
}
