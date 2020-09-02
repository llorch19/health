using health.Controllers;
using health.web.StdResponse;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.Controllers
{
    public abstract class BaseNonPagedController : BaseController
    {
        protected BaseNonPagedController(IRepository repository, IServiceProvider serviceProvider) : base(repository, serviceProvider)
        {
        }

        public override JObject Del(JObject req)
        {
            return base.Del(req);
        }

        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        public override JObject GetAltInfo(int? id)
        {
            return base.GetAltInfo(id);
        }

        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        public override JObject Set(JObject req)
        {
            return base.Set(req);
        }
    }
}
