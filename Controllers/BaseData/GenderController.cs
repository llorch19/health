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
    public class GenderController : BaseNonPagedController
    {
        private readonly ILogger<GenderController> _logger;

        public GenderController(GenderRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<GenderController>)) as ILogger<GenderController>;
        }

        /// <summary>
        /// 获取“性别”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“性别”数组</returns>
        [HttpGet]
        [Route("GetGenderList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“性别”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“性别”信息</returns>
        [HttpGet]
        [Route("GetGender")]
        public override JObject Get(int id)
        {
            var res = base.Get(id);
            if (res["id"] != null)
                return Response_200_read.GetResult(res);
            else
                return Response_201_read.GetResult(res);
        }

        /// <summary>
        /// 修改“性别”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“性别”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetGender")]
        public override JObject Set([FromBody] JObject req)
        {
            if (req["code"]?.ToObject<string>()?.Length > 1)
                return Response_201_write.GetResult(null, "编码长度不大于1");
            return base.Set(req);
        }


        /// <summary>
        /// 删除“性别”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“性别”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelGender")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetGenderInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
