/*
 * Title : “菜单”控制器
 * Author: zudan
 * Date  : 2020-07-13
 * Description: 获取菜单，需要在中间件判断用户组并加以过滤
 * Comments
 * - Area 应该返回增加时需要录入的全部字段。包括但不限于[AreaCodeV2]。    @xuedi    2020-07-16  08:17
 * - 新增AreaList接口返回地域的树形结构。     @xuedi  2020-07-20  10:25
 * */

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
    public class AreaController : BaseNonPagedController
    {
        ILogger<AreaController> _logger;
        AreaRepository _repository;
        public AreaController(AreaRepository repository,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<AreaController>)) as ILogger<AreaController>;
            _repository = repository;
        }

        /// <summary>
        /// 获取“区域”列表
        /// </summary>
        /// <param name="parentId">指定parentId</param>
        /// <returns>JSON对象，包含指定parentId的下属“区域”数组</returns>
        [HttpGet]
        [Route("GetAreaListH")]
        public JObject GetAreaList(int parentId)
        {
            JObject res = new JObject();
            res["list"] = _repository.GetListJointImp(parentId, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取“区域”列表
        /// </summary>
        /// <returns>JSON对象，包含指定parentId的下属“区域”数组</returns>
        [HttpGet]
        [Route("GetAreaList")]
        public override JObject GetList()
        {
            return base.GetList();
        }


        /// <summary>
        /// 获取层级的“区域”列表
        /// </summary>
        /// <returns>JSON对象，包含层级的区域列表</returns>
        [HttpGet]
        [Route("AreaList")]
        public JObject GetBaseData()
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            res["status"] = 200;
            common.BaseConfig _area = new common.BaseConfig();
            res["AreaList"] = _area.GetAreaTree();
            res["UserGroup"] = db.GetArray("SELECT id,cname text FROM t_user_group where isActive = 1");
            return res;
        }

        /// <summary>
        /// 获取“区域”信息
        /// </summary>
        /// <returns>获取指定id的“区域”信息</returns>
        [HttpGet]
        [Route("GetArea")]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        /// <summary>
        /// 修改“区域”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“区域”信息</param>
        /// <returns>响应状态信息</returns>
        //[HttpPost]
        //[Route("SetArea")]
        [NonAction]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“区域”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“区域”信息</param>
        /// <returns>响应状态信息</returns>
        //[HttpPost]
        //[Route("DelArea")]
        [NonAction]
        public override JObject Del([FromBody] JObject req)
        {
            return Response_201_write.GetResult();
        }


        [NonAction]
        public JObject GetAreaInfo(int? id)
        {
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,AreaName text from data_area where id=?p1", id);
            return res;
        }
    }
}
