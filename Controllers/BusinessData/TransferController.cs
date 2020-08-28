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
    public class TransferController:AbstractBLLControllerT
    {
        TransferRepository _repository;
        public TransferController(TransferRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository, serviceProvider)
        {
            _repository = repository;
        }

        [HttpGet("GetTransferList")]
        public JObject GetTransferList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repository.GetListByOrgJointImp(orgid ?? 0);
            return Response_200_read.GetResult(res);
        }

        [HttpGet("GetTransferUnhandledList")]
        public JObject GetTransferUnhandledList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repository.GetListByRecvOrgJointImp(orgid ?? 0);
            return Response_200_read.GetResult(res);
        }

        public JObject GetTransferListForPerson(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repository.GetListByPersonJointImp(personid);
            return Response_200_read.GetResult(res);
        }

        public JObject DoTransfer(JObject data)
        {
            throw new NotImplementedException();
            JObject res = new JObject();
            return Response_200_read.GetResult(res);
        }

        public JObject CancelTransfer()
        {
            throw new NotImplementedException();
        }


        public JObject AcceptTransfer()
        {
            throw new NotImplementedException();
        }

    }
}
