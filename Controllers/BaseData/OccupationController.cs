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
    public class OccupationController : AbstractBLLController
    {

        private readonly ILogger<OccupationController> _logger;
        dbfactory db = new dbfactory();

        public override string TableName => "data_occupation";

        public OccupationController(ILogger<OccupationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“职业”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“职业”数组</returns>
        [HttpGet]
        [Route("GetOccupationList")]
        public override JObject GetList()
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,OccupationName,OccupationRemarks from data_occupation where IsActive=1 and IsDeleted=0");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“职业”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“职业”信息</returns>
        [HttpGet]
        [Route("GetOccupation")]
        public override JObject Get(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Code,OccupationName,OccupationRemarks from data_occupation where id=?p1 and IsDeleted=0", id);
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
        /// 修改“职业”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“职业”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetOccupation")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“职业”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“职业”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelOccupation")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetOccupationInfo(int? id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,OccupationName text from data_occupation where id=?p1 and IsDeleted=0", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["OccupationName"] = req["occupationname"]?.ToObject<string>();
            dict["OccupationRemarks"] = req["occupationremarks"]?.ToObject<string>();


            return dict;
        }
    }
}
