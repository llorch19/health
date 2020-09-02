/*
 * Title : “就诊”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“就诊”信息的增删查改
 * Comments
 */
using health.common;
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
    public class AttandentController : BaseController
    {
        private readonly ILogger<AttandentController> _logger;
        OrganizationController _org;
        PersonController _person;

        public AttandentController(AttandentRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<AttandentController>)) as ILogger<AttandentController>;
            _org = serviceProvider.GetService(typeof(OrganizationController)) as OrganizationController;
            _person = serviceProvider.GetService(typeof(PersonController)) as PersonController;
        }

        /// <summary>
        /// 获取机构的“就诊”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetAttandentList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            res["list"] = _repo.GetListByOrgJointImp(orgid??0, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取个人的“就诊”列表
        /// </summary>
        /// <param name="personid">请求的个人id</param>
        /// <returns>JSON对象，包含相应的“就诊”数组</returns>
        [HttpGet]
        [Route("GetAttandentListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JArray GetListPImp(int personid)
        {
            return _repo.GetListByPersonJointImp(personid, Const.defaultPageSize, Const.defaultPageIndex);
        }

        /// <summary>
        /// 获取“就诊”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“就诊”信息</returns>
        [HttpGet]
        [Route("GetAttandent")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            var canread = res.Challenge(r=>r["id"]!=null);
            if (!canread)
                return Response_201_read.GetResult();

            res["person"] = _person.GetPersonInfo(res["personid"].ToObject<int>());
            res["orgnization"] = _org.GetOrgInfo(res["orgnizationid"].ToObject<int>());
            res["srcorg"] = _org.GetOrgInfo(res["srcorgid"].ToObject<int>());
            res["desorg"] = _org.GetOrgInfo(res["desorgid"].ToObject<int>());

            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“就诊”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetAttandent")]
        public override JObject Set([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            // 如果存在未转诊及出院信息，不允许新增就诊记录
            JObject attand = _repo.GetOneRawImp(id ?? 0);
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
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
        /// 删除“就诊”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“就诊”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelAttandent")]
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