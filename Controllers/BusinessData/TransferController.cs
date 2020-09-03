using health.common;
using health.Controllers;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.Controllers.BusinessData
{
    [Route("api")]
    public class TransferController : BaseTransactionController
    {
        TransferRepository _repository;
        public TransferController(
            TransferRepository repository
            , IServiceProvider serviceProvider)
            : base(repository, serviceProvider)
        {
            _repository = repository;
        }

        [HttpGet("GetTransferList")]
        public JObject GetTransferList()
        {
            return base.GetList(int.MaxValue, 0);
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
            return base.GetListByPerson(personid);
        }

        [HttpPost("DoTransfer")]
        public JObject DoTransfer(
            [FromServices] AppointRepository appoint,
            [FromServices] CheckRepository check,
            [FromServices] FollowupRepository followup,
            [FromServices] TreatRepository treat,
            [FromServices] VaccRepository vacc,
            [FromQuery] int patientid
            , [FromQuery] int desorgid
            , [FromQuery] string remarks)
        {
            // 防止重复转诊
            JArray array = _repo.GetListByPersonJointImp(patientid, int.MaxValue, 0);
            JToken active = array.Where(t => !t.Value<bool>("iscancel") && !t.Value<bool>("isfinish")).FirstOrDefault();
            if (active!=null)
                return Response_201_write.GetResult(null,"已经发起过转诊："+active.Value<string>("id"));

            // 锁定个人信息
            var username = StampUtil.Stamp(HttpContext);
            appoint.LockPersonData(patientid, username);
            check.LockPersonData(patientid, username);
            followup.LockPersonData(patientid, username);
            treat.LockPersonData(patientid, username);
            vacc.LockPersonData(patientid, username);

            // 开始转诊
            JObject res = new JObject();
            res["id"] = _repository.BeginTransfer(HttpContext,patientid,desorgid,remarks);
            return Response_200_write.GetResult(res);
        }

        [HttpPost("CancelTransfer")]
        public JObject CancelTransfer([FromQuery] int transferId, [FromQuery] string remarks)
        {
            // 防止重复取消
            JObject active = _repo.GetOneRawImp(transferId);
            if (active == null || active.Value<bool>("iscancel") || active.Value<bool>("isfinish"))
                return Response_201_write.GetResult(null, "转诊已取消或已完成");

            JObject res = new JObject();
            if (_repository.CancelTransfer(HttpContext, transferId, remarks) > 0)
            {
                res["id"] = transferId;
                return Response_200_write.GetResult(res);
            }
            else
                return Response_201_write.GetResult();
        }

        [HttpPost("AcceptTransfer")]
        public JObject AcceptTransfer([FromServices] PersonRepository personRepository ,[FromQuery] int transferId, [FromQuery] string remarks)
        {
            // 防止重复接收
            JObject active = _repo.GetOneRawImp(transferId);
            if (active == null || active.Value<bool>("iscancel") || active.Value<bool>("isfinish"))
                return Response_201_write.GetResult(null, "转诊已取消或已完成");

            JObject res = new JObject();
            if (_repository.AcceptTransfer(personRepository, HttpContext, transferId, remarks) > 0)
            {
                res["id"] = transferId;
                return Response_200_write.GetResult(res);
            }
            else
                return Response_201_write.GetResult();
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

        [NonAction]
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


        [NonAction]
        public override JObject GetAltInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
