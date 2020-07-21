/*
 * Title : “药品”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“药品”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class MedicationController : ControllerBase
    {
        private readonly ILogger<MedicationController> _logger;
        dbfactory db = new dbfactory();
        public MedicationController(ILogger<MedicationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“药品”列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>JSON对象，包含相应的“药品”数组</returns>
        [HttpGet]
        [Route("GetMedicationList")]
        public JObject GetMedicationList(int pageSize,int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

            JObject res = new JObject();
            JArray rows = db.GetArray(@"
SELECT   
IFNULL(ID,'') AS ID
,IFNULL(`Name`,'') AS `Name`
,IFNULL(CommonName,'') AS CommonName
,IFNULL(Specification,'') AS Specification
,IFNULL(ESC,'') AS ESC
,IFNULL(ProductionDate,'') AS ProductionDate
,IFNULL(ExpiryDate,'') AS ExpiryDate
,IFNULL(Manufacturer,'') AS Manufacturer
FROM t_medication
LIMIT ?p1,?p2
", offset, pageSize);
            res["list"] = rows;
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“药品”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“药品”信息</returns>
        [HttpGet]
        [Route("GetMedication")]
        public JObject GetMedication(int id)
        {
            JObject res = db.GetOne(@"SELECT   
IFNULL(ID,'') AS ID
,IFNULL(`Name`,'') AS `Name`
,IFNULL(CommonName,'') AS CommonName
,IFNULL(Specification,'') AS Specification
,IFNULL(ESC,'') AS ESC
,IFNULL(ProductionDate,'') AS ProductionDate
,IFNULL(ExpiryDate,'') AS ExpiryDate
,IFNULL(Manufacturer,'') AS Manufacturer
FROM t_medication
WHERE ID=?p1",id);
            if (res["id"]!=null)
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
        /// 更改“药品”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“药品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetMedication")]
        public JObject SetMedication([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Name"] = req["name"]?.ToObject<string>();
            dict["CommonName"] = req["commonname"]?.ToObject<string>();
            dict["Specification"] = req["specification"]?.ToObject<string>();
            dict["ESC"] = req["esc"]?.ToObject<string>();
            dict["ProductionDate"] = req["productiondate"]?.ToObject<string>();
            dict["ExpiryDate"] = req["expirydate"]?.ToObject<string>();
            dict["Manufacturer"] = req["manufacturer"]?.ToObject<string>();


            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_medication", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("t_medication", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }




        /// <summary>
        /// 删除“药品”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“药品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelMedication")]
        public JObject DelMedication([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            var count = db.del("t_medication", dict);
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
        public JObject GetMedicationInfo(int id)
        {
            JObject res = db.GetOne("SELECT id,Name text,ESC code FROM t_medication where id=?p1", id);
            return res;
        }
    }
}