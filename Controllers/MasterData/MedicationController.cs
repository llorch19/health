/*
 * Title : “药品”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“药品”信息的增删查改
 * Comments
 * - 开会时说过，Medication总数不多，需要手选药品    @xuedi  2020-07-22
 */
using health.web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;
using health.web.StdResponse;
using health.web.Controllers;

namespace health.Controllers
{
    [Route("api")]
    public class MedicationController : BasePagedController
    {
        private readonly ILogger<MedicationController> _logger;

        public MedicationController(
            MedicationRepository repository
            ,IServiceProvider serviceProvider
            )
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<MedicationController>>();
        }

        /// <summary>
        /// 获取机构的“药品”列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>JSON对象，包含相应的“药品”数组</returns>
        [HttpGet]
        [Route("GetMedicationList")]
        public override JObject GetList(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            return base.GetList(pageSize,pageIndex);
        }


        /// <summary>
        /// 获取“药品”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“药品”信息</returns>
        [HttpGet]
        [Route("GetMedication")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }


        /// <summary>
        /// 更改“药品”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“药品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetMedication")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }




        /// <summary>
        /// 删除“药品”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“药品”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelMedication")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetMedicationInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}