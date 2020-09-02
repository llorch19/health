/*
 * Title : “检测”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测”信息的增删查改
 * Comments
 * -GetUserCheckList 应该和GetPeron["check"]字段一致     @xuedi      2020-07-22      15:48
 */
using health.common;
using health.web.Domain;
using health.web.StdResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509.SigI;
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
    public class CheckController : AbstractBLLControllerT
    {
        private readonly ILogger<CheckController> _logger;
        PersonRepository _person;
        OrgnizationRepository _org;
        DetectionResultTypeRepository _rtype;
        TreatmentOptionRepository _toption;

        dbfactory db = new dbfactory();
        string[] _permittedPictureExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };

        public CheckController(
            CheckRepository repository
            ,IServiceProvider serviceProvider)
            :base(repository,serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<CheckController>>();
            _person = serviceProvider.GetService<PersonRepository>();
            _org = serviceProvider.GetService<OrgnizationRepository>();
            _rtype = serviceProvider.GetService<DetectionResultTypeRepository>();
            _toption = serviceProvider.GetService<TreatmentOptionRepository>();
        }

        /// <summary>
        /// 获取机构的“检测”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [HttpGet]
        [Route("GetCheckList")]
        public override JObject GetList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = _repo.GetListByOrgJointImp(orgid ?? 0, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取个人的“检测”列表
        /// </summary>
        /// <param name="personid">请求的个人id</param>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [HttpGet]
        [Route("GetCheckListP")]
        public JObject GetListP(int personid)
        {
            JObject res = new JObject();
            res["list"] = _repo.GetListByPersonJointImp(personid, int.MaxValue, 0);
            return Response_200_read.GetResult(res);
        }

        /// <summary>
        /// 获取“检测”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测”信息</returns>
        [HttpGet]
        [Route("GetCheck")]
        public override JObject Get(int id)
        {
            JObject res = base.Get(id);


            res["person"] = _person.GetAltInfo(res["patientid"]?.ToObject<int>() ?? 0);
            res["resulttype"] = _rtype.GetAltInfo(res["resulttypeid"]?.ToObject<int>() ?? 0);
            res["orgnization"] = _org.GetAltInfo(res["orgnizationid"]?.ToObject<int>() ?? 0);
            
            res["recommend"] = JsonConvert.DeserializeObject<JArray>(res["recommend"]?.ToObject<string>() ?? "");
            res["chosen"] = JsonConvert.DeserializeObject<JObject>(res["chosen"]?.ToObject<string>() ?? "");



            var tmpPics = JsonConvert.DeserializeObject<JObject>(res["pics"]?.ToObject<string>() ?? "");
            res.Remove("pics");
            if (tmpPics?.HasValues == true)
            {
                var dict = tmpPics.ToObject<Dictionary<string, object>>();
                JArray pics = JArray.FromObject(dict.Select(item => (JToken)PicUrlGenFunc(id)(item.Key)));
                res["pics"] = pics;
            }

            
            var tmpPDF = JsonConvert.DeserializeObject<JObject>(res["pdf"]?.ToObject<string>() ?? "");
            res.Remove("pdf");
            if (tmpPDF?.HasValues == true)
            {
                var dict = tmpPDF.ToObject<Dictionary<string, object>>();
                JArray pdf = JArray.FromObject(dict.Select(item => (JToken)PDFUrlGenFunc(id)(item.Key)));
                res["pdf"] = pdf;
            }

            return Response_200_read.GetResult(res);
        }


        /// <summary>
        /// 更改“检测”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="request">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetCheck")]
        public JObject SetCheck([FromBody] dynamic request)
        {
            JObject req = (JObject)request;
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


            JArray recommend = JArray.FromObject(req["recommend"]);
            bool bNonResultRecommend = req.Challenge(r =>
                string.IsNullOrEmpty(r["result"]?.ToObject<string>())
                && recommend.HasValues
            );
            if (bNonResultRecommend)
                return Response_201_write.GetResult(null, "未保存检测结果，不可以推荐方案");
            req["recommend"] = JsonConvert.SerializeObject(recommend);
            JObject chosen = JObject.FromObject(req["chosen"]);
            req["chosen"] = JsonConvert.SerializeObject(chosen);
            var bChoiceInRecommend =
                req["recommend"].ToString().Contains(req["chosen"].ToString())
                || (!recommend.HasValues && !chosen.HasValues
                || (recommend.HasValues && !chosen.HasValues));
            if (!bChoiceInRecommend)
                return Response_201_write.GetResult(null, "选择方案与提供方案不符");

            return base.Set(req);
        }
        /// <summary>
        /// 删除“检测”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelCheck")]
        public JObject DelCheck([FromBody] JObject req)
        {
            var id = req.ToInt("id");
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            var orgaltinfo = _org.GetAltInfo(base.Get(id ?? 0).ToInt("orgnizationid"));
            var canwrite = req.Challenge(r => orgaltinfo.ToInt("id") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult();


            return base.Del(req);
        }



        #region 上传下载个人图片

        /// <summary>
        /// 上传指定“检查结果”对应的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]Pics")]
        public JObject UploadPics(
         int checkid,
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("person:pics:upload");
            int countlimit = int.Parse(conf.GetValue("person:pics:filecount"));
            if (files.Length > countlimit)
                return Response_201_write.GetResult(null, "最多允许上传 " + countlimit + " 个文件");
            

            long sizelimit = long.Parse(conf.GetValue("person:pics:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
                return Response_201_write.GetResult(null, "文件大小介于0，" + sizelimit);
            

            JObject check = db.GetOne(@"SELECT ID,OrgnizationID,ReportTime,Pics,IsActive FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            var canwrite = check.Challenge(r =>
                r["id"] != null
                && (r["isactive"]?.ToObject<bool?>() ?? false)
                && r.ToInt("orgnizationid") == orgid);
            if (!canwrite)
                return Response_201_write.GetResult(null, "无法提交相应的数据");

            JObject objPICObject = new JObject();
            var bOk = files.Length > 0 && FileHelpers.CheckFiles(files, _permittedPictureExtensions, cancellationToken);
            string[] results = bOk? FileHelpers.UploadStorage(files,uploadir,cancellationToken):new string[0];

            for (int actionIndex = 0; actionIndex < results.Length; actionIndex++)
                objPICObject[PicKeyGenFunc(checkid)(actionIndex.ToString())] = Path.GetFullPath(results[actionIndex]);


            if (results.Length > 0)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["Pics"] = JsonConvert.SerializeObject(objPICObject);
                dict["LastUpdatedBy"] = StampUtil.Stamp(this.HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> keys = new Dictionary<string, object>();
                keys["id"] = checkid;

                int row = db.Update("t_check", dict, keys);

                if (row > 0 && !string.IsNullOrEmpty(check["pics"]?.ToObject<string>()))
                    foreach (var oldfile in check["pics"]?.ToObject<Dictionary<string, string>>()?.Values)
                        if (System.IO.File.Exists(oldfile))
                            System.IO.File.Delete(oldfile);

                res = GetPicsList(checkid);
                return Response_200_write.GetResult(res,"上传成功");
            }
            else
                return Response_201_write.GetResult(null,"上传失败，请重试");
        }



        


        private Func<string, string> PicKeyGenFunc(int checkid)
        {
            config conf = new config();
            string domain = conf.GetValue("sys:domain");
            return index => index.ToString();
        }
        private Func<string, string> PicUrlGenFunc(int checkid)
        {
            config conf = new config();
            string domain = conf.GetValue("sys:domain");
            return index => string.Format("{0}/api/GetCheckPic?checkid={1}&index={2}",domain,checkid,index);
        }


        /// <summary>
        /// 获取指定“检查编号”对应的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpGet("Get[controller]Pic")]
        public IActionResult GetPic(int checkid, int index)
        {
            JObject check = db.GetOne(@"SELECT ReportTime,Pics FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            JObject res = new JObject();
            var pics = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                check["pics"]?.ToObject<string>()??"");
            if (pics?.Keys == null)
                return Ok(Response_201_read.GetResult());

            string filepath = pics?.ToArray()?.FirstOrDefault(pic=>pic.Key==index.ToString()).Value;
            if (string.IsNullOrEmpty(filepath))
                return Ok(Response_201_read.GetResult());

            string mimeType = FileHelpers.mimetype[Path.GetExtension(filepath)];
            var stream = new FileStream(filepath, FileMode.Open);

            StringBuilder bFileDownloadName = new StringBuilder();
            bFileDownloadName.Append(check["reporttime"]?.ToObject<DateTime>().ToString("yyyymmdd"));
            bFileDownloadName.Append("-");
            bFileDownloadName.Append(index);
            bFileDownloadName.Append(Path.GetExtension(filepath));
            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = bFileDownloadName.ToString()
            };
        }
        /// <summary>
        /// 获取指定检查结果中包含的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Get[controller]Pics")]
        public JObject GetPicsList(int checkid)
        {
            JObject tmp = db.GetOne(@"SELECT Pics FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            var pics = JsonConvert.DeserializeObject<Dictionary<string,string>>(
                tmp["pics"]?.ToObject<string>()??"");
            JArray array = pics!=null
                ?JArray.FromObject(pics.Select(pic=> PicUrlGenFunc(checkid)(pic.Key)))
                :null;

            JObject res = new JObject();
            res["list"] = array;
            return Response_200_read.GetResult(res);
        }

        #endregion


        #region 上传下载个人PDF

        /// <summary>
        /// 上传指定“检查结果”对应的PDF
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]PDF")]
        public JObject UploadPDFFile(
         int checkid,
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("person:pdf:upload");
            int countlimit = int.Parse(conf.GetValue("person:pdf:filecount"));
            if (files.Length > countlimit)
                return Response_201_write.GetResult(null, "最多允许上传 " + countlimit + " 个文件");
            

            long sizelimit = long.Parse(conf.GetValue("person:pdf:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
                return Response_201_write.GetResult(null, "文件大小介于0，" + sizelimit);


            JObject check = db.GetOne(@"SELECT ID,OrgnizationID,ReportTime,Pdf,IsActive FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            var canwrite = check.Challenge(r=> 
                r["id"] != null
                && (r["isactive"]?.ToObject<bool?>()??false)
                && r.ToInt("orgnizationid")==orgid);
            if (!canwrite)
                return Response_201_write.GetResult(null, "无法提交相应的数据");

            JObject objPDFObject = new JObject();
            var bOk = files.Length > 0 && FileHelpers.CheckFiles(files, new string[] { ".pdf" }, cancellationToken);
            string[] results = bOk ? FileHelpers.UploadStorage(files, uploadir, cancellationToken) : new string[0];

            for (int actionIndex = 0; actionIndex < results.Length; actionIndex++)
                objPDFObject[PDFKeyGenFunc(checkid)(actionIndex.ToString())] = Path.GetFullPath(results[actionIndex]);

            if (results.Length > 0)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["Pdf"] = JsonConvert.SerializeObject(objPDFObject);
                dict["LastUpdatedBy"] = StampUtil.Stamp(this.HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> keys = new Dictionary<string, object>();
                keys["id"] = checkid;

                int row = db.Update("t_check", dict, keys);

                if (row > 0 && !string.IsNullOrEmpty(check["pdf"]?.ToObject<string>()))
                    foreach (var oldfile in check["pdf"]?.ToObject<Dictionary<string, string>>()?.Values)
                        if (System.IO.File.Exists(oldfile))
                            System.IO.File.Delete(oldfile);

                res = GetPDFList(checkid);
                return Response_200_write.GetResult(res, "上传成功");
            }
            else
                return Response_201_write.GetResult(null,"上传失败，请重试");
        }




        private Func<string, string> PDFKeyGenFunc(int checkid)
        {
            config conf = new config();
            string domain = conf.GetValue("sys:domain");
            return index => index.ToString();
        }
        private Func<string, string> PDFUrlGenFunc(int checkid)
        {
            config conf = new config();
            string domain = conf.GetValue("sys:domain");
            return index => string.Format("{0}/api/GetCheckPDF?checkid={1}&index={2}", domain, checkid, index);
        }


        /// <summary>
        /// 获取指定“检查编号”对应的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpGet("Get[controller]PDF")]
        public IActionResult GetPDF(int checkid, int index)
        {
            JObject check = db.GetOne(@"SELECT ReportTime,Pdf FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            JObject res = new JObject();
            var pics = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                check["pdf"]?.ToObject<string>()??"");
            if (pics?.Keys == null)
                return NoContent();

            string filepath = pics?.ToArray()?.FirstOrDefault(pic => pic.Key == index.ToString()).Value;
            if (string.IsNullOrEmpty(filepath))
                return NoContent();

            string mimeType = FileHelpers.mimetype[Path.GetExtension(filepath)];
            var stream = new FileStream(filepath, FileMode.Open);

            StringBuilder bFileDownloadName = new StringBuilder();
            bFileDownloadName.Append(check["reporttime"]?.ToObject<DateTime>().ToString("yyyymmdd"));
            bFileDownloadName.Append("-");
            bFileDownloadName.Append(index);
            bFileDownloadName.Append(Path.GetExtension(filepath));
            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = bFileDownloadName.ToString()
            };
        }
       
        
        /// <summary>
        /// 获取指定检查结果中包含的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <returns></returns>
        /// <summary>
        /// 获取指定检查结果中包含的图片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Get[controller]PDFList")]
        public JObject GetPDFList(int checkid)
        {
            JObject tmp = db.GetOne(@"SELECT Pdf FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            var pdf = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                tmp["pdf"]?.ToObject<string>() ?? "");
            JArray array =pdf!=null
                ?JArray.FromObject(pdf?.Select(p => PDFUrlGenFunc(checkid)(p.Key)))
                :null;
            JObject res = new JObject();
            res["list"] = array;
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        #endregion
    }
}