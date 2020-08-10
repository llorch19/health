/*
 * Title : “检测”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测”信息的增删查改
 * Comments
 * -GetUserCheckList 应该和GetPeron["check"]字段一致     @xuedi      2020-07-22      15:48
 */
using health.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using util;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class CheckController : ControllerBase
    {
        private readonly ILogger<CheckController> _logger;
        dbfactory db = new dbfactory();
        const string spliter = "$$";
        string[] _permittedPictureExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };

        public CheckController(ILogger<CheckController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“检测”列表
        /// </summary>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [HttpGet]
        [Route("GetCheckList")]
        public JObject GetList()
        {
            var orgid = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            JObject res = new JObject();
            res["list"] = db.GetArray(@"
SELECT 
t_check.ID
,IFNULL(CType,'') AS CheckType
,t_check.OperTime AS OperationTime
,t_check.ReportTime AS ReportTime
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_check.OrgnizationID
,t_orgnization.OrgName AS OrgName
,t_check.Result AS Result
,IFNULL(t_check.Recommend,'') AS Recommend
,IFNULL(t_check.Chosen,'') AS Chosen 
,t_patient.Tel AS PersonTel
,IFNULL(t_check.IsRexam,'') AS IsReexam
,t_orgnization.OrgCode
,IFNULL(CheckNO,'') AS DetectionNO
,IFNULL(t_check.ResultTypeID,'') AS ResultTypeID
, IFNULL(data_detectionresulttype.control1,'') AS CType
, IFNULL(data_detectionresulttype.control2,'') AS CValue
, IFNULL(t_check.IsActive,'') AS IsActive
FROM 
t_check
LEFT JOIN t_patient
ON t_check.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_check.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_check.ResultTypeID=data_detectionresulttype.ID
WHERE t_check.OrgnizationID =?p1
AND t_check.IsDeleted=0
", orgid);

            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
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
            res["list"] = db.GetArray(@"
SELECT 
t_check.ID
,IFNULL(CType,'') AS CheckType
,t_check.OperTime AS OperationTime
,t_check.ReportTime AS ReportTime
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_check.OrgnizationID
,t_orgnization.OrgName AS OrgName
,t_check.Result AS Result
,IFNULL(t_check.Recommend,'') AS Recommend
,IFNULL(t_check.Chosen,'') AS Chosen 
,t_patient.Tel AS PersonTel
,IFNULL(t_check.IsRexam,'') AS IsReexam
,t_orgnization.OrgCode
,IFNULL(CheckNO,'') AS DetectionNO
,IFNULL(t_check.ResultTypeID,'') AS ResultTypeID
, IFNULL(data_detectionresulttype.control1,'') AS CType
, IFNULL(data_detectionresulttype.control2,'') AS CValue
, IFNULL(t_check.IsActive,'') AS IsActive
FROM 
t_check
LEFT JOIN t_patient
ON t_check.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_check.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_check.ResultTypeID=data_detectionresulttype.ID
WHERE t_check.PatientID =?p1
AND t_check.IsDeleted=0
", personid);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取“检测”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测”信息</returns>
        [HttpGet]
        [Route("GetCheck")]
        public JObject GetCheck(int id)
        {
            JObject res = db.GetOne(@"
SELECT
ID
,IFNULL(CType,'') AS CheckType
,IFNULL(PatientID,'') AS PatientID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,t_check.Result AS Result
,IFNULL(t_check.IsRexam,'') AS IsReexam
,Recommend
,Chosen
,IFNULL(CheckNO,'') AS DetectionNO
,IFNULL(Pics,'') AS Pics
,IFNULL(Pdf,'') AS Pdf
,IFNULL(ResultTypeID,'') AS ResultTypeID
,IFNULL(PName,'') AS ProductName
,IFNULL(Spec,'') AS Specification
,IFNULL(Batch,'') AS BatchNumber
,IFNULL(OperTime,'') AS OperationTime
,IFNULL(ReportTime,'') AS ReportTime
FROM t_check
WHERE ID=?p1
AND t_check.IsDeleted=0", id);


            res["person"] = new PersonController(null, null)
                .GetPersonInfo(res["patientid"]?.ToObject<int>()??0);
            res["orgnization"] = new OrganizationController(null)
                .GetOrgInfo(res["orgnizationid"]?.ToObject<int>()??0);
            res["recommend"] = JsonConvert.DeserializeObject<JArray>(res["recommend"]?.ToObject<string>() ?? "");
            res["chosen"] = JsonConvert.DeserializeObject<JObject>(res["chosen"]?.ToObject<string>() ?? ""); 
            res["result"] = new DetectionResultTypeController(null)
                .GetResultTypeInfo(res["resulttypeid"]?.ToObject<int>() ?? 0);
            
            
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

            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
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

            JObject req=(JObject)request;

            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req.ToInt("patientid");
            dict["OrgnizationID"] = HttpContext.GetIdentityInfo<int?>("orgnizationid");
            dict["ResultTypeID"] = req.ToInt("resulttypeid");
            dict["Result"] = req["result"]?.ToObject<string>();
            dict["Recommend"] = JsonConvert.SerializeObject(req["recommend"]); 
            dict["Chosen"] = JsonConvert.SerializeObject(req["chosen"]);
            dict["IsRexam"] = req["isreexam"]?.ToObject<int?>();
            dict["CType"] = req["checktype"]?.ToObject<string>();
            dict["CheckNO"] = req["detectionno"]?.ToObject<string>();
            dict["PName"] = req["productname"]?.ToObject<string>();
            dict["Spec"] = req["specification"]?.ToObject<string>();
            dict["Batch"] = req["batchnumber"]?.ToObject<string>();
            dict["OperTime"] = req["operationtime"]?.ToObject<DateTime?>();
            dict["ReportTime"] = req["reporttime"]?.ToObject<DateTime?>();

            //dict["ClinicalNO"] = req["clinicalno"]?.ToObject<string>();
            //dict["DepartmentName"] = req["departmentname"]?.ToObject<string>();
            //dict["InpatientArea"] = req["inpatientarea"]?.ToObject<string>();
            //dict["SickbedNO"] = req["sickbedno"]?.ToObject<string>();
            //dict["SampleID"] = req["sampleid"]?.ToObject<string>();
            //dict["SampleType"] = req["sampletype"]?.ToObject<string>();
            //dict["SampleStatus"] = req["samplestatus"]?.ToObject<string>();
            //dict["SubmitBy"] = req["submitby"]?.ToObject<string>();
            //dict["SubmitTime"] = req.ToDateTime("submittime");
            //dict["ObjectiveResult"] = req["objectiveresult"]?.ToObject<string>();
            //dict["SubjectiveResult"] = req["subjectiveresult"]?.ToObject<string>();
            //dict["Pics"] = req["pics"]?.ToObject<string>();
            //dict["Pdf"] = req["pdf"]?.ToObject<string>();
            //dict["DiagnoticsTypeID"] = req.ToInt("diagnoticstypeid");
            //dict["DiagnoticsTime"] = req.ToDateTime("diagnoticstime");
            //dict["DiagnoticsBy"] = req["diagnoticsby"]?.ToObject<string>();
            //dict["ReportTime"] = req.ToDateTime("reporttime");
            //dict["ReportBy"] = req["reportby"]?.ToObject<string>();
            //dict["Reference"] = req["reference"]?.ToObject<string>();
            // TODO: ADD CheckItem HERE

            JObject res = new JObject();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = StampUtil.Stamp(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_check", dict, condi);
                res["id"] = req["id"];
            }
            else
            {
                dict["CreatedBy"] = StampUtil.Stamp(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                dict["IsActive"] = 1;
                dict["IsDeleted"] = 0;
                res["id"] = this.db.Insert("t_check", dict);
            }

            
            res["status"] = 200;
            res["msg"] = "提交成功";
            
            return res;
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
            JObject res = new JObject();
            var dict = new Dictionary<string, object>();
            dict["IsDeleted"] = 1;
            var keys = new Dictionary<string, object>();
            keys["id"] = req.ToInt("id");
            var count = db.Update("t_check", dict, keys);
            if (count > 0)
            {
                res["status"] = 200;
                res["msg"] = "操作成功";
                return res;
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "操作失败";
                return res;
            }
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
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("person:pics:upload");
            int countlimit = int.Parse(conf.GetValue("person:pics:filecount"));
            if (files.Length > countlimit)
            {
                res["status"] = 201;
                res["msg"] = "最多允许上传 " + countlimit + " 个文件";
                return res;
            }

            long sizelimit = long.Parse(conf.GetValue("person:pics:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
            {
                res["status"] = 201;
                res["msg"] = "文件大小介于0，" + sizelimit;
                return res;
            }

            JObject check = db.GetOne(@"SELECT ID,ReportTime,Pics,IsActive FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            if (check["id"] == null || !((check["isactive"]?.ToObject<bool>() ?? false)))
            {
                res["status"] = 201;
                res["msg"] = "无法上传";
                return res;
            }

            JObject picsJObject = new JObject();
            var bOk = files.Length > 0 && FileHelpers.CheckFiles(files, _permittedPictureExtensions, cancellationToken);
            string[] results = bOk? FileHelpers.UploadStorage(files,uploadir,cancellationToken):new string[0];

            for (int actionIndex = 0; actionIndex < results.Length; actionIndex++)
                picsJObject[PicKeyGenFunc(checkid)(actionIndex.ToString())] = Path.GetFullPath(results[actionIndex]);


            if (results.Length > 0)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["Pics"] = picsJObject.ToString();
                dict["LastUpdatedBy"] = StampUtil.Stamp(this.HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> keys = new Dictionary<string, object>();
                keys["id"] = checkid;

                int row = db.Update("t_check", dict, keys);

                if (row > 0)
                    foreach (var oldfile in check["pics"]?.ToObject<string>()?.Split(spliter, StringSplitOptions.RemoveEmptyEntries))
                        if (System.IO.File.Exists(oldfile))
                            System.IO.File.Delete(oldfile);

                res = GetPicsList(checkid);
                res["msg"] = "上传成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "上传失败";
            }

            return res;
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
                return NoContent();

            string filepath = pics?.ToArray()?.FirstOrDefault(pic=>pic.Key==index.ToString()).Value;
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
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
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
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("person:pdf:upload");
            int countlimit = int.Parse(conf.GetValue("person:pdf:filecount"));
            if (files.Length > countlimit)
            {
                res["status"] = 201;
                res["msg"] = "最多允许上传 " + countlimit + " 个文件";
                return res;
            }

            long sizelimit = long.Parse(conf.GetValue("person:pdf:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
            {
                res["status"] = 201;
                res["msg"] = "文件大小介于0，" + sizelimit;
                return res;
            }

            JObject check = db.GetOne(@"SELECT ID,ReportTime,Pdf,IsActive FROM t_check WHERE ID=?p1 AND IsDeleted=0", checkid);
            if (check["id"] == null || !((check["isactive"]?.ToObject<bool>() ?? false)))
            {
                res["status"] = 201;
                res["msg"] = "无法上传";
                return res;
            }

            JObject picsJObject = new JObject();
            var bOk = files.Length > 0 && FileHelpers.CheckFiles(files, new string[] { ".pdf" }, cancellationToken);
            string[] results = bOk ? FileHelpers.UploadStorage(files, uploadir, cancellationToken) : new string[0];

            for (int actionIndex = 0; actionIndex < results.Length; actionIndex++)
                picsJObject[PDFKeyGenFunc(checkid)(actionIndex.ToString())] = Path.GetFullPath(results[actionIndex]);

            if (results.Length > 0)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["Pdf"] = picsJObject.ToString();
                dict["LastUpdatedBy"] = StampUtil.Stamp(this.HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                Dictionary<string, object> keys = new Dictionary<string, object>();
                keys["id"] = checkid;

                int row = db.Update("t_check", dict, keys);

                if (row > 0)
                    foreach (var oldfile in check["pdf"]?.ToObject<string>()?.Split(spliter, StringSplitOptions.RemoveEmptyEntries))
                        if (System.IO.File.Exists(oldfile))
                            System.IO.File.Delete(oldfile);

                res = GetPDFList(checkid);
                res["msg"] = "上传成功";
                res["status"] = 200;
                
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "上传失败";
            }

            return res;
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