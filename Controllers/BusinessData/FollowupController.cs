/*
 * Title : “随访”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“随访”信息的增删查改
 * Comments
 */
using health.common;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class FollowupController : AbstractBLLControllerT
    {
        private readonly ILogger<FollowupController> _logger;
        PersonController _person;
        OrganizationController _org;

        public FollowupController(
            FollowupRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<FollowupController>>();
            _person = serviceProvider.GetService<PersonController>(); 
            _org = serviceProvider.GetService<OrganizationController>(); 
        }

        /// <summary>
        /// 获取机构的“随访”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“随访”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取机构的“随访”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“随访”数组</returns>
        [HttpGet]
        [Route("Get[controller]ListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取“随访”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“随访”信息</returns>
        [HttpGet]
        [Route("GetFollowup")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res["id"] == null)
                return Response_201_read.GetResult();

            res["person"] = _person.GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["orgnization"] = _org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“随访”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“随访”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetFollowup")]
        public override JObject Set([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var id = req.ToInt("id");
            if (id == 0) // 新增
                req["orgnizationid"] = orgid;
            else
            {
                var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
                if (!canwrite)
                    return Response_201_write.GetResult();
            }

            return base.Set(req);
        }




        /// <summary>
        /// 删除“随访”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“随访”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelFollowup")]
        public override JObject Del([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetAltInfo(base.Get(id ?? 0).ToInt("orgnizationid"));
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Del(req);
        }
    }
}