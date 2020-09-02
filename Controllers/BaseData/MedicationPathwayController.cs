using health.web.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class MedicationPathwayController : BaseNonPagedController
    {

        private readonly ILogger<MedicationPathwayController> _logger;

        public MedicationPathwayController(MedicationPathwayRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<MedicationPathwayController>)) as ILogger<MedicationPathwayController>;
        }

        /// <summary>
        /// 获取“用药途径”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“用药途径”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            return base.GetList();
        }

        /// <summary>
        /// 获取“用药途径”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“用药途径”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        /// <summary>
        /// 修改“用药途径”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“用药途径”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“用药途径”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“用药途径”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetPathwayInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
