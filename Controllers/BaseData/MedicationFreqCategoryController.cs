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
    public class MedicationFreqCategoryController : AbstractBLLControllerT
    {

        private readonly ILogger<MedicationFreqCategoryController> _logger;

        public MedicationFreqCategoryController(
            MedicationFreqCategoryRepository repository
            ,IServiceProvider serviceProvider
           )
           :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<MedicationFreqCategoryController>)) as ILogger<MedicationFreqCategoryController>;
        }

        /// <summary>
        /// 获取“用药频次”列表
        /// </summary>
        /// <returns>JSON对象，包含所有可用的“用药频次”数组</returns>
        [HttpGet]
        [Route("GetMedicationFreqCategoryList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListJointImp(int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“用药频次”信息
        /// </summary>
        /// <param name="id">指定id</param>
        /// <returns>JSON对象，包含相应的“用药频次”信息</returns>
        [HttpGet]
        [Route("GetMedicationFreqCategory")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res["id"] != null)
                return Response_200_read.GetResult(res);
            else
                return Response_201_read.GetResult();
        }

        /// <summary>
        /// 修改“用药频次”
        /// </summary>
        /// <param name="req">JSON对象，包含待修改的“用药频次”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("SetMedicationFreqCategory")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }


        /// <summary>
        /// 删除“用药频次”
        /// </summary>
        /// <param name="req">JSON对象，包含待删除的“用药频次”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost("DelMedicationFreqCategory")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }


        [NonAction]
        public JObject GetFreqInfo(int? id)
        {
            return base.GetAltInfo(id);
        }

    }
}
