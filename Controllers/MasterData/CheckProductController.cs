/*
 * Title : “检测产品”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测产品”信息的增删查改
 * Comments
 */
using health.web.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Relational;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class CheckProductController : BasePagedController
    {
        private readonly ILogger<CheckProductController> _logger;

        public CheckProductController(
            CheckProductRepository repository
            , IServiceProvider serviceProvider
        )
            : base(repository, serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<CheckProductController>>();
        }

        [NonAction]
        public override JObject GetList()
        {
            return base.GetList();
        }

        /// <summary>
        /// 获取机构的“检测产品”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“检测产品”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            return base.GetList(pageSize,pageIndex);
        }




        /// <summary>
        /// 获取“检测产品”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测产品”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }


        /// <summary>
        /// 更改“检测产品”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测产品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Set[controller]")]
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
        [Route("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }



        [NonAction]
        public JObject GetCheckProductInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}