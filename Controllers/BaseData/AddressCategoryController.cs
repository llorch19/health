using health.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.BaseData
{
    [ApiController]
    [Route("api")]
    public class AddressCategoryController : AbstractBLLController
    {
        private readonly ILogger<AddressCategoryController> _logger;
        public override string TableName => "data_addresscategory";

        public AddressCategoryController(ILogger<AddressCategoryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“地址类型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“地址类型”数组</returns>
        [HttpGet("GetAddressCategoryList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";
            res["list"] = db.GetArray("select ID,Code,AddressCategory from data_addresscategory WHERE IsActive=1 AND IsDeleted=0");
            return res;
        }

        /// <summary>
        /// 获取“地址类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“地址类型”信息</returns>
        [HttpGet("GetAddressCategory")]
        public override JObject Get(int id)
        {
            JObject res = db.GetOne("select ID,Code,AddressCategory from data_addresscategory where id=?p1 AND IsActive=1 AND IsDeleted=0", id);
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
        /// 修改“地址类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“地址类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetAddressCategory")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“地址类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“地址类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelAddressCategory")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetAddressCategoryInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,AddressCategory text from data_addresscategory where id=?p1", id);
            return res;
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["AddressCategory"] = req["addresscategory"]?.ToObject<string>();
            return dict;
        }
    }
}
