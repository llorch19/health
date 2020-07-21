using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.BaseData
{
    [ApiController]
    [Route("api")]
    public class AddressCategoryController : ControllerBase
    {
        private readonly ILogger<AddressCategoryController> _logger;
        dbfactory db = new dbfactory();
        public AddressCategoryController(ILogger<AddressCategoryController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“地址类型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“地址类型”数组</returns>
        [HttpGet("GetAddressCategoryList")]
        public JObject GetAddressCategoryList()
        {
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";
            res["list"] = db.GetArray("select ID,Code,AddressCategory from data_addresscategory");
            return res;
        }

        /// <summary>
        /// 获取“地址类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“地址类型”信息</returns>
        [HttpGet("GetAddressCategory")]
        public JObject GetAddressCategory(int id)
        {
            JObject res = db.GetOne("select ID,Code,AddressCategory from data_addresscategory where id=?p1", id);
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
        public JObject SetAddressCategory([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = req["code"]?.ToObject<string>();
            dict["AddressCategory"] = req["addresscategory"]?.ToObject<string>();
           

            if (req["id"].ToObject<int>()>0)
            {
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                var tmp=this.db.Update("data_addresscategory",dict,condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("data_addresscategory",dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }


        /// <summary>
        /// 删除“地址类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“地址类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelAddressCategory")]
        public JObject DelAddressCategory([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
            var count = db.del("data_addresscategory", dict);
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
        public JObject GetAddressCategoryInfo(int id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,AddressCategory text from data_addresscategory where id=?p1", id);
            return res;
        }
    }
}
