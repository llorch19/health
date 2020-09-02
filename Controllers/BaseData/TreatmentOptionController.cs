using health.web.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class TreatmentOptionController : BaseNonPagedController
    {

        private readonly ILogger<TreatmentOptionController> _logger;

        public TreatmentOptionController(
            TreatmentOptionRepository repository
            , IServiceProvider serviceProvider
            ) : base(repository, serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<TreatmentOptionController>)) as ILogger<TreatmentOptionController>;
        }

        /// <summary>
        /// 获取“治疗方案”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“治疗方案”数组</returns>
        [HttpGet]
        [Route("GetTreatmentOptionList")]
        public override JObject GetList()
        {
            return base.GetList();
        }

        /// <summary>
        /// 获取“治疗方案”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“治疗方案”信息</returns>
        [HttpGet]
        [Route("GetTreatmentOption")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        /// <summary>
        /// 修改“治疗方案”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“治疗方案”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetTreatmentOption")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“治疗方案”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“治疗方案”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelTreatmentOption")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetTreatOptionInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
