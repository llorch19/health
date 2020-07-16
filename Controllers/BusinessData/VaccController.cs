/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“接种记录”信息的增删查改
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
    public class VaccController : ControllerBase
    {
        private readonly ILogger<VaccController> _logger;
        public VaccController(ILogger<VaccController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“接种记录”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("GetOrgVaccList")]
        public JObject GetOrgVaccList(int orgid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取个人的“接种记录”历史
        /// </summary>
        /// <param name="userid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("GetPersonVaccList")]
        public JObject GetPersonVaccList(int userid)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 获取“接种记录”信息，点击[科普公告]中的一个项目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”信息</returns>
        [HttpGet]
        [Route("GetVacc")]
        public JObject GetVacc(int id)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 更改“接种记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetVacc")]
        public JObject SetVacc([FromBody] JObject req)
        {
            throw new NotImplementedException();
        }




        /// <summary>
        /// 删除“接种记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelVacc")]
        public JObject DelVacc([FromBody] JObject req)
        {
            throw new NotImplementedException();
        }
    }
}