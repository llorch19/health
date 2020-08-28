using health.common;
using health.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.Controllers.BusinessData
{
    [Route("api")]
    public class TransferController : AbstractBLLControllerT
    {
        TransferRepository _repository;
        public TransferController(TransferRepository repository
            , IServiceProvider serviceProvider)
            : base(repository, serviceProvider)
        {
            _repository = repository;
        }

        [HttpGet("GetTransferList")]
        public JObject GetTransferList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repository.GetListByOrgJointImp(orgid ?? 0, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }

        [HttpGet("GetTransferUnhandledList")]
        public JObject GetTransferUnhandledList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repository.GetListByRecvOrgJointImp(orgid ?? 0, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }

        [HttpGet("GetTransferListForPerson")]
        public JObject GetTransferListForPerson(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repository.GetListByPersonJointImp(personid, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }

        [HttpPost("DoTransfer")]
        public JObject DoTransfer(JObject data)
        {
            throw new NotImplementedException();
            JObject res = new JObject();
            return Response_200_read.GetResult(res);
        }

        [HttpPost("CancelTransfer")]
        public JObject CancelTransfer()
        {
            throw new NotImplementedException();
        }

        [HttpPost("AcceptTransfer")]
        public JObject AcceptTransfer()
        {
            throw new NotImplementedException();
        }

        [NonAction]
        public override JObject GetList()
        {
            return base.GetList();
        }

        [NonAction]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }

        [NonAction]
        public override JObject Set(JObject req)
        {
            return base.Set(req);
        }

        [NonAction]
        public override JObject Del(JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public override JObject GetAltInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
