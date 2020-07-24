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
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class NationController : AbstractBLLController
    {

        private readonly ILogger<NationController> _logger;
        public override string TableName => "data_nation";

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
        public override JObject GetList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            JArray rows = db.GetArray("select ID,Code,Name from data_nation where IsActive=1 and IsDeleted=0");

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
        public override JObject Get(int id)
        {
            JObject res = db.GetOne("select ID,Code,Name from data_nation where id=?p1 and IsDeleted=0", id);
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
        public override JObject Set([FromBody] JObject req)
        {

            return base.Set(req);
        }


        /// <summary>
        /// 删除“民族”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“民族”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelNation")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetNationInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,Name text from data_nation where id=?p1 and IsDeleted=0", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();


            return dict;
        }
    }
}
