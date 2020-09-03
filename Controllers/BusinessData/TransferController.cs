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
        TransferRepository _srvTransfer;
        public TransferController(
            TransferRepository repository
            , IServiceProvider serviceProvider)
            : base(repository, serviceProvider)
        {
            _srvTransfer = repository;
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
            res["list"] = _srvTransfer.GetListByRecvOrgJointImp(orgid ?? 0, Const.defaultPageSize, Const.defaultPageIndex);
            return Response_200_read.GetResult(res);
        }

        [HttpGet("GetTransferListForPerson")]
        public JObject GetTransferListForPerson(int personid)
        {
            return base.GetListByPerson(personid);
        }

        [HttpPost("DoTransfer")]
        public JObject DoTransfer(
            [FromServices] AppointRepository srvAppoint,
            [FromServices] CheckRepository srvCheck,
            [FromServices] FollowupRepository srvFollowup,
            [FromServices] TreatRepository srvTreat,
            [FromServices] VaccRepository srvVacc,
            [FromServices] PersonRepository srvPerson,
            [FromServices] OrgnizationRepository srvOrg,
            [FromQuery] int patientid
            , [FromQuery] int desorgid
            , [FromQuery] string remarks)
        {
            var intUserOrgId = HttpContext.GetIdentityInfo<int?>("orgnizationid");

            JObject objPerson = srvPerson.GetOneRawImp(patientid);
            if (objPerson.ToInt("id") != patientid || objPerson.ToInt("orgnizationid") != intUserOrgId)
                return Response_201_write.GetResult();


            JObject objOrg = srvOrg.GetOneRawImp(desorgid);
            if (objOrg.ToInt("id") != desorgid)
                return Response_201_write.GetResult();


            // 防止重复转诊
            JArray array = _repo.GetListByPersonJointImp(patientid, int.MaxValue, 0);
            JToken active = array.Where(t => !t.Value<bool>("iscancel") && !t.Value<bool>("isfinish")).FirstOrDefault();
            if (active!=null)
                return Response_201_write.GetResult(null,"已经发起过转诊："+active.Value<string>("id"));

            // 锁定个人信息
            var username = StampUtil.Stamp(HttpContext);
            srvAppoint.LockPersonData(patientid, username);
            srvCheck.LockPersonData(patientid, username);
            srvFollowup.LockPersonData(patientid, username);
            srvTreat.LockPersonData(patientid, username);
            srvVacc.LockPersonData(patientid, username);

            // 开始转诊
            JObject res = new JObject();
            res["id"] = _srvTransfer.BeginTransfer(HttpContext,patientid,desorgid,remarks);
            objPerson["IsOnReferral"] = 1;
            srvPerson.AddOrUpdateRaw(objPerson, StampUtil.Stamp(HttpContext));
            return Response_200_write.GetResult(res);
        }

        [HttpPost("CancelTransfer")]
        public JObject CancelTransfer(
            [FromServices] PersonRepository srvPerson
            , [FromQuery] int transferId
            , [FromQuery] string remarks)
        {
            
            JObject active = _repo.GetOneRawImp(transferId);

            // 防止重复取消
            if (active == null || active.Value<bool>("iscancel") || active.Value<bool>("isfinish"))
                return Response_201_write.GetResult(null, "转诊已取消或已完成");

            JObject res = new JObject();
            if (_srvTransfer.CancelTransfer(HttpContext, transferId, remarks) > 0)
            {
                res["id"] = transferId;
                JObject objPerson = srvPerson.GetOneRawImp(active.ToInt("patientid") ?? 0);
                objPerson["IsOnReferral"] = 0;
                srvPerson.AddOrUpdateRaw(objPerson, StampUtil.Stamp(HttpContext)); // 修改转诊标志
                objPerson["isactive"] = 1;
                srvPerson.SetLock(objPerson,StampUtil.Stamp(HttpContext)); // 解锁个人信息
                return Response_200_write.GetResult(res);
            }
            else
                return Response_201_write.GetResult();
        }

        [HttpPost("AcceptTransfer")]
        public JObject AcceptTransfer([FromServices] PersonRepository srvPerson ,[FromQuery] int transferId, [FromQuery] string remarks)
        {
            // 防止重复接收
            JObject active = _repo.GetOneRawImp(transferId);
            if (active == null || active.Value<bool>("iscancel") || active.Value<bool>("isfinish"))
                return Response_201_write.GetResult(null, "转诊已取消或已完成");

            JObject res = new JObject();
            if (_srvTransfer.AcceptTransfer(HttpContext, transferId, remarks) > 0)
            {
                // 接收后回写Patient.OrgnizationID
                var transfer = _repo.GetOneRawImp(transferId);
                var objPerson = srvPerson.GetOneRawImp(transfer.ToInt("patientid") ?? 0);
                objPerson["orgnizationid"] = HttpContext.GetIdentityInfo<int?>("orgnizationid");
                objPerson["IsOnReferral"] = 0; 
                var isPersonUpdated = srvPerson.AddOrUpdateRaw(objPerson, StampUtil.Stamp(HttpContext));
                objPerson["isactive"] = 1;
                srvPerson.SetLock(objPerson, StampUtil.Stamp(HttpContext)); // 解锁个人信息
                res["id"] = isPersonUpdated > 0 ? transferId : 0;
                if (isPersonUpdated>0)
                    return Response_200_write.GetResult(res);
            }

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
