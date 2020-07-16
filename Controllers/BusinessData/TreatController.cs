/*
 * Title : “用药记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“用药记录”信息的增删查改
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
    public class TreatController : ControllerBase
    {
        private readonly ILogger<TreatController> _logger;
        public TreatController(ILogger<TreatController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“用药记录”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetOrgTreatList")]
        public JObject GetOrgTreatList(int orgid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取个人的“用药记录”历史
        /// </summary>
        /// <param name="userid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetPersonTreatList")]
        public JObject GetPersonTreatList(int userid)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 获取“用药记录”信息，点击[科普公告]中的一个项目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”信息</returns>
        [HttpGet]
        [Route("GetTreat")]
        public JObject GetTreat(int id)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 更改“用药记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetTreat")]
        public JObject SetTreat([FromBody] JObject req)
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// 删除“用药记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelTreat")]
        public JObject DelTreat([FromBody] JObject req)
        {
            throw new NotImplementedException();
        }
    }
}