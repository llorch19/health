﻿/*
 * Author: nuowei
 * Date  : 2020-07-16
 * Description: 注册用户信息管理
 * Comments
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using util.mysql;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices.ComTypes;

namespace health.Controllers.BaseData
{
    /// <summary>
    /// 登录用户管理
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        dbfactory db = new dbfactory();
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 获取人员信息相关基础资料信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBaseData")]
        public JObject GetBaseData()
        {
            JObject res = new JObject();
            res["status"] = 200;
            common.BaseConfig _area = new common.BaseConfig();
            res["AreaList"] = _area.GetAreaTree();
            res["UserGroup"] = db.GetArray("SELECT id,cname text FROM t_user_group where isActive = 1");
            return res;
        }
        /// <summary>
        /// 获取单个用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}")]
        public JObject GetUser(int id)
        {
            JObject res = new JObject();
            
            string sql = "SELECT id,ChineseName,Username,Email,PhoneNumber,ProvinceID,CityID,CountyID,GroupId,OrgnizationID,IsActive,Address from t_user where id=?p1";
            res = db.GetOne(sql, id);
            if(res["id"] != null)
            {
                common.BaseConfig conf = new common.BaseConfig();
                res["Province"] = conf.GetAreaInfo(res["provinceid"].ToObject<int>());
                res["City"] = conf.GetAreaInfo(res["cityid"].ToObject<int>());
                res["County"] = conf.GetAreaInfo(res["countyid"].ToObject<int>());
                res["UserGroup"] = conf.GetUserGroup(res["groupid"].ToObject<int>());
                res["org"] = conf.GetOrg(res["orgnizationid"].ToObject<int>());

                res["status"] = 200;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "查询不到该记录";
            }
            return res;
        }
        /// <summary>
        /// 用户信息提交/修改
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id:int}")]
        public JObject SetUser(int id, [FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            //req.ToObject<Dictionary<string, object>>();
            dict.Add("ChineseName", req["chinesename"].ToString());
            dict.Add("Email", req["email"].ToString());
            dict.Add("PhoneNumber", req["phonenumber"].ToString());
            dict.Add("ProvinceID", req["provinceid"].ToObject<int>());
            dict.Add("CityID", req["cityid"].ToObject<int>());
            dict.Add("CountyID", req["countyid"].ToObject<int>());
            dict.Add("GroupId", req["groupid"].ToObject<int>());
            dict.Add("OrgnizationID", req["orgnizationid"].ToObject<int>());
            dict.Add("IsActive", req["isactive"].ToObject<int>());
            if (id > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = id;

                db.Update("t_user", dict, condi);
            }
            else
            {
                dict.Add("Username", req["username"].ToString());
                dict["PasswordHash"] = util.Security.String2MD5(req["pass"].ToString());
                id = db.Insert("t_user", dict);
            }
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "";
            res["id"] = id;
            return res;
        }
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <param name="pagesize">每页记录数</param>
        /// <param name="page">页</param>
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        public JObject GetList(int pagesize, int page)
        {
            int offset = 0;
            if (page > 0)
                offset = pagesize * (page - 1);

            JObject res = new JObject();
            res["status"] = 200;
            res["list"] = db.GetArray(string.Format(@"SELECT t1.id,ChineseName,Username,t2.AreaName Province,t3.AreaName City,t4.AreaName County,
t5.cname GroupName,t1.IsActive,t1.PhoneNumber,t6.OrgName
from t_user t1
left JOIN data_area t2 on t1.ProvinceID=t2.id
left JOIN data_area t3 on t1.CityID=t3.id
left JOIN data_area t4 on t1.CountyID=t4.id
inner join t_user_group t5 on t1.GroupId=t5.id
inner join t_orgnization t6 on t1.OrgnizationID=t6.id
order by t1.id desc LIMIT {0},{1}", offset, pagesize));
            return res;
        }
        /// <summary>
        /// 机构列表
        /// </summary>
        /// <param name="cname"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("OrgList")]
        public JObject GetOrgList(string cname="")
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["list"] = db.GetArray("SELECT id,OrgName text FROM t_orgnization where isActive = 1 and IsDeleted=0");
            return res;
        }
    }
}