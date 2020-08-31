/*
 * Title : “机构”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“机构”信息的增删查改
 * Comments
 * - 多行，SQL字符串可以用@符号来写，这样可以有效减少+号的拼接。 @norway 2020-07-14 09:56
 * - Org需要 ParentID以及 上级机构的名字 ParentName           @xuedi  2020-07-16 15:59
 * - Post Org不提交ProvinceAddr,CityAddr,CountyAddr          @xuedi,norway 2020-07-17  09:10
 * - 个人档案中读取组织机构时，需要读取id,text(OrgName),code(OrgCode),register(CertCode) @xuedi  2020-07-20  09:20
 * 
 * */
using health.web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;
using health.web.StdResponse;

namespace health.Controllers
{
    [Route("api")]
    public class OrganizationController : AbstractBLLControllerT
    {
        private readonly ILogger<OrganizationController> _logger;
        AreaController _area;
        OrgnizationRepository _orgnizationRepository;

        public OrganizationController(
            OrgnizationRepository repository
            ,IServiceProvider serviceProvider
            )
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<OrganizationController>>();
            _area = serviceProvider.GetService<AreaController>();
            _orgnizationRepository = repository;
        }

        /// <summary>
        /// 获取“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgListD")]
        public override JObject GetList()
        {
            return GetOrgList(10,0);
        }

        /// <summary>
        /// 获取“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgList")]
        public JObject GetOrgList(int pageSize=Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(pageSize,pageIndex);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取指定区县级行政地址下的“机构”列表
        /// </summary>
        /// <returns>JSON数组形式的“机构”信息</returns>
        [HttpGet]
        [Route("GetOrgListv2")]
        public JObject GetOrgList(int provinceid = 0, int cityid = 0, int countyid = 0)
        {
            JObject res = new JObject();
            res["list"] = _orgnizationRepository.GetListJointImp(provinceid,cityid,countyid);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“机构”信息
        /// </summary>
        /// <returns>JSON形式的某个“机构”信息</returns>
        [HttpGet]
        [Route("GetOrg")]
        public override JObject Get(int id)
        {
            var res = base.Get(id);

            if (res["id"] == null)
                return Response_201_read.GetResult(res);
            else
            {
                res["province"] = _area.GetAreaInfo(res.ToInt("provinceid"));
                res["city"] = _area.GetAreaInfo(res.ToInt("cityid"));
                res["county"] = _area.GetAreaInfo(res.ToInt("countyid"));
                res["parent"] = this.GetOrgInfo(res.ToInt("parentid"));
            }

            return res;
        }

        /// <summary>
        /// 更改“机构”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“机构”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetOrg")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }

        /// <summary>
        /// 删除“机构”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“机构”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelOrg")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetOrgInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
      
    }
}
