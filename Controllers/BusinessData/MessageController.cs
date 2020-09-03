/*
 * Title : “公告”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - 发布时间由服务器定，过期时间去掉，公告只针对Patient不针对User       @xuedi      2020-07-23  10:50
 */
using health.common;
using health.Controllers.BaseData;
using health.web.Controllers;
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
    public class MessageController : BaseNonPagedController
    {
        private readonly ILogger<MessageController> _logger;
        OrgnizationRepository _org;
        PersonRepository _person;
        IWebHostEnvironment _env;
        config conf = new config();
        const string spliter = "$$";
        private readonly string[] permittedExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };

        public MessageController(
            MessageInOrgRepository messageInOrgRepository
            , IServiceProvider serviceProvider
            )
            : base(messageInOrgRepository, serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<MessageController>>();
            _env = serviceProvider.GetService<IWebHostEnvironment>(); ;
            _org = serviceProvider.GetService<OrgnizationRepository>();
            _person = serviceProvider.GetService<PersonRepository>();
        }

        /// <summary>
        /// 获取“公告”列表，[科普公告]菜单
        /// </summary>
        /// <returns>JSON对象，包含相应的公告数组</returns>
        [HttpGet]
        [Route("GetMessageList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["list"] = base.GetList();
            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 获取“公告”信息，点击[科普公告]中的一个栏目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“公告”信息</returns>
        [NonAction]
        public override JObject Get(int id)
        {
            return base.Get(id);
        }


        /// <summary>
        /// 获取“公告”信息，点击[科普公告]中的一个栏目
        /// </summary>
        /// <param name="option"></param>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“公告”信息</returns>
        [HttpGet]
        [Route("GetMessage")]
        public JObject Get([FromServices] OptionRepository option, int id)
        {
            JObject res = base.Get(id);
            if (res["id"] == null)
                return Response_201_read.GetResult();

            var fileserver = option.GetOptionByName("fileserver");
            res["thumbnail"] = fileserver["value"]?.ToObject<string>()
                + res["thumbnail"]?.ToObject<string>();
            res["attachment"] = fileserver["value"]?.ToObject<string>()
                + res["attachment"]?.ToObject<string>();

            res["orgnization"] = _org.GetAltInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            res["publish"] = _person.GetUserAltInfo(res["publishuserid"]?.ToObject<int>() ?? 0);

            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“公告”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“公告”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetMessage")]
        public override JObject Set([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var userid = HttpContext.GetIdentityInfo<int?>("id");
            var id = req.ToInt("id");
            if (id == 0)
            {
                // 新增
                req["orgnizationid"] = orgid;
                req["publishuserid"] = userid;
            }
            else
            {
                var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
                if (!canwrite)
                    return Response_201_write.GetResult();
            }

            return base.Set(req);
        }




        /// <summary>
        /// 删除“公告”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“公告”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelMessage")]
        public override JObject Del([FromBody] JObject req)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var canwrite = req.Challenge(r => r.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();

            return base.Del(req);
        }


        /// <summary>
        /// 上传指定“公告”对应的图片
        /// </summary>
        /// <param name="fuc"></param>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]Pics")]
        public JObject UploadPics(
        [FromServices] FileUploadCommand fuc,
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            string uploadir = conf.GetValue("user:pic:upload");
            string physical = Path.Combine(_env.ContentRootPath, conf.GetValue("user:static"), uploadir);
            int countlimit = int.Parse(conf.GetValue("user:pic:filecount"));
            long sizelimit = long.Parse(conf.GetValue("user:pic:filesize"));
            var hasUploaded = fuc.ExecuteRelative(files, cancellationToken, physical, permittedExtensions, countlimit, sizelimit
                , out string[] lstServerFiles
                , out string strError);

            JObject res = new JObject();
            if (!hasUploaded)
                return Response_201_write.GetResult(res, strError);

            res["list"] = JArray.FromObject(lstServerFiles);
            return Response_200_write.GetResult(res, strError);
        }


        /// <summary>
        /// 上传指定“公告”对应的附件
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