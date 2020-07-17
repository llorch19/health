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
    public class MedicationController : ControllerBase
    {
        private readonly ILogger<MedicationController> _logger;
        public MedicationController(ILogger<MedicationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“药品”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“药品”数组</returns>
        [HttpGet]
        [Route("GetMedicationList")]
        public JObject GetMedicationList(int orgid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取个人的“药品”历史
        /// </summary>
        /// <param name="userid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“药品”数组</returns>
        [HttpGet]
        [Route("GetPersonMedicationList")]
        public JObject GetPersonMedicationList(int userid)
        {
            throw new NotImplementedException();
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
            dbfactory db = new dbfactory();
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
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    req.Remove("publish");
                    req["OrgnizationID"] = null;
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_vacc", dict);
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
                    var rows = db.Update("t_medication", dict, keys);
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
            dbfactory db = new dbfactory();
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
    }
}