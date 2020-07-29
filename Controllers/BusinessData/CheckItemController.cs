/*
 * Title : “检测”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对“检测”信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using Renci.SshNet.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class CheckItemController : ControllerBase
    {
        private readonly ILogger<CheckItemController> _logger;
        dbfactory db = new dbfactory();
        public CheckItemController(ILogger<CheckItemController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取机构的“检测”列表
        /// </summary>
        /// <param name="orgid">检索指定机构的id</param>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [NonAction]
        public JArray GetOrgCheckItemList(int orgid)
        {
            return db.GetArray(@"
SELECT 
t_detectionrecorditem.ID
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonIDCard
,t_detectionrecorditem.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,DetectionProductID
,t_detectionproduct.`Name` AS ProductName
,t_detectionproduct.ESC
,t_detectionproduct.Manufacturer
,DetectionResultTypeID
,data_detectionresulttype.ResultName 
,Result
,ResultUnit
,ResultTime
,Sugguest
,ReferenceValue
,SubmitBy
,submit.ChineseName AS submit
,SubmitTime
,Injecter
,inject.ChineseName AS inject
,InjectTime
,Observer
,observe.ChineseName AS observe
,ObserveTime
,ReferenceDescription
,t_detectionrecorditem.IsActive AS IsActive
FROM 
t_detectionrecorditem
LEFT JOIN t_patient
ON t_detectionrecorditem.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecorditem.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_detectionrecorditem.DetectionResultTypeID=data_detectionresulttype.ID
LEFT JOIN t_detectionproduct
ON t_detectionrecorditem.DetectionProductID=t_detectionproduct.ID
LEFT JOIN t_user submit
ON t_detectionrecorditem.SubmitBy=submit.ID
LEFT JOIN t_user inject
ON t_detectionrecorditem.Injecter=inject.ID
LEFT JOIN t_user observe
ON t_detectionrecorditem.Observer=observe.ID
WHERE t_detectionrecorditem.OrgnizationID=?p1
AND t_detectionrecorditem.IsDeleted=0
", orgid);
        }

        /// <summary>
        /// 获取个人的“检测”历史
        /// </summary>
        /// <param name="personid">检索指定个人的id</param>
        /// <returns>JSON对象，包含相应的“检测”数组</returns>
        [NonAction]
        public JArray GetPersonCheckItemList(int personid)
        {
            return db.GetArray(@"
SELECT 
t_detectionrecorditem.ID
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonIDCard
,t_detectionrecorditem.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,DetectionProductID
,t_detectionproduct.`Name` AS ProductName
,t_detectionproduct.ESC
,t_detectionproduct.Manufacturer
,DetectionResultTypeID
,data_detectionresulttype.ResultName 
,Result
,ResultUnit
,ResultTime
,Sugguest
,ReferenceValue
,SubmitBy
,submit.ChineseName AS submit
,SubmitTime
,Injecter
,inject.ChineseName AS inject
,InjectTime
,Observer
,observe.ChineseName AS observe
,ObserveTime
,ReferenceDescription
,t_detectionrecorditem.IsActive AS IsActive
FROM 
t_detectionrecorditem
LEFT JOIN t_patient
ON t_detectionrecorditem.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecorditem.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_detectionrecorditem.DetectionResultTypeID=data_detectionresulttype.ID
LEFT JOIN t_detectionproduct
ON t_detectionrecorditem.DetectionProductID=t_detectionproduct.ID
LEFT JOIN t_user submit
ON t_detectionrecorditem.SubmitBy=submit.ID
LEFT JOIN t_user inject
ON t_detectionrecorditem.Injecter=inject.ID
LEFT JOIN t_user observe
ON t_detectionrecorditem.Observer=observe.ID
WHERE t_detectionrecorditem.PatientID=?p1
AND t_detectionrecorditem.IsDeleted=0
", personid);
        }


        [NonAction]
        public JArray GetCheckItemList(int checkid)
        {
            return db.GetArray(@"
SELECT 
t_detectionrecorditem.ID
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_patient.IDCardNO AS PersonIDCard
,t_detectionrecorditem.OrgnizationID
,t_orgnization.OrgName
,t_orgnization.OrgCode
,DetectionProductID
,t_detectionproduct.`Name` AS ProductName
,t_detectionproduct.ESC
,t_detectionproduct.Manufacturer
,DetectionResultTypeID
,data_detectionresulttype.ResultName 
,Result
,ResultUnit
,ResultTime
,Sugguest
,ReferenceValue
,SubmitBy
,submit.ChineseName AS submit
,SubmitTime
,Injecter
,inject.ChineseName AS inject
,InjectTime
,Observer
,observe.ChineseName AS observe
,ObserveTime
,ReferenceDescription
,t_detectionrecorditem.IsActive AS IsActive
FROM 
t_detectionrecorditem
LEFT JOIN t_patient
ON t_detectionrecorditem.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecorditem.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_detectionrecorditem.DetectionResultTypeID=data_detectionresulttype.ID
LEFT JOIN t_detectionproduct
ON t_detectionrecorditem.DetectionProductID=t_detectionproduct.ID
LEFT JOIN t_user submit
ON t_detectionrecorditem.SubmitBy=submit.ID
LEFT JOIN t_user inject
ON t_detectionrecorditem.Injecter=inject.ID
LEFT JOIN t_user observe
ON t_detectionrecorditem.Observer=observe.ID
WHERE t_detectionrecorditem.DetectionRecordID=?p1
AND ,t_detectionrecorditem.IsDeleted=0
", checkid);
        }


        /// <summary>
        /// 获取“检测”信息
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“检测”信息</returns>
        [NonAction]
        public JObject GetCheckItem(int id)
        {
            JObject res= db.GetOne(@"
SELECT 
t_detectionrecorditem.ID
,PatientID
,t_detectionrecorditem.OrgnizationID
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
,ReferenceDescription
,t_detectionrecorditem.IsActive AS IsActive
FROM 
t_detectionrecorditem
LEFT JOIN t_patient
ON t_detectionrecorditem.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_detectionrecorditem.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_detectionrecorditem.DetectionResultTypeID=data_detectionresulttype.ID
WHERE ID=?p1
AND t_detectionrecorditem.IsDeleted=0
", id);
            PersonController person = new PersonController(null,null);
            res["person"] = person.GetPersonInfo(res["patientid"].ToObject<int>());
            res["submit"] = person.GetUserInfo(res["submitby"].ToObject<int>());
            res["inject"] = person.GetUserInfo(res["injecter"].ToObject<int>());
            res["observe"] = person.GetUserInfo(res["observer"].ToObject<int>());
            OrganizationController org = new OrganizationController(null);
            res["orgnization"] = org.GetOrgInfo(res["orgnizationid"].ToObject<int>());
            CheckProductController product = new CheckProductController(null);
            res["product"] = product.GetCheckProductInfo(res["detectionproductid"].ToObject<int>());
            DetectionResultTypeController type = new DetectionResultTypeController(null);
            res["rtype"] = type.GetResultTypeInfo(res["detectionresulttypeid"].ToObject<int>());
            return res;
        }


        /// <summary>
        /// 更改“检测”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="items">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [NonAction]
        public int[] SetCheckItem([FromBody] JObject[] items)
        {
            int[] res = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict["PatientID"] = items[i]["patientid"]?.ToObject<int>();
                dict["OrgnizationID"] = items[i]["orgnizationid"]?.ToObject<int>();
                dict["DetectionProductID"] = items[i]["detectionproductid"]?.ToObject<int>();
                dict["DetectionRecordID"] = items[i]["detectionrecordid"]?.ToObject<int>();
                dict["DetectionResultTypeID"] = items[i]["detectionresulttypeid"]?.ToObject<int>();
                dict["Result"] = items[i]["result"]?.ToObject<int>();
                dict["ResultUnit"] = items[i]["resultunit"]?.ToObject<int>();
                dict["ResultTime"] = items[i]["resulttime"]?.ToObject<int>();
                dict["Sugguest"] = items[i]["sugguest"]?.ToObject<int>();
                dict["ReferenceValue"] = items[i]["referencevalue"]?.ToObject<int>();
                dict["ReferenceDescription"] = items[i]["referencedescription"]?.ToObject<int>();
                dict["SubmitBy"] = items[i]["submitby"]?.ToObject<int>();
                dict["SubmitTime"] = items[i]["submittime"]?.ToObject<int>();
                dict["Injecter"] = items[i]["injecter"]?.ToObject<int>();
                dict["InjectTime"] = items[i]["injecttime"]?.ToObject<int>();
                dict["Observer"] = items[i]["observer"]?.ToObject<int>();
                dict["ObserveTime"] = items[i]["observetime"]?.ToObject<int>();

                if (items[i]["id"]?.ToObject<int>()>0)
                {
                    Dictionary<string, object> keys = new Dictionary<string, object>();
                    keys["id"] = items[i]["id"].ToObject<int>();
                    res[i] = db.Update("t_detectionrecorditem", dict, keys);
                }
                else
                {
                    res[i] = db.Insert("t_detectionrecorditem", dict);
                }
            }

            return res;
        }




        /// <summary>
        /// 删除“检测”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“检测”信息</param>
        /// <returns>响应状态信息</returns>
        [NonAction]
        public JObject DelCheckItem([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = new Dictionary<string, object>();
            dict["IsDeleted"] = 1;
            var keys = new Dictionary<string, object>();
            keys["id"] = req.ToInt("id");
            var count = db.Update("t_detectionrecorditem", dict, keys);
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
    }
}