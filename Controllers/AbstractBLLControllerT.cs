/*
 * Title : 个人信息管理控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * -  GetList 和 Get返回 IsDeleted
 *
 */

using health.common;
using health.web;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.Controllers
{
    public abstract class AbstractBLLControllerT : ControllerBase
    {
        protected IRepository _repo;
        public AbstractBLLControllerT(IRepository repository,IServiceProvider serviceProvider)
        {
            _repo = repository;
        }

        public virtual JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp();
            return Response_200_read.GetResult(res);
        }

        public virtual JObject Get(int id)
        {
            JObject res = _repo.GetOneRawImp(id);
            return Response_200_read.GetResult(res);
        }

        public virtual JObject Set(JObject req)
        {
            JObject res = new JObject();
            res["id"] = _repo.AddOrUpdateRaw(req, StampUtil.Stamp(HttpContext));
            return Response_200_write.GetResult(res);
        }

        public virtual JObject Del(JObject req)
        {
            JObject res = new JObject();
            res["id"] = _repo.DelRaw(req, StampUtil.Stamp(HttpContext));
            return Response_200_write.GetResult(res);
        }
    }
}
