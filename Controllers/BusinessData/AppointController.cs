/*
 * Title : “预约”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“预约”信息的增删查改
 * Comments
 */
using health.common;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;
using health.web.Controllers;

namespace health.Controllers
{
    [Route("api")]
    public class AppointController : AbsTransactionBLLController
    {
        private readonly ILogger<AppointController> _logger;

        public AppointController(
            AppointRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<AppointController>>();
        }

        /// <summary>
        /// 获取机构的“预约”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetAppointList")]
        public override JObject GetList()
        {
            return base.GetList();
        }

        /// <summary>
        /// 获取个人的“预约”列表
        /// </summary>
        /// <param name="personid">请求的个人id</param>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetAppointListP")]
        public JObject GetAppointListP(int personid)
        {
            return base.GetListByPerson(personid);
        }

        /// <summary>
        /// 获取“预约”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“预约”信息</returns>
        [HttpGet]
        [Route("GetAppoint")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }


        /// <summary>
        /// 更改“预约”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAppoint")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }




        /// <summary>
        /// 删除“预约”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAppoint")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }
       
    }
}