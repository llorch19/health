/*
 * Title : “预约”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“预约”信息的增删查改
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
    public class AppointController : AbstractBLLControllerT
    {
        private readonly ILogger<AppointController> _logger;
        OrganizationController _org;
        PersonController _person;

        public AppointController(
            AppointRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<AppointController>>();
            _org = serviceProvider.GetService<OrganizationController>();
            _person = serviceProvider.GetService<PersonController>(); ;
        }

        /// <summary>
        /// 获取机构的“预约”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetAppointList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取个人的“预约”列表
        /// </summary>
        /// <param name="personid">请求的个人id</param>
        /// <returns>JSON对象，包含相应的“预约”数组</returns>
        [HttpGet]
        [Route("GetAppointListP")]
        public JObject GetAppointListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“预约”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“预约”信息</returns>
        [HttpGet]
        [Route("GetAppoint")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            var canread = res.Challenge(r=>r["id"]!=null);
            if (!canread)
                return Response_201_read.GetResult(res);

            res["orgnization"] = _org.GetOrgInfo(res.ToInt("orgnizationid"));
            res["person"] = _person.GetPersonInfo(res.ToInt("patientid"));
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“预约”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAppoint")]
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
        /// 删除“预约”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“预约”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAppoint")]
        public override JObject Del([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetOrgInfo(id);
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();
            return base.Del(req);
        }
       
    }
}