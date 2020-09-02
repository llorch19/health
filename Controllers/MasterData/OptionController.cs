/*
 * Title : 参数信息控制器
 * Author: zudan
 * Date  : 2020-07-15
 * Description: 对全局或特定于机构的参数信息进行增删查改
 * Comments
 * - 支持其他可变参数   @xuedi  2020-07-15 09:52
 */
using health.web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;
using health.web.StdResponse;

namespace health.Controllers
{
    [Route("api")]
    public class OptionController : BaseController
    {
        OptionRepository optionRepository;
        private readonly ILogger<OptionController> _logger;

        public OptionController(
            OptionRepository repository
            , IServiceProvider serviceProvider
            )
            : base(repository, serviceProvider)
        {
            optionRepository = repository;
            _logger = serviceProvider.GetService<ILogger<OptionController>>();
        }

        /// <summary>
        /// 获取“参数”列表
        /// </summary>
        /// <param name="section">节名</param>
        /// <returns>JSON数组形式的“参数”信息</returns>
        [HttpGet("GetOptionSectionList")]
        public JObject GetList([FromQuery]string section = null)
        {
            JObject res = new JObject();
            res["list"] = optionRepository.GetListJointImp(section, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public override JObject GetList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取“参数”列表
        /// </summary>
        /// <returns>JSON数组形式的“参数”信息</returns>
        [HttpGet("GetOptionList")]
        public JObject GetOptionList()
        {
            JObject res = new JObject();
            res["list"] = optionRepository.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“参数”信息
        /// </summary>
        /// <returns>JSON形式的某个“参数”信息</returns>
        [HttpGet]
        [Route("GetOption")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        /// <summary>
        /// 更改“参数”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“参数”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetOption")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }

        /// <summary>
        /// 删除“参数”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“参数”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelOption")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }
    }
}
