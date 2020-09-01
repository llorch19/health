/*
 * Title : “用药记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“用药记录”信息的增删查改
 * Comments
 * - - GetOrgTreatList 应该和GetPeron["treat"]字段一致     @xuedi      2020-07-22 
 * - 新增“治疗用药记录”     @xuedi      2020-07-22      16:30
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
using System.Collections.Immutable;
using System.Linq;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class TreatController : AbstractBLLControllerT
    {
        private readonly ILogger<TreatController> _logger;
        TreatItemRepository _items;
        OrgnizationRepository _org;
        PersonRepository _person;

        public TreatController(
            TreatRepository repository,
            IServiceProvider serviceProvider)
            :base(repository, serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<TreatController>>();
            _items = serviceProvider.GetService<TreatItemRepository>();
            _org = serviceProvider.GetService<OrgnizationRepository>();
            _person = serviceProvider.GetService<PersonRepository>();
        }

        /// <summary>
        /// 获取机构的“治疗记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetTreatList2")]
        public JObject GetTreatList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取机构的“治疗用药记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetTreatList")]
        public override JObject GetList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取个人的“治疗用药记录”列表
        /// </summary>
        /// <param name="personid">个人id</param>
        /// <returns>JSON对象，包含相应的“用药记录”数组</returns>
        [HttpGet]
        [Route("GetTreatListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“治疗记录”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“用药记录”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);

            var canread = res.Challenge(r=>r["id"]!=null);
            if (!canread)
                return Response_201_read.GetResult();

            res["orgnization"] = _org.GetAltInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["person"] = _person.GetAltInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["items"] = _items.ListItemJointByTreat(res["id"].ToObject<int>());
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“治疗记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
        {
            throw new NotFiniteNumberException();
        }




        /// <summary>
        /// 删除“治疗记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“用药记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetAltInfo(id);
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();


            return base.Del(req);
        }
    }
}