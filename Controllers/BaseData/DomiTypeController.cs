/*
 * Title : “户籍类型”控制器
 * Author: zudan
 * Date  : 2020-07-20
 * Description: 对“户籍类型”信息的增删查改
 * Comments
 * - 需要户籍类型控制器，支持增删查改。    @xuedi  2020-07-20 16:55
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
    public class DomiTypeController : ControllerBase
    {

        private readonly ILogger<DomiTypeController> _logger;
        dbfactory db = new dbfactory();
        public DomiTypeController(ILogger<DomiTypeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“户籍类型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“户籍类型”数组</returns>
        [HttpGet]
        [Route("GetDomiTypeList")]
        public JObject GetDomiTypeList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Name from data_domitype");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“户籍类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“户籍类型”信息</returns>
        [HttpGet]
        [Route("GetDomiType")]
        public JObject GetDomiType(int id)
        {
            JObject res = db.GetOne("select ID,Name from data_domitype where id=?p1", id);
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
        /// 修改“户籍类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“户籍类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetDomiType")]
        public JObject SetDomiType([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = req["id"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();


            if (req["id"].ToObject<int>() > 0)
            {
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                var tmp = this.db.Update("data_domitype", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("data_domitype", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }


        /// <summary>
        /// 删除“户籍类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“户籍类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelDomiType")]
        public JObject DelDomiType([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            var count = db.del("data_domitype", dict);
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
        public JObject GetDomiTypeInfo(int id)
        {
            JObject res = db.GetOne("select id,Name text from data_domitype where id=?p1", id);
            return res;
        }
    }
}
