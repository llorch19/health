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
    [Route("api")]
    public class DomiTypeController : AbstractBLLController
    {

        private readonly ILogger<DomiTypeController> _logger;
        public override string TableName => "data_domitype";

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
        public override JObject GetList()
        {
            JObject res = new JObject();
            JArray rows = db.GetArray("select ID,Name,IsActive from data_domitype WHERE IsActive=1 AND IsDeleted=0");

            if (rows.HasValues)
            {
                res["status"] = 200;
                res["msg"] = "读取成功";
                res["list"] = rows;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
            }
           
            return res;
        }

        /// <summary>
        /// 获取“户籍类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“户籍类型”信息</returns>
        [HttpGet]
        [Route("GetDomiType")]
        public override JObject Get(int id)
        {
            JObject res = db.GetOne("select ID,Name,IsActive from data_domitype where id=?p1 and isdeleted=0", id);
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
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“户籍类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“户籍类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelDomiType")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetDomiTypeInfo(int? id)
        {
            JObject res = db.GetOne(@"
select id,Name text from data_domitype where id=?p1 and isdeleted=0", id);
            return res;
        }


        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = req["id"]?.ToObject<string>();
            dict["Name"] = req["name"]?.ToObject<string>();
            return dict;
        }
    }
}
