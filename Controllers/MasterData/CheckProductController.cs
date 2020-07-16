/*
 * Title : “检测产品”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测产品”信息的增删查改
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
    public class CheckProductController : ControllerBase
    {
        private readonly ILogger<CheckProductController> _logger;
        public CheckProductController(ILogger<CheckProductController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“检测产品”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“检测产品”数组</returns>
        [HttpGet]
        [Route("GetCheckProductList")]
        public JObject GetCheckProductList(int orgid)
        {
            throw new NotImplementedException();
        }

       


        /// <summary>
        /// 获取“检测产品”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测产品”信息</returns>
        [HttpGet]
        [Route("GetCheckProduct")]
        public JObject GetCheckProduct(int id)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 更改“检测产品”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测产品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetCheckProduct")]
        public JObject SetCheckProduct([FromBody] JObject req)
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// 删除“检测产品”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测产品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelCheckProduct")]
        public JObject DelCheckProduct([FromBody] JObject req)
        {
            throw new NotImplementedException();
        }
    }
}