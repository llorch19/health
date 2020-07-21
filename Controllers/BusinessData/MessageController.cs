/*
 * Title : “公告”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        dbfactory db = new dbfactory();
        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“公告”列表，[科普公告]菜单
        /// </summary>
        /// <returns>JSON对象，包含相应的公告数组</returns>
        [HttpGet]
        [Route("GetMessageList")]
        public JObject GetMessageList()
        {
            JObject res = new JObject();
            JArray list = db.GetArray(@"SELECT
IFNULL(t_messagesent.ID,'') as ID
,IFNULL(t_user.ID,'') as PublishUserID
,IFNULL(t_user.FamilyName,'') as Publish 
,IFNULL(t_messagesent.Title,'') as Title
,IFNULL(Content,'') as Content
,IFNULL(PublishTime,'') as PublishTime
,IFNULL(OutdateTime,'') as OutdateTime
,IFNULL(IsCancel,'') as IsCancel
,IFNULL(IsClose,'') as IsClose
,IFNULL(ReaderType,'') as ReaderType
,IFNULL(t_messagesent.Description,'') as Description
FROM t_messagesent 
LEFT JOIN t_user
ON t_user.ID=t_messagesent.PublishUserID
WHERE OutdateTime > NOW()
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
        /// 获取“公告”信息，点击[科普公告]中的一个栏目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“公告”信息</returns>
        [HttpGet]
        [Route("GetMessage")]
        public JObject GetMessage(int id)
        {
            JObject res = db.GetOne(@"select
IFNULL(t_messagesent.ID,'') as ID
,IFNULL(t_user.ID,'') as PublishUserID
,IFNULL(t_user.FamilyName,'') as Publish 
,IFNULL(t_messagesent.Title,'') as Title
,IFNULL(Content,'') as Content
,IFNULL(PublishTime,'') as PublishTime
,IFNULL(OutdateTime,'') as OutdateTime
,IFNULL(IsCancel,'') as IsCancel
,IFNULL(IsClose,'') as IsClose
,IFNULL(ReaderType,'') as ReaderType
,IFNULL(t_messagesent.Description,'') as Description
from t_messagesent 
LEFT JOIN t_user
ON t_user.ID=t_messagesent.PublishUserID
WHERE t_messagesent.ID=?p1
"
                    , id);
            if (res["id"] != null)
            {
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
        /// 更改“公告”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“公告”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetMessage")]
        public JObject SetMessage([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = req["orgnizationid"]?.ToObject<int>();
            dict["PublishUserID"] = req["publishuserid"]?.ToObject<int>();
            dict["Title"] = req["title"]?.ToObject<string>();
            dict["Content"] = req["content"]?.ToObject<string>();
            dict["Attachment"] = req["attachment"]?.ToObject<string>();
            dict["PublishTime"] = req["publishtime"]?.ToObject<DateTime>();
            dict["OutdateTime"] = req["outdatetime"]?.ToObject<DateTime>();
            dict["IsCancel"] = req["iscancel"]?.ToObject<int>();
            dict["IsClose"] = req["isclose"]?.ToObject<int>();
            dict["ReaderType"] = req["readertype"]?.ToObject<string>();


            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_messagesent", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["CreatedTime"] = DateTime.Now;
                this.db.Insert("t_messagesent", dict);
            }

            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "提交成功";
            res["id"] = req["id"];
            return res;
        }




        /// <summary>
        /// 删除“公告”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“公告”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelMessage")]
        public JObject DelMessage([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            var count = db.del("t_messagesent", dict);
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