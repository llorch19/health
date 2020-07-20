/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-20
 * Description: 对“接种记录”信息的增删查改
 * Comments
 * - 需要民族控制器，支持增删查改。    @xuedi  2020-07-20 16:55
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class NationController : ControllerBase
    {

        private readonly ILogger<NationController> _logger;

        public NationController(ILogger<NationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“民族”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“民族”数组</returns>
        [HttpGet]
        [Route("GetNationList")]
        public JObject GetNationList(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,Name from data_nation");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“民族”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“民族”信息</returns>
        [HttpGet]
        [Route("GetNation")]
        public JObject GetNation(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Code,Name from data_nation where id=?p1", id);
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
        /// 修改“民族”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“民族”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetNation")]
        public JObject SetNation([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("data_nation", dict);
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
                    var rows = db.Update("data_nation", dict, keys);
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
        /// 删除“民族”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“民族”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelNation")]
        public JObject DelNation([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_nation", dict);
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

        [NonAction]
        public JObject GetNationInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,Name text from data_nation where id=?p1", id);
            return res;
        }
    }
}
