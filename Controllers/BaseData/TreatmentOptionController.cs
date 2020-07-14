using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using util;
using util.mysql;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace health.Controllers
{
    [ApiController]
    public class TreatmentOptionController : ControllerBase
    {

        private readonly ILogger<TreatmentOptionController> _logger;

        public TreatmentOptionController(ILogger<TreatmentOptionController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“治疗方案”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“治疗方案”数组</returns>
        [HttpGet]
        [Route("GetTreatmentOptionList")]
        public JObject GetTreatmentOptionList(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Name,Introduction from data_treatmentoption");

            res["list"] = rows;
            return res;
        }

        /// <summary>
        /// 获取“治疗方案”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“治疗方案”信息</returns>
        [HttpGet]
        [Route("GetTreatmentOption")]
        public JObject GetTreatmentOption(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select ID,Name,Introduction from data_treatmentoption where id=?p1", id);
            if (res["id"] != null)
                res["status"] = 200;
            else
            {
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }
            return res;
        }

        /// <summary>
        /// 修改“治疗方案”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“治疗方案”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetTreatmentOption")]
        public JObject SetTreatmentOption([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("data_treatmentoption", dict);
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
                    var rows = db.Update("data_treatmentoption", dict, keys);
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
        /// 删除“治疗方案”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“治疗方案”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelTreatmentOption")]
        public JObject DelTreatmentOption([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_treatmentoption", dict);
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
