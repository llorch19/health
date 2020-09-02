/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-20
 * Description: 对“接种记录”信息的增删查改
 * Comments
 * - 需要民族控制器，支持增删查改。    @xuedi  2020-07-20 16:55
 */

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
    public class NationController : BaseNonPagedController
    {

        private readonly ILogger<NationController> _logger;

        public NationController(NationRepository repository
            ,IServiceProvider serviceProvider
            ):base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<NationController>)) as ILogger<NationController>;
        }

        /// <summary>
        /// 获取“民族”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“民族”数组</returns>
        [HttpGet]
        [Route("GetNationList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“民族”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“民族”信息</returns>
        [HttpGet]
        [Route("GetNation")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res["id"] != null)
                return Response_200_read.GetResult(res);
            else
                return Response_201_read.GetResult();
        }


        /// <summary>
        /// 修改“民族”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“民族”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetNation")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“民族”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“民族”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelNation")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetNationInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
