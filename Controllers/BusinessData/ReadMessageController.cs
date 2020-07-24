/*
 * Title : “未读消息”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - ReadMessage 只针对个人用户    @xuedi      2020-07-23      10:50
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class ReadMessageController : ControllerBase
    {
        private readonly ILogger<ReadMessageController> _logger;
        dbfactory db = new dbfactory();
        public ReadMessageController(ILogger<ReadMessageController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“未读消息”列表，[科普公告]菜单
        /// </summary>
        /// <returns>JSON对象，包含相应的“未读消息”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public JObject GetList(int userid)
        {
            JObject res = new JObject();
            JArray list = db.GetArray(@"
-- Get Unread Messages By User Id
SELECT 
IFNULL(t_messagesent.ID,'') AS ID
,IFNULL(t_messagesent.OrgnizationID,'') as OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName 
,IFNULL(t_user.ID,'') AS PublishUserID
,IFNULL(t_user.FamilyName,'') AS Publish 
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(t_messagesent.Title,'') AS Title
,IFNULL(t_messagesent.Thumbnail,'') AS Thumbnail
,IFNULL(t_messagesent.Abstract,'') AS Abstract
,IFNULL(Content,'') AS Content
FROM t_messagesent
LEFT JOIN t_user
ON t_user.ID=t_messagesent.PublishUserID
LEFT JOIN t_orgnization
ON t_orgnization.ID=t_messagesent.OrgnizationID
WHERE t_messagesent.ID NOT IN(
SELECT MessageID AS ID FROM t_messageread
WHERE PatientID=1)
AND t_messagesent.IsPublic = 1
");
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
        /// 打开“未读消息”信息，点击[科普公告]中的一个“未读消息”
        /// </summary>
        /// <param name="msgid">指定消息的id</param>
        /// <param name="patientid">指定用户的id</param>
        /// <returns>JSON对象，包含相应的“未读消息”</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public JObject Get(int msgid, int patientid)
        {
            JObject res = db.GetOne(@"
select 
IFNULL(ID,'') AS ID 
from t_messagesent where id=?p1
", msgid);
            if (res["id"] == null)
            {
                res["status"] = 201;
                res["msg"] = "没有获取到相应的数据";
                return res;
            }

            res = new JObject();
            // 自动插入已读记录
            JObject read = db.GetOne(@"select ID
from t_messageread 
where messageid=?p1 
and patientid=?p2", msgid, patientid);
            if (read["id"] == null)
            {
                // 未打开过消息则自动插入
                var newRead = new Dictionary<string, object>();
                newRead["MessageID"] = msgid;
                newRead["PatientID"] = patientid;
                newRead["OpenTime"] = DateTime.Now;
                newRead["CreatedBy"] = patientid;
                newRead["CreatedTime"] = DateTime.Now;
                db.Insert("t_messageread", newRead);
            }

            res = db.GetOne(@"SELECT 
IFNULL(ID,'') AS ID
,IFNULL(MessageID,'') AS MessageID
,IFNULL(PatientID,'') AS PersonID
,IFNULL(OpenTime,'') AS OpenTime
,IFNULL(FinishTime,'') AS FinishTime
,IFNULL(IsRead,'') AS IsRead 
FROM t_messageread
where MessageID=?p1 and PatientID=?p2", msgid, patientid);


            PersonController person = new PersonController(null, null);
            res["person"] = person.GetPersonInfo(res["personid"]?.ToObject<int>()??0);

            MessageController msg = new MessageController(null, null);
            res["message"] = msg.Get(msgid);

            res["status"] = 200;
            res["msg"] = "获取数据成功";
            return res;
        }


        /// <summary>
        /// 更改“未读消息”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“未读消息”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Set[controller]")]
        public JObject Set([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["MessageID"] = req["messageid"]?.ToObject<int>();
            dict["PatientID"] = req["patientid"]?.ToObject<int>();
            dict["OpenTime"] = req["opentime"]?.ToObject<DateTime>();
            dict["FinishTime"] = req["finishtime"]?.ToObject<DateTime>();
            dict["IsRead"] = req["isread"]?.ToObject<int>();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_messageread", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("t_messageread", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }




        /// <summary>
        /// 删除“未读消息”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“未读消息”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("Del[controller]")]
        public JObject Del([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = new Dictionary<string, object>();
            dict["IsDeleted"] = 1;
            var keys = new Dictionary<string, object>();
            keys["id"] = req["id"]?.ToObject<int>();
            var count = db.Update("t_messageread", dict, keys);
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