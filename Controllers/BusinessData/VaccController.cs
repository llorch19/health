/*
 * Title : “接种记录”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“接种记录”信息的增删查改
 * Comments
 * - GetOrgVaccList 应该和GetPeron["vacc"]字段一致     @xuedi      2020-07-22      15:20
 */
using health.common;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace health.Controllers
{
    [Route("api")]
    public class VaccController : AbstractBLLControllerT
    {
        private readonly ILogger<VaccController> _logger;
        PersonController _person;
        OrganizationController _org;
        MedicationController _med;
        MedicationDosageFormController _dosage;
        MedicationPathwayController _pathway;

        public VaccController(
            VaccRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<VaccController>>();
            _person = serviceProvider.GetService<PersonController>();
            _org = serviceProvider.GetService<OrganizationController>();
            _med = serviceProvider.GetService<MedicationController>();
            _dosage = serviceProvider.GetService<MedicationDosageFormController>();
            _pathway = serviceProvider.GetService<MedicationPathwayController>();
        }

        /// <summary>
        /// 获取机构的“接种记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取个人的“接种记录”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“接种记录”数组</returns>
        [HttpGet]
        [Route("Get[controller]ListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取“接种记录”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“接种记录”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);

            res["person"] = _person.GetPersonInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["org"] = _org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["medication"] = _med.GetMedicationInfo(res["medicationid"]?.ToObject<int>() ?? 0);
            res["dosage"] = _dosage.GetDosageInfo(res["medicationdosageformid"]?.ToObject<int>() ?? 0);
            res["pathway"] = _pathway.GetPathwayInfo(res["medicationpathwayid"]?.ToObject<int>() ?? 0);

            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“接种记录”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Set[controller]")]
        public override JObject Set([FromBody] JObject req)
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




        /// <summary>
        /// 删除“接种记录”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“接种记录”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetOrgInfo(id);
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Del(req);
        }
    }
}