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
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class OptionController : ControllerBase
    {

        private readonly ILogger<OptionController> _logger;

        public OptionController(ILogger<OptionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“参数”列表
        /// </summary>
        /// <param name="section">节名</param>
        /// <returns>JSON数组形式的“参数”信息</returns>
        [HttpGet]
        [Route("GetOptionList")]
        public JObject GetOptionList(string section=null)
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
        public JObject GetOption(int id)
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
        public JObject SetOption([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_option", dict);
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
                    var rows = db.Update("t_option", dict, keys);
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
        /// 删除“参数”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“参数”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelOption")]
        public JObject DelOption([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("t_option", dict);
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
