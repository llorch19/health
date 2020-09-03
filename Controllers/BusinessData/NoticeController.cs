/*
 * Title : “通知”控制器
 * Author: zudan
 * Date  : 2020-07-23
 * Description: 对个人信息的增删查改
 * Comments
 * - 新增“通知”，单独的数据库表结构     @xuedi    2020-07-23      10:50
 * - 发布时间由服务器定，过期时间去掉，通知只针对Patient不针对User       @xuedi      2020-07-23  10:50
 */
using health.common;
using health.Controllers.BaseData;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using util;
using util.mysql;
using health.web.Service;

namespace health.Controllers
{
    [Route("api")]
    public class NoticeController : BaseController
    {
        private readonly ILogger<NoticeController> _logger;
        OrgnizationRepository _org;
        PersonRepository _person;
        IWebHostEnvironment _env;
        config conf = new config();
        string[] permittedExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };

        public NoticeController(
            NoticeRepository noticeRepository
            ,IServiceProvider serviceProvider
            ):base(noticeRepository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<NoticeController>>() ;
            _env = serviceProvider.GetService<IWebHostEnvironment>();
            _org = serviceProvider.GetService<OrgnizationRepository>();
            _person = serviceProvider.GetService<PersonRepository>();
        }

        /// <summary>
        /// 获取“通知”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的通知数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            return base.GetList();
        }


        /// <summary>
        /// 获取“通知”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“通知”信息</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);
            if (res.ToInt("status")==200)
            {
                res["orgnization"] = _org.GetAltInfo(res.ToInt("orgnizationid") ?? 0);
                res["publish"] = _person.GetUserAltInfo(res.ToInt("publishuserid") ?? 0);
                return Response_200_read.GetResult(res);
            }

            return res;
        }


        /// <summary>
        /// 更改“通知”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“通知”信息</param>
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
        /// 删除“通知”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“通知”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Del[controller]")]
        public override JObject Del([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetAltInfo(base.Get(id ?? 0).ToInt("orgnizationid"));
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Del(req);
        }

        /// <summary>
        /// 上传指定“通知”对应的附件
        /// </summary>
        /// <param name="fuc"></param>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]Zip")]
        public JObject UploadZip(
            [FromServices] FileUploadCommand fuc,
            IFormFile[] files,
            CancellationToken cancellationToken)
        {
            string uploadir = conf.GetValue("user:zip:upload");
            string physical = Path.Combine(_env.ContentRootPath, conf.GetValue("user:static"), uploadir);
            int countlimit = int.Parse(conf.GetValue("user:zip:filecount"));
            long sizelimit = long.Parse(conf.GetValue("user:zip:filesize"));
            var hasUploaded = fuc.ExecuteRelative(files, cancellationToken, physical, new string[] { ".zip" }, countlimit, sizelimit
                , out string[] lstServerFiles
                , out string strError);

            JObject res = new JObject();
            if (!hasUploaded)
                return Response_201_write.GetResult(res, strError);

            res["list"] = JArray.FromObject(lstServerFiles);
            return Response_200_write.GetResult(res, strError);
        }
    }
}