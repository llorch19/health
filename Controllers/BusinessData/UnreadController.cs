﻿/*
 * Title : “未读消息”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class UnreadController : ControllerBase
    {
        private readonly ILogger<UnreadController> _logger;
        public UnreadController(ILogger<UnreadController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“未读消息”列表，[科普公告]菜单
        /// </summary>
        /// <returns>JSON对象，包含相应的“未读消息”数组</returns>
        [HttpGet]
        [Route("GetUnreadList")]
        public JObject GetUnreadList(int userid)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            JArray list = db.GetArray(@"
-- Get Unread Messages By User Id
SELECT 
IFNULL(t_messagesent.ID,'') AS ID
,IFNULL(t_user.ID,'') AS PublishUserID
,IFNULL(t_user.FamilyName,'') AS Publish 
,IFNULL(t_messagesent.Title,'') AS Title
,IFNULL(Content,'') AS Content
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(OutdateTime,'') AS OutdateTime
,IFNULL(IsCancel,'') AS IsCancel
,IFNULL(IsClose,'') AS IsClose
,IFNULL(ReaderType,'') AS ReaderType
,IFNULL(t_messagesent.Description,'') AS Description 
FROM t_messagesent
LEFT JOIN t_user
ON t_user.ID=t_messagesent.PublishUserID
WHERE t_messagesent.ID NOT IN(
SELECT MessageID AS ID FROM t_messageread
WHERE UserID=0)
AND IsCancel=0
AND IsClose=0
AND OutdateTime > NOW()
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
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“未读消息”</returns>
        [HttpGet]
        [Route("GetUnread")]
        public JObject GetUnread(int id)
        {
            dbfactory db = new dbfactory();
            JObject res =  db.GetOne(@"select
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
            if (res["id"]!=null)
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
        [Route("SetUnread")]
        public JObject SetUnread([FromBody] JObject req)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            if (req["id"] != null)
            {
                int id = req["id"].ToObject<int>();
                if (id == 0)
                {
                    req.Remove("publish");
                    req["OrgnizationID"] = null;
                    var dict = req.ToObject<Dictionary<string, object>>();
                    var rows = db.Insert("t_messagesent", dict);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "新增成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "无法新增数据";
                    }
                }
                else if (id > 0)
                {
                    var dict = req.ToObject<Dictionary<string, object>>();
                    dict.Remove("id");
                    var keys = new Dictionary<string, object>();
                    keys["id"] = req["id"];
                    var rows = db.Update("t_messagesent", dict, keys);
                    if (rows > 0)
                    {
                        res["status"] = 200;
                        res["msg"] = "修改成功";
                    }
                    else
                    {
                        res["status"] = 201;
                        res["msg"] = "修改失败";
                    }
                }
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "非法的请求";
            }
            return res;
        }




        /// <summary>
        /// 删除“公告”。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“公告”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("DelUnread")]
        public JObject DelUnread([FromBody] JObject req)
        {
            JObject res = new JObject();
            var dict = req.ToObject<Dictionary<string, object>>();
            dbfactory db = new dbfactory();
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