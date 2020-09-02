using health.common;
using health.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace health.web.Controllers
{
    /// <summary>
    /// 事务控制器总是包含两个外键： PersonID 及 OrgnizationID
    /// </summary>
    public abstract class BaseTransactionController : BaseController
    {
        protected OrgnizationRepository _org;
        protected PersonRepository _person;
        protected BaseTransactionController(IRepository repository, IServiceProvider serviceProvider) : base(repository, serviceProvider)
        {
            _org = serviceProvider.GetService<OrgnizationRepository>();
            _person = serviceProvider.GetService<PersonRepository>();
        }

        public override JObject Del(JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetAltInfo(base.Get(id ?? 0).ToInt("orgnizationid"));
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Del(req);
        }

        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            var canread = res.Challenge(r => r["id"] != null);
            if (!canread)
                return Response_201_read.GetResult(res);

            res["orgnization"] = _org.GetAltInfo(res.ToInt("orgnizationid"));
            res["person"] = _person.GetAltInfo(res.ToInt("patientid"));
            return Response_200_read.GetResult(res);
        }

        public override JObject GetAltInfo(int? id)
        {
            return base.GetAltInfo(id);
        }

        public override JObject GetList()
        {
            JObject res = new JObject();
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JObject GetList(int pageSize, int pageIndex)
        {
            JObject res = new JObject();
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, pageSize, pageIndex);
            return Response_200_read.GetResult(res);
        }


        [NonAction]
        public JObject GetListByPerson(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JObject GetListByPerson(int personid, int pageSize, int pageIndex)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, pageSize, pageIndex);
            return Response_200_read.GetResult(res);
        }

        public override JObject Set(JObject req)
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
    }
}
