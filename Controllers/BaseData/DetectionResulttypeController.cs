using health.web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class DetectionResultTypeController : AbstractBLLControllerT
    {
        private readonly ILogger<DetectionResultTypeController> _logger;

        public DetectionResultTypeController(DetectionResultTypeRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<DetectionResultTypeController>)) as ILogger<DetectionResultTypeController>;
        }

        /// <summary>
        /// 获取“检测结果”列表
        /// </summary>
        /// <returns>JSON对象，包含“检测结果”的数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            return base.GetList();
        }

        /// <summary>
        /// 获取“检测结果”信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }



        /// <summary>
        /// 修改“检测结果”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“检测结果”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“检测结果”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“检测结果”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelDetectionResultType")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetResultTypeInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
