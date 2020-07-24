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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    [ApiController]
    [Route("api")]
    public class NoticeController : AbstractBLLController
    {
        private readonly ILogger<NoticeController> _logger;
        IWebHostEnvironment _env;
        config conf = new config();
        string[] permittedExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };
        public override string TableName => "t_notice";

        public NoticeController(ILogger<NoticeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// 获取“通知”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的通知数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            string sql = @"
SELECT 
IFNULL(t_notice.ID,'') AS ID
,IFNULL(t_notice.OrgnizationID,'') AS OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName
,IFNULL(t_notice.PublishUserID,'') AS PublishUserID
,IFNULL(t_user.FamilyName,'') AS Publish
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') AS Attachment
FROM t_notice
LEFT JOIN t_orgnization
ON t_notice.OrgnizationID=t_orgnization.ID
LEFT JOIN t_user
ON t_notice.PublishUserID=t_user.ID
";

            JArray list = db.GetArray(sql);
            if (list.HasValues)
            {
                res["status"] = 200;
                res["msg"] = "获取数据成功";
                res.Add("list", list);
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "没有获取到相应的数据";
            }
            return res;
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
            string sql = @"SELECT 
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PublishUserID,'') AS PublishUserID
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') AS Attachment
FROM t_notice
WHERE ID=?p1";

            JObject res = db.GetOne(sql, id);
            if (res["id"] != null)
            {
                OrgnizationController org = new OrgnizationController(null);
                res["orgnization"] = org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
                PersonController person = new PersonController(null, null);
                res["publish"] = person.GetUserInfo(res["publishuserid"]?.ToObject<int>() ?? 0);
                res["status"] = 200;
                res["msg"] = "获取数据成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "没有获取到相应的数据";
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
            return base.Del(req);
        }

        /// <summary>
        /// 只有机构用户可以查看并编辑未发布的通知
        /// </summary>
        /// <returns></returns>
        private bool IsOrgUser()
        {
            return true;
        }


        /// <summary>
        /// 上传指定“通知”对应的图片
        /// </summary>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //[HttpPost("Upload[controller]Pics")]
        [NonAction]
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
        /// 上传指定“通知”对应的附件
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

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = 1;
            dict["PublishUserID"] = 4;
            dict["Content"] = req["content"]?.ToObject<string>();
            dict["Attachment"] = req["attachment"]?.ToObject<string>();

            return dict;
        }
    }
}