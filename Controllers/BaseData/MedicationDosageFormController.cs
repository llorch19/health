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
    public class MedicationDosageFormController : BaseController
    {

        private readonly ILogger<MedicationDosageFormController> _logger;

        public MedicationDosageFormController(MedicationDosageFormRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<MedicationDosageFormController>)) as ILogger<MedicationDosageFormController>;
        }

        /// <summary>
        /// 获取“药物剂型”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“药物剂型”数组</returns>
        [HttpGet]
        [Route("GetMedicationDosageFormList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“药物剂型”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“药物剂型”信息</returns>
        [HttpGet]
        [Route("GetMedicationDosageForm")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res["id"] != null)
                return Response_200_read.GetResult(res);
            else
                return Response_201_read.GetResult();
        }


        /// <summary>
        /// 修改“药物剂型”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“药物剂型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetMedicationDosageForm")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“药物剂型”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“药物剂型”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelMedicationDosageForm")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        [NonAction]
        public JObject GetDosageInfo(int? id)
        {
            return base.GetAltInfo(id);
        }
    }
}
