/*
 * Title : 个人信息管理控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - Get返回值应该同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi 2020-07-14 09:07
 * - Post提交值将同时包含：个人信息、检查诊断信息、复查信息、推荐疫苗信息和随访信息 @xuedi  2020-07-14 09:08
 * - 使用JObject["personinfo"]=db.GetOne() 这种形式逐个添加所需信息             @norway 2020-07-14 10:24
 * - GetList 需要附带返回Person列表的总条数。                                   @xuedi  2020-07-17  10:47
 * - Post提交值将只包含：个人信息。检测及随访的针对各自的接口进行POST。            @xuedi  2020-07-17  17:21
 * - 先不检查复诊字段       @xuedi  2020-07-22  10:35
 */
using health.BaseData;
using health.common;
using health.web.Domain;
using health.web.StdResponse;
using IdGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class PersonController : AbstractBLLControllerT
    {
        private readonly ILogger<PersonController> _logger;
        Lazy<OrganizationController> _org;
        Lazy<GenderController> _gender;
        Lazy<OccupationController> _occupation;
        Lazy<AddressCategoryController> _addrcategory;
        Lazy<CheckController> _check;
        Lazy<AppointController> _appoint;
        Lazy<AttandentController> _attandent;
        Lazy<FollowupController> _followup;
        Lazy<TreatController> _treat;
        Lazy<TreatItemController> _treatitem;
        Lazy<VaccController> _vacc;
        IdGenerator _idGenerator;
        PersonRepository _repository;

        public PersonController(PersonRepository repository,
                                IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<PersonController>)) as ILogger<PersonController>;
            _idGenerator = serviceProvider.GetService(typeof(IdGenerator)) as IdGenerator;
            _org = serviceProvider.GetService(typeof(Lazy<OrganizationController>)) as Lazy<OrganizationController>;
            _gender = serviceProvider.GetService(typeof(Lazy<GenderController>)) as Lazy<GenderController>;
            _occupation = serviceProvider.GetService(typeof(Lazy<OccupationController>)) as Lazy<OccupationController>;
            _addrcategory = serviceProvider.GetService(typeof(Lazy<AddressCategoryController>)) as Lazy<AddressCategoryController>;
            _check = serviceProvider.GetService(typeof(Lazy<CheckController>)) as Lazy<CheckController>;
            _appoint = serviceProvider.GetService(typeof(Lazy<AppointController>)) as Lazy<AppointController>;
            _attandent = serviceProvider.GetService(typeof(Lazy<AttandentController>)) as Lazy<AttandentController>;
            _followup = serviceProvider.GetService(typeof(Lazy<FollowupController>)) as Lazy<FollowupController>;
            _treat = serviceProvider.GetService(typeof(Lazy<TreatController>)) as Lazy<TreatController>;
            _treatitem = serviceProvider.GetService(typeof(Lazy<TreatItemController>)) as Lazy<TreatItemController>;
            _vacc = serviceProvider.GetService(typeof(Lazy<VaccController>)) as Lazy<VaccController>;
            _repository = repository;
        }

        /// <summary>
        /// 获取个人列表，[人员转诊]菜单
        /// </summary>
        /// <returns>JSON数组形式的个人信息</returns>
        [HttpGet]
        [Route("GetPersonListD")]
        public override JObject GetList()
        {
            return GetPersonList(Const.defaultPageSize, Const.defaultPageIndex);
        }


        /// <summary>
        /// 获取个人列表，[人员转诊]菜单
        /// </summary>
        /// <returns>JSON数组形式的个人信息</returns>
        [HttpGet]
        [Route("GetPersonList")]
        public JObject GetPersonList(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            JObject res = GetPersonListImp(pageSize,pageIndex);
            return Response_200_read.GetResult(res);
        }

        [NonAction]
        public JObject GetPersonListImp(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);

            JObject res = new JObject();
            JArray list = _repo.GetListByOrgJointImp(orgid ?? 0,pageSize,pageIndex);
            res["list"] = list;
            return res;
        }

        [NonAction]
        public JObject GetPersonRawImp(int id)
        {
            common.BaseConfig conf = new common.BaseConfig();
            var res = _repo.GetOneRawImp(id);
            if (!res.HasValues)
                return res;

            res["primaryorg"] = _org.Value.GetOrgInfo(res["primaryorgnizationid"].ToObject<int>());
            res["orgnization"] = _org.Value.GetOrgInfo(res["orgnizationid"].ToObject<int>());

            res["gender"] = _gender.Value.GetGenderInfo(res["genderid"].ToObject<int>());
            res["occupation"] = _occupation.Value.GetOccupationInfo(res["occupationcategoryid"].ToObject<int>());
            res["addresscategory"] = _addrcategory.Value.GetAddressCategoryInfo(res["addresscategoryid"].ToObject<int>());

            res["province"] = conf.GetAreaInfo(res["provinceid"].ToObject<int>());
            res["city"] = conf.GetAreaInfo(res["cityid"].ToObject<int>());
            res["county"] = conf.GetAreaInfo(res["countyid"].ToObject<int>());


            return res;
        }


        /// <summary>
        /// 获取人员信息，初始化[人员信息录入]菜单
        /// </summary>
        /// <returns>JSON形式的某位个人信息，包括个人信息，</returns>
        [HttpGet]
        [Route("GetPerson")]
        public override JObject Get(int id)
        {
            JObject res = new JObject();
            res["personinfo"] = GetPersonRawImp(id);
            if (res["personinfo"]?.HasValues==false)
                return Response_201_read.GetResult();

            res["checkinfo"] = _check.Value.GetListPImp(id);
            res["treatinfo"] = _treat.Value.GetListPImp(id);

            res["followupinfo"] = _followup.Value.GetListPImp(id);
            res["vaccinfo"] = _vacc.Value.GetListPImp(id);

            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 更改个人信息。如果id=0新增个人信息，如果id>0修改个人信息。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的个人信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetPerson")]
        public override JObject Set([FromBody] JObject req)
        {
            DateTime dt;
            if (DateTime.TryParse(req["birthday"].ToObject<string>(), out dt))
            {
                req["Birthday"] = dt;
            }


            if (req.ToInt("id") == 0)
            {
                // 新增人员生成邀请码和档案号
                req["InviteCode"] = ShareCodeUtils.New();
                req["RegisterNO"] = _idGenerator.CreateId();
                // 在 添加Attandent 记录
                // 为Patient指定当前orgid
            }
            return base.Set(req);
        }

        /// <summary>
        /// 删除个人信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的个人信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelPerson")]
        public override JObject Del([FromBody] JObject req)
        {
            req["orgnization"] = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            req["idcardno"] = null; // 身份证上有Uniq唯一索引
            return base.Del(req);
        }

        /// <summary>
        /// TODO: 转诊
        /// </summary>
        /// <param name="req">在请求body中JSON形式的转诊信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("Transfer")]
        public JObject Transfer([FromBody] JObject req)
        {
            JObject res = new JObject();
            res["status"] = 201;
            res["msg"] = "功能正在开发中";
            return res;
        }


        [HttpGet]
        [Route("GetId")]
        public string GetId()
        {
            return _idGenerator.CreateId().ToString();
        }


        [NonAction]
        public JObject GetPersonInfo(int? id)
        {
            return _repository.GetAltInfo(id);
        }


        [NonAction]
        public JObject GetUserInfo(int? id)
        {
            return _repository.GetUserAltInfo(id ?? 0);
        }

    }
}
