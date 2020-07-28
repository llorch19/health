/*
 * Title : 参数信息控制器
 * Author: zudan
 * Date  : 2020-07-15
 * Description: 对全局或特定于机构的参数信息进行增删查改
 * Comments
 * - 支持其他可变参数   @xuedi  2020-07-15 09:52
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class OptionController : AbstractBLLController
    {

        private readonly ILogger<OptionController> _logger;
        public override string TableName => "t_option";

        public OptionController(ILogger<OptionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“参数”列表
        /// </summary>
        /// <returns>JSON数组形式的“参数”信息</returns>
        [HttpGet]
        [Route("GetOptionListD")]
        public override JObject GetList()
        {
            return GetOptionList(null);
        }

        /// <summary>
        /// 获取“参数”列表
        /// </summary>
        /// <param name="section">节名</param>
        /// <returns>JSON数组形式的“参数”信息</returns>
        [HttpGet]
        [Route("GetOptionList")]
        public JObject GetOptionList(string section = null)
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            string sql = null;
            if (section == null)
                sql = @"SELECT 
IFNULL(id,'') AS id
,IFNULL(section,'') AS section
,IFNULL(`name`,'') AS `name`
,IFNULL(`value`,'') AS `value`
,IFNULL(`description`,'') AS `description`
FROM t_option
";
            else
                sql = @"SELECT 
IFNULL(id,'') AS id
,IFNULL(section,'') AS section
,IFNULL(`name`,'') AS `name`
,IFNULL(`value`,'') AS `value`
,IFNULL(`description`,'') AS `description`
FROM t_option
WHERE section=?p1
";
            JArray rows = db.GetArray(
                sql
                , section);

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“参数”信息
        /// </summary>
        /// <returns>JSON形式的某个“参数”信息</returns>
        [HttpGet]
        [Route("GetOption")]
        public override JObject Get(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne(
                @"SELECT 
IFNULL(id,'') AS id
,IFNULL(section,'') AS section
,IFNULL(`name`,'') AS `name`
,IFNULL(`value`,'') AS `value`
,IFNULL(`description`,'') AS `description`
FROM t_option
WHERE id=?p1"
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
        /// 更改“参数”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“参数”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetOption")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }

        /// <summary>
        /// 删除“参数”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“参数”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelOption")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

      

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["section"] = req["orgname"]?.ToObject<string>();
            dict["name"] = req["orgcode"]?.ToObject<string>();
            dict["value"] = req["certcode"]?.ToObject<string>();
            dict["description"] = req["legalname"]?.ToObject<string>();
            //dict["OrgnizationID"] = req["legalidcode"]?.ToObject<string>();

            return dict;
        }
    }
}
