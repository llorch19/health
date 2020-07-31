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
        string[] permittedExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif" };

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
t_detectionrecord.ID
,CheckType
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_detectionrecord.OrgnizationID
,t_orgnization.OrgName AS OrgName
,RecommendedTreatID
,recom.`Name` AS Recommend
,ChosenTreatID
,chosen.`Name` AS Chosen
,t_patient.Tel AS PersonTel
,IsReexam
,t_detectionrecord.GenderID
,data_gender.GenderName 
,SubmitBy
,submit.ChineseName AS Submitter
,SubmitTime
,t_orgnization.OrgCode
,DetectionNO
,ClinicalNO
,Pics
,Pdf
,DiagnoticsTypeID
,data_detectionresulttype.ResultName
,DiagnoticsTime
,DiagnoticsBy
,diagnotics.ChineseName AS Diagnoser
,ReportTime
,ReportBy
,report.ChineseName AS Reporter
,Reference
, IFNULL(t_detectionrecord.IsActive,'') AS IsActive
FROM 
t_detectionrecord
LEFT JOIN t_patient
ON t_detectionrecord.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecord.OrgnizationID=t_orgnization.ID
LEFT JOIN data_treatmentoption recom
ON t_detectionrecord.RecommendedTreatID=recom.ID
LEFT JOIN data_treatmentoption chosen
ON t_detectionrecord.ChosenTreatID=chosen.ID
LEFT JOIN data_gender
ON t_detectionrecord.GenderID=data_gender.ID
LEFT JOIN t_user submit
ON t_detectionrecord.SubmitBy=submit.ID
LEFT JOIN t_user diagnotics
ON t_detectionrecord.DiagnoticsBy=diagnotics.ID
LEFT JOIN t_user report
ON t_detectionrecord.ReportBy=report.ID
LEFT JOIN data_detectionresulttype
ON t_detectionrecord.DiagnoticsTypeID=data_detectionresulttype.ID
WHERE t_detectionrecord.OrgnizationID=?p1
AND t_detectionrecord.IsDeleted=0
", orgid);
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
,CheckType
,PatientID
,OrgnizationID
,RecommendedTreatID
,ChosenTreatID
,IsReexam
,GenderID
,SubmitBy
,SubmitTime
,DetectionNO
,ClinicalNO
,Pics
,Pdf
,DiagnoticsTypeID
,DiagnoticsTime
,DiagnoticsBy
,ReportTime
,ReportBy
,Reference
,IFNULL(t_detectionrecord.IsActive,'') AS IsActive
FROM 
t_detectionrecord
WHERE ID=?p1
AND t_detectionrecord.IsDeleted=0", id);
            res["person"] = new PersonController(null, null)
                .GetPersonInfo(res["patientid"]?.ToObject<int>()??0);
            res["orgnization"] = new OrganizationController(null)
                .GetOrgInfo(res["orgnizationid"]?.ToObject<int>()??0);
            res["recommend"] = new TreatmentOptionController(null)
                .GetTreatOptionInfo(res["recommendedtreatid"]?.ToObject<int>() ?? 0);
            res["chosen"] = new TreatmentOptionController(null)
                .GetTreatOptionInfo(res["chosentreatid"]?.ToObject<int>() ?? 0);
            res["gender"] = new GenderController(null)
                .GetGenderInfo(res["genderid"]?.ToObject<int>() ?? 0);
            res["result"] = new DetectionResultTypeController(null)
                .GetResultTypeInfo(res["diagnoticstypeid"]?.ToObject<int>() ?? 0);
            res["items"] = GetCheckItems(res["id"]?.ToObject<int>() ?? 0);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 更改“检测”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetCheck")]
        public JObject SetCheck([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = req.ToInt("patientid");
            dict["OrgnizationID"] = req.ToInt("orgnizationid");
            dict["RecommendedTreatID"] = req.ToInt("recommendedtreatid");
            dict["ChosenTreatID"] = req.ToInt("chosentreatid");
            dict["IsReexam"] = req["isreexam"]?.ToObject<Boolean>();
            dict["GenderID"] = req.ToInt("genderid");
            dict["CheckType"] = req["checktype"]?.ToObject<string>();
            dict["DetectionNO"] = req["detectionno"]?.ToObject<string>();
            //dict["ClinicalNO"] = req["clinicalno"]?.ToObject<string>();
            //dict["DepartmentName"] = req["departmentname"]?.ToObject<string>();
            //dict["InpatientArea"] = req["inpatientarea"]?.ToObject<string>();
            //dict["SickbedNO"] = req["sickbedno"]?.ToObject<string>();
            //dict["SampleID"] = req["sampleid"]?.ToObject<string>();
            //dict["SampleType"] = req["sampletype"]?.ToObject<string>();
            //dict["SampleStatus"] = req["samplestatus"]?.ToObject<string>();
            dict["SubmitBy"] = req["submitby"]?.ToObject<string>();
            dict["SubmitTime"] = req.ToDateTime("submittime");
            dict["ObjectiveResult"] = req["objectiveresult"]?.ToObject<string>();
            dict["SubjectiveResult"] = req["subjectiveresult"]?.ToObject<string>();
            dict["Pics"] = req["pics"]?.ToObject<string>();
            dict["Pdf"] = req["pdf"]?.ToObject<string>();
            dict["DiagnoticsTypeID"] = req.ToInt("diagnoticstypeid");
            dict["DiagnoticsTime"] = req.ToDateTime("diagnoticstime");
            dict["DiagnoticsBy"] = req["diagnoticsby"]?.ToObject<string>();
            dict["ReportTime"] = req.ToDateTime("reporttime");
            dict["ReportBy"] = req["reportby"]?.ToObject<string>();
            dict["Reference"] = req["reference"]?.ToObject<string>();
            // TODO: ADD CheckItem HERE

            CheckProductController product = new CheckProductController(null);
            JObject pObject = new JObject();
            pObject["Name"] = req["productname"];
            pObject["BatchNumber"] = req["product"];
            pObject["Specification"] = req["specification"];
            product.Set(pObject);

            CheckItemController items = new CheckItemController(null);
            JObject itemObject = new JObject();
            //itemObject[""]

            JObject res = new JObject();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = StampUtil.Stamp(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_detectionrecord", dict, condi);
                res["id"] = req["id"];
            }
            else
            {
                dict["CreatedBy"] = StampUtil.Stamp(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                res["id"] = this.db.Insert("t_detectionrecord", dict);
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
            var count = db.Update("t_detectionrecord", dict, keys);
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


        [NonAction]
        public JArray GetCheckItems(int checkid)
        {
            JArray res= db.GetArray(@"
SELECT 
ID
,PatientID AS PersonID
,OrgnizationID AS OrgnizationID
,DetectionProductID
,DetectionResultTypeID
,Result
,ResultUnit
,ResultTime
,Sugguest
,ReferenceValue
,SubmitBy
,SubmitTime
,Injecter
,InjectTime
,Observer
,ObserveTime
,IFNULL(t_detectionrecorditem.IsActive,'') AS IsActive
FROM t_detectionrecorditem
WHERE DetectionRecordID=?p1
AND t_detectionrecorditem.IsDeleted=0", checkid);
            foreach (JToken item in res)
            {
                PersonController person = new PersonController(null, null);
                item["person"] = person
                    .GetPersonInfo(item["personid"]?.ToObject<int>() ?? 0);
                item["submit"] = person.GetUserInfo(item["submitby"]?.ToObject<int>() ?? 0);
                item["inject"] = person.GetUserInfo(item["injecter"]?.ToObject<int>() ?? 0);
                item["observe"] = person.GetUserInfo(item["observer"]?.ToObject<int>() ?? 0);

                item["orgnization"] = new OrganizationController(null)
                    .GetOrgInfo(item["orgnizationid"]?.ToObject<int>() ?? 0);
                item["product"] = new CheckProductController(null)
                    .GetCheckProductInfo(item["detectionproductid"]?.ToObject<int>() ?? 0);
                item["cresult"] = new DetectionResultTypeController(null)
                    .GetResultTypeInfo(item["detectionresulttypeid"]?.ToObject<int>() ?? 0);
                //item["tester"]
            }

            return res;
        }


        
        /// <summary>
        /// 上传指定“检查结果”对应的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="files"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("Upload[controller]Pics/{checkid:int}")]
        public JObject UploadFile(
         int checkid,
         IFormFile[] files,
         CancellationToken cancellationToken)
        {
            JObject res = new JObject();
            config conf = new config();
            string uploadir = conf.GetValue("person:upload");
            int countlimit = int.Parse(conf.GetValue("person:filecount"));
            if (files.Length > countlimit)
            {
                res["status"] = 201;
                res["msg"] = "最多允许上传 " + countlimit + " 个文件";
                return res;
            }

            long sizelimit = long.Parse(conf.GetValue("person:filesize"));
            if (files
                .Where(f => f.Length == 0 || f.Length > sizelimit)
                .FirstOrDefault() != null)
            {
                res["status"] = 201;
                res["msg"] = "文件大小介于0，" + sizelimit;
                return res;
            }

            JObject check = db.GetOne(@"SELECT ID,ReportTime,Pics,IsArchived FROM t_detectionrecord WHERE ID=?p1 AND IsDeleted=0", checkid);
            if (check["id"] == null || (check["isarchived"]?.ToObject<bool>() ?? false))
            {
                res["status"] = 201;
                res["msg"] = "无法上传";
                return res;
            }


            if (!Directory.Exists(uploadir))
                Directory.CreateDirectory(uploadir);

            JObject picsJObject = new JObject();
            StringBuilder bPics = new StringBuilder();
            MemoryStream[] msArray = new MemoryStream[files?.Length??0];
            for (int checkIndex = 0; checkIndex < files?.Length && !cancellationToken.IsCancellationRequested; checkIndex++)
            {
                using (msArray[checkIndex] = new MemoryStream())
                {
                    files[checkIndex].CopyTo(msArray[checkIndex]);
                    if (!FileHelpers.IsValidFileExtensionAndSignature(files[checkIndex].FileName, msArray[checkIndex], permittedExtensions))
                    {
                        res["status"] = 201;
                        res["msg"] = files[checkIndex].FileName + " 不能上传";
                        return res;
                    }
                }
            }

            for (int actionIndex = 0; actionIndex < files?.Length && !cancellationToken.IsCancellationRequested; actionIndex++)
            {
                string filepath = Path.Combine(uploadir, Path.GetRandomFileName() + Path.GetExtension(files[actionIndex].FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(uploadir, Path.GetRandomFileName() + Path.GetExtension(files[actionIndex].FileName));

                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    files[actionIndex].CopyTo(fileStream);
                }
                    

                picsJObject[actionIndex.ToString()] = Path.GetFullPath(filepath);
            }



            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Pics"] = picsJObject.ToString();
            dict["LastUpdatedBy"] = StampUtil.Stamp(this.HttpContext);
            dict["LastUpdatedTime"] = DateTime.Now;
            Dictionary<string, object> keys = new Dictionary<string, object>();
            keys["id"] = checkid;

            int row = db.Update("t_detectionrecord", dict, keys);

            if (row > 0)
                foreach (var oldfile in check["pics"]?.ToObject<string>()?.Split(spliter, StringSplitOptions.RemoveEmptyEntries))
                    if (System.IO.File.Exists(oldfile))
                        System.IO.File.Delete(oldfile);

            res = GetFileList(checkid);
            res["msg"] = "上传成功";
            return res;
        }

        /// <summary>
        /// 获取指定“检查编号”对应的图片
        /// </summary>
        /// <param name="checkid"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [HttpGet("Get[controller]Pic/{checkid:int}/{index:int}")]
        public IActionResult GetFile(int checkid, int index)
        {
            JObject check = db.GetOne(@"SELECT ReportTime,Pics FROM t_detectionrecord WHERE ID=?p1 AND IsDeleted=0", checkid);
            JObject res = new JObject();
            var pics = JsonConvert.DeserializeObject<Dictionary<string, string>>(check["pics"]?.ToObject<string>());
            if (pics?.Keys == null)
                return NoContent();

            string filepath = pics?[index.ToString()];
            if (string.IsNullOrEmpty(filepath))
                return NoContent();

            string mimeType = FileHelpers.mimetype[Path.GetExtension(filepath)];
            var stream = new FileStream(filepath, FileMode.Open);

            StringBuilder bFileDownloadName = new StringBuilder();
            bFileDownloadName.Append(check["reporttime"]?.ToObject<DateTime>().ToString("yyyymmdd"));
            bFileDownloadName.Append(".");
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
        [Route("Get[controller]Pics/{checkid:int}")]
        public JObject GetFileList(int checkid)
        {
            JObject tmp = db.GetOne(@"SELECT Pics FROM t_detectionrecord WHERE ID=?p1 AND IsDeleted=0", checkid);
            var pics = JsonConvert.DeserializeObject<Dictionary<string,string>>(tmp["pics"]?.ToObject<string>());
            JArray array = new JArray();
            foreach (var key in  pics?.Keys)
            {
                var tmpl = ControllerContext.ActionDescriptor.AttributeRouteInfo.Template;
                tmpl = tmpl.Replace("{checkid:int}", "{0}/{1}");
                tmpl = tmpl.Replace("Upload","Get");
                var url = string.Format(tmpl,checkid,key);
                array.Add(url);
            }
            JObject res = new JObject();
            res["list"] = array;
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }
    }
}