using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.BaseData
{
    [ApiController]
    public class AddressCategoryController : ControllerBase
    {
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

            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select ID,Code,AddressCategory from data_addresscategory");

            res["list"] = rows;
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
            dbfactory db = new dbfactory();
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
            JObject res = new JObject();
            dbfactory db = new dbfactory();
            if (req["id"] == null)
            {
                res["status"] = 201;
                res["msg"] = "无法添加或修改数据";
                return res;
            }

            if (req["id"].ToObject<int>() == 0)
            {
                var dict = req.ToObject<Dictionary<string, object>>();
                dict.Remove("id");
                var newID = db.Insert("data_addresscategory", dict);
                if (newID > 0)
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
            else
            {
                var dict = req.ToObject<Dictionary<string, object>>();
                dict.Remove("id");
                var keys = new Dictionary<string, object>();
                keys["id"] = req["id"];
                var rows = db.Update("data_addresscategory", dict, keys);
                if (rows > 0)
                {
                    res["status"] = 200;
                    res["msg"] = "修改成功";
                    return res;
                }
                else
                {
                    res["status"] = 201;
                    res["msg"] = "修改失败";
                    return res;
                }
            }
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
    }
}
