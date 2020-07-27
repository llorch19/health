/*
 * Title : “检测产品”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测产品”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class CheckProductController : AbstractBLLController
    {
        private readonly ILogger<CheckProductController> _logger;
        public override string TableName => "t_detectionproduct";

        public CheckProductController(ILogger<CheckProductController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("GetCheckProductListDefault")]
        public override JObject GetList()
        {
            return GetCheckProductList(10, 0);
        }

        /// <summary>
        /// 获取机构的“检测产品”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“检测产品”数组</returns>
        [HttpGet]
        [Route("GetCheckProductList")]
        public JObject GetCheckProductList(int pageSize,int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

            JObject res = new JObject();
            JArray rows = db.GetArray(@"
SELECT 
ID
,`Name`
,ShortName
,CommonName
,Specification
,BatchNumber
,Manufacturer
,ESC
,ProductionDate
,ExpiryDate
FROM t_detectionproduct
LIMIT ?p1,?p2
", offset, pageSize);
            res["list"] = rows;
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }




        /// <summary>
        /// 获取“检测产品”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测产品”信息</returns>
        [HttpGet]
        [Route("GetCheckProduct")]
        public override JObject Get(int id)
        {
            JObject res = db.GetOne(@"SELECT 
ID
,`Name`
,ShortName
,CommonName
,Specification
,BatchNumber
,Manufacturer
,ESC
,ProductionDate
,ExpiryDate
FROM t_detectionproduct
WHERE ID=?p1", id);
            if (res["id"] != null)
            {
                res["status"] = 200;
                res["msg"] = "读取成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "无法读取相应的数据";
            }
            return res;
        }


        /// <summary>
        /// 更改“检测产品”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测产品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetCheckProduct")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }




        /// <summary>
        /// 删除“检测产品”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测产品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelCheckProduct")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }



        [NonAction]
        public JObject GetCheckProductInfo(int? id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,Name text from t_detectionproduct where id=?p1", id);
            return res;
        }

        

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Name"] = req["name"]?.ToObject<string>();
            dict["ShortName"] = req["shortname"]?.ToObject<string>();
            dict["BatchNumber"] = req["batchnumber"]?.ToObject<string>();
            dict["CommonName"] = req["commonname"]?.ToObject<string>();
            dict["Specification"] = req["specification"]?.ToObject<string>();
            dict["ESC"] = req["esc"]?.ToObject<string>();
            dict["ProductionDate"] = req["productiondate"]?.ToObject<string>();
            dict["ExpiryDate"] = req["expirydate"]?.ToObject<string>();
            dict["Manufacturer"] = req["manufacturer"]?.ToObject<string>();

            return dict;
        }
    }
}