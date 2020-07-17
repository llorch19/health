/*
 * Title : “就诊”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“就诊”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class AttandentController : ControllerBase
    {
        private readonly ILogger<AttandentController> _logger;
        public AttandentController(ILogger<AttandentController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“就诊”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetOrgAttandentList")]
        public JObject GetOrgAttandentList(int orgid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取个人的“就诊”历史
        /// </summary>
        /// <param name="userid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetPersonAttandentList")]
        public JObject GetPersonAttandentList(int userid)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 获取“就诊”信息，点击[科普公告]中的一个项目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“就诊”信息</returns>
        [HttpGet]
        [Route("GetAttandent")]
        public JObject GetAttandent(int id)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 更改“就诊”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAttandent")]
        public JObject SetAttandent([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    req.Remove("publish");
                    req["OrgnizationID"] = null;
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_attandent", dict);
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
                    var rows = db.Update("t_attandent", dict, keys);
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
        /// 删除“就诊”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAttandent")]
        public JObject DelAttandent([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_attandent", dict);
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