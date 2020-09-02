using health.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.BaseData
{
    [Route("api")]
    public class AddressCategoryController : BaseController
    {
        public AddressCategoryController(AddressCategoryRepository addressCategoryRepository,IServiceProvider serviceProvider) 
            : base(addressCategoryRepository,serviceProvider) 
        { 

        }


        /// <summary>
        /// 获取“地址类型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“地址类型”数组</returns>
        [HttpGet("GetAddressCategoryList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue,0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“地址类型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“地址类型”信息</returns>
        [HttpGet("GetAddressCategory")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        /// <summary>
        /// 修改“地址类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“地址类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetAddressCategory")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“地址类型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“地址类型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelAddressCategory")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetAddressCategoryInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
