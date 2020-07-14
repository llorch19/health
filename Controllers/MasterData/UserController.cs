/*
 * Title : 个人信息管理控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - Get返回值应该同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi 2020-07-14 09:07
 * - Post提交值将同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi  2020-07-14 09:08
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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取个人列表
        /// </summary>
        /// <returns>JSON数组形式的个人信息</returns>
        [HttpGet]
        [Route("GetPatientList")]
        public JObject GetPatientList(int pageIndex)
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";
           
            dbfactory db = new dbfactory();
            JArray rows = db.GetArray(
                @"select ID
                ,IFNULL(OrgName,'') as OrgName
                ,IFNULL(OrgCode,'') as OrgCode
                ,IFNULL(CertCode,'') as CertCode
                ,IFNULL(LegalName,'') as LegalName
                ,IFNULL(LegalIDCode,'') as LegalIDCode
                ,IFNULL(Address,'') as Address
                ,IFNULL(Tel,'') as Tel
                ,IFNULL(Coordinates,'') as Coordinates
                from t_orgnization limit ?p1,10"
                , pageIndex); 
            
            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取机构信息
        /// </summary>
        /// <returns>JSON形式的某个机构信息</returns>
        [HttpGet]
        [Route("GetPatient")]
        public JObject GetPatient(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne(
                @"select ID
                ,IFNULL(OrgName,'') as OrgName
                ,IFNULL(OrgCode,'') as OrgCode
                ,IFNULL(CertCode,'') as CertCode
                ,IFNULL(LegalName,'') as LegalName
                ,IFNULL(LegalIDCode,'') as LegalIDCode
                ,IFNULL(Address,'') as Address
                ,IFNULL(Tel,'') as Tel
                ,IFNULL(Coordinates,'') as Coordinates
                from t_orgnization where id=?p1"
                , id); 
            if(res["id"] != null)
                res["status"] = 200;
            else{
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }
            return res;
        }

        /// <summary>
        /// 更改机构信息。如果id=0新增机构信息，如果id>0修改机构信息。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的机构信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetPatient")]
        public JObject SetPatient([FromBody] JObject req)
        {
            dbfactory db=new dbfactory();
            JObject res=new JObject();
            if(req["id"] !=null)
            {
                int id=req["id"].ToObject<int>();
                if(id==0)
                {
                    var dict=req.ToObject<Dictionary<string,object>>();
                    var rows=db.Insert("t_orgnization", dict);
                    if(rows>0)
                    {
                        res["status"]=200;
                        res["msg"]="新增成功";
                    }
                    else
                    {
                        res["status"]=201;
                        res["msg"]="无法新增数据";
                    }
                }
                else if(id>0)
                {
                    var dict = req.ToObject<Dictionary<string,object>>();
                    dict.Remove("id");
                    var keys = new Dictionary<string,object>();
                    keys["id"]=req["id"];
                    var rows=db.Update("t_orgnization", dict,keys);
                    if(rows>0)
                    {
                       res["status"]=200;
                       res["msg"]="修改成功";
                    }
                    else{
                        res["status"]=201;
                        res["msg"]="修改失败";
                    }
                }
            }
            else{
                res["status"]=201;
                res["message"]="非法的请求";
            }
            return res;
        }

        /// <summary>
        /// 删除机构信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的机构信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelPatient")]
        public JObject DelPatient([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict=req.ToObject<Dictionary<string,object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_orgnization", dict);
            if(count > 0)
            {
                res["status"]=200;
                res["msg"] = "操作成功";
                return res;
            }
            else
            {
                res["status"]=201;
                res["msg"]= "操作失败";
                return res;
            }
        }
    }
}
