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
        string[] permittedExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };

        public MessageController(
            MessageInOrgRepository messageInOrgRepository
            ,IServiceProvider serviceProvider
            )
            :base(messageInOrgRepository,serviceProvider)
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
        public JObject Get([FromServices] OptionRepository option,int id)
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
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]Pics")]
        public JObject UploadPics(
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("user:pic:upload");
            int countlimit = int.Parse(conf.GetValue("user:pic:filecount"));
            if (files.Length > countlimit)
            {
                res["status"] = 201;
                res["msg"] = "最多允许上传 " + countlimit + " 个图像文件";
                return res;
            }

            long sizelimit = long.Parse(conf.GetValue("user:pic:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
            {
                res["status"] = 201;
                res["msg"] = "文件大小介于0，" + sizelimit;
                return res;
            }


            string filestore = Path.Combine(_env.ContentRootPath, conf.GetValue("user:static"), uploadir);

            if (!Directory.Exists(filestore))
                Directory.CreateDirectory(filestore);

            JArray array = new JArray();
            foreach (var f in files)
            {
                string filepath = Path.Combine(filestore, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(filestore, Path.GetRandomFileName() + Path.GetExtension(f.FileName));

                using (var memoryStream = new MemoryStream())
                {
                    f.CopyTo(memoryStream);
                    if (!FileHelpers.IsValidFileExtensionAndSignature(f.FileName, memoryStream, permittedExtensions))
                    {
                        res["status"] = 201;
                        res["msg"] = f.FileName + " 不能上传";
                        return res;
                    }
                    else
                    {
                        using (var fileStream = new FileStream(filepath, FileMode.Create))
                            f.CopyTo(fileStream);
                    }
                }
                Uri full = new Uri(filepath);
                Uri baseUri = new Uri(_env.ContentRootPath);
                array.Add(full.ToString().Replace(baseUri.ToString(), ""));
            }


            res["list"] = array;
            res["status"] = 200;
            res["msg"] = "上传成功";

            return res;
        }

        /// <summary>
        /// 上传指定“公告”对应的附件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]Zip")]
        public JObject UploadZip(
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("user:zip:upload");
            int countlimit = int.Parse(conf.GetValue("user:zip:filecount"));
            if (files.Length > countlimit)
            {
                res["status"] = 201;
                res["msg"] = "最多允许上传 " + countlimit + " 个zip文件";
                return res;
            }

            long sizelimit = long.Parse(conf.GetValue("user:zip:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
            {
                res["status"] = 201;
                res["msg"] = "文件大小介于0，" + sizelimit;
                return res;
            }


            string filestore = Path.Combine(_env.ContentRootPath, conf.GetValue("user:static"), uploadir);

            if (!Directory.Exists(filestore))
                Directory.CreateDirectory(filestore);

            JArray array = new JArray();
            foreach (var f in files)
            {
                string filepath = Path.Combine(filestore, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(filestore, Path.GetRandomFileName() + Path.GetExtension(f.FileName));

                using (var memoryStream = new MemoryStream())
                {
                    f.CopyTo(memoryStream);
                    if (!FileHelpers.IsValidFileExtensionAndSignature(f.FileName, memoryStream, new string[] { ".zip" }))
                    {
                        res["status"] = 201;
                        res["msg"] = f.FileName + " 不能上传";
                        return res;
                    }
                    else
                    {
                        using (var fileStream = new FileStream(filepath, FileMode.Create))
                            f.CopyTo(fileStream);
                    }
                }

                Uri full = new Uri(filepath);
                Uri baseUri = new Uri(_env.ContentRootPath);
                array.Add(full.ToString().Replace(baseUri.ToString(), ""));
            }

            res["list"] = array;
            res["status"] = 200;
            res["msg"] = "上传成功";

            return res;
        }

    }
}