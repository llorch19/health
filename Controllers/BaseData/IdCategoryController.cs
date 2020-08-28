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
    public class IdCategoryController : AbstractBLLControllerT
    {

        private readonly ILogger<IdCategoryController> _logger;

        public IdCategoryController(IdCategoryRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<IdCategoryController>)) as ILogger<IdCategoryController>;
        }

        /// <summary>
        /// 获取“身份证件类型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“身份证件类型”数组</returns>
        [HttpGet]
        [Route("GetIdCategoryList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = base.GetList();
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“身份证件类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“身份证件类型”信息</returns>
        [HttpGet]
        [Route("GetIdCategory")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res.HasValues)
                return Response_200_read.GetResult(res);
            else
                return Response_201_read.GetResult(res);
        }

        /// <summary>
        /// 修改“身份证件类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“身份证件类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetIdCategory")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }



        /// <summary>
        /// 删除“身份证件类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“身份证件类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelIdCategory")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetIdCategoryInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
