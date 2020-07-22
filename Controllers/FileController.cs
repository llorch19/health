using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using System;
/*
 * Title : “文件”控制器
 * Author: zudan
 * Date  : 2020-07-22
 * Description: 上传下载文件，需要在中间件加以权限控制
 * Comments
 */
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using util;
using util.mysql;

namespace health.Controllers
{
    /// <summary>
    /// 上传下载图片
    /// </summary>
    [ApiController]
    [Route("api")]
    public class FileController:ControllerBase
    {
        dbfactory db = new dbfactory();
        const string spliter = "$$";

        /// <summary>
        /// 上传指定“检查结果”对应的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("upload/{checkid:int}")]
        public JObject UploadFile(
         int checkid,
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            JObject res = new JObject();
            string uploadir = new config().GetValue("upload");
            if (!Directory.Exists(uploadir))
                Directory.CreateDirectory(uploadir);

            StringBuilder bPics = new StringBuilder();
            
            foreach (var f in files)
            {
                string filepath = Path.Combine(uploadir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(uploadir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));

                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    f.CopyTo(stream);
                }
                bPics.Append(Path.GetFullPath(filepath));
                bPics.Append(spliter);
            }


            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Pics"] = bPics.ToString();

            Dictionary<string, object> keys = new Dictionary<string, object>();
            keys["id"] = checkid;

            db.Update("t_detectionrecord", dict, keys);

            res["status"] = 200;
            res["msg"] = "上传成功";

            return res;
        }

        [HttpGet("pics/{checkid:int}/{index:int}")]
        public IActionResult GetFile(int checkid,int index)
        {
            JObject tmp = db.GetOne(@"SELECT ReportTime,Pics FROM t_detectionrecord WHERE ID=?p1", checkid);
            JObject res = new JObject();
            string[] pics = tmp["pics"]?.ToObject<string>()?.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            if (index >= pics?.Length)
                return NoContent();

            string filepath = pics?[index];
            if (string.IsNullOrEmpty(filepath))
                return NoContent();

            string mimeType = "application/octet-stream";
            var stream = new FileStream(filepath, FileMode.Open);

            StringBuilder bFileDownloadName = new StringBuilder();
            bFileDownloadName.Append(tmp["reporttime"]?.ToObject<DateTime>().ToString("yyyymmdd"));
            bFileDownloadName.Append(".");
            bFileDownloadName.Append(index);
            bFileDownloadName.Append(Path.GetExtension(filepath));
            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = bFileDownloadName.ToString()
            };
        }

        [HttpGet("pics/{checkid:int}")]
        public JObject GetFileList(int checkid)
        {
            JObject tmp = db.GetOne(@"SELECT Pics FROM t_detectionrecord WHERE ID=?p1", checkid);
            string[] pics = tmp["pics"]?.ToObject<string>()?.Split(spliter,StringSplitOptions.RemoveEmptyEntries);
            JArray array = new JArray();
            for (int i = 0; i < pics?.Length; i++)
            {
                array.Add(i);
            }
            JObject res = new JObject();
            res["list"] = array;
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }
    }
}
