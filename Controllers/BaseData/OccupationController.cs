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
    public class OccupationController : BaseNonPagedController
    {

        private readonly ILogger<OccupationController> _logger;
        public OccupationController(
            OccupationRepository repository
            ,IServiceProvider serviceProvider
            ):base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<OccupationController>)) as ILogger<OccupationController>;
        }

        /// <summary>
        /// 获取“职业”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“职业”数组</returns>
        [HttpGet]
        [Route("GetOccupationList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“职业”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“职业”信息</returns>
        [HttpGet]
        [Route("GetOccupation")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res["id"] != null)
                return Response_200_read.GetResult(res);
            else
                return Response_201_read.GetResult();
        }

        /// <summary>
        /// 修改“职业”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“职业”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetOccupation")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“职业”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“职业”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelOccupation")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetOccupationInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
