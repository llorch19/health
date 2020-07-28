﻿/*
 * Title : “未读消息”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - ReadMessage 只针对个人用户    @xuedi      2020-07-23      10:50
 */
using health.common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.Controllers
{
    [Route("api")]
    public class ReadNoticeController : ControllerBase
    {
        private readonly ILogger<ReadNoticeController> _logger;
        dbfactory db = new dbfactory();
        public ReadNoticeController(ILogger<ReadNoticeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取“未读通知”列表，右上角入口
        /// </summary>
        /// <returns>JSON对象，包含相应的“未读消息”数组</returns>
        [HttpGet]
        [Route("Get[controller]List")]
        public JObject GetList()
        {
            var userid = HttpContext.GetUserInfo<int>("id");
            JObject res = new JObject();
            JArray list = db.GetArray(@"
-- Get Unread notice By User Id
SELECT 
IFNULL(t_notice.ID,'') AS ID
,IFNULL(t_notice.OrgnizationID,'') as OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName 
,IFNULL(t_user.ID,'') AS PublishUserID
,IFNULL(t_user.FamilyName,'') AS Publish 
,IFNULL(PublishTime,'') AS PublishTime
,IFNULL(t_notice.Title,'') AS Title
,IFNULL(Content,'') AS Content
,IFNULL(Attachment,'') AS Attachment
,IFNULL(t_notice.IsActive,'') AS IsActive
FROM t_notice
LEFT JOIN t_user
ON t_user.ID=t_notice.PublishUserID
LEFT JOIN t_orgnization
ON t_orgnization.ID=t_notice.OrgnizationID
WHERE t_notice.ID NOT IN(
SELECT NoticeID AS ID FROM t_noticeread
WHERE UserID=?p1)
AND t_notice.IsDeleted = 0
",userid);
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
        /// <param name="notid">指定消息的id</param>
        /// <param name="userid">指定用户的id</param>
        /// <returns>JSON对象，包含相应的“未读消息”</returns>
        [HttpGet]
        [Route("Get[controller]")]
        public JObject Get(int notid, int userid)
        {
            JObject res = db.GetOne(@"
select 
IFNULL(ID,'') AS ID 
from t_notice where id=?p1
", notid);
            if (res["id"] == null)
            {
                res["status"] = 201;
                res["msg"] = "没有获取到相应的数据";
                return res;
            }

            res = new JObject();
            // 自动插入已读记录
            JObject read = db.GetOne(@"select ID
from t_noticeread 
where noticeid=?p1 
and userid=?p2
", notid, userid);
            if (read["id"] == null)
            {
                // 未打开过消息则自动插入
                var newRead = new Dictionary<string, object>();
                newRead["NoticeID"] = notid;
                newRead["UserID"] = userid;
                newRead["OpenTime"] = DateTime.Now;
                newRead["CreatedBy"] = userid;
                newRead["CreatedTime"] = DateTime.Now;
                db.Insert("t_noticeread", newRead);
            }

            res = db.GetOne(@"
SELECT 
IFNULL(ID,'') AS ID
,IFNULL(NoticeID,'') AS NoticeID
,IFNULL(UserID,'') AS UserID
,IFNULL(OpenTime,'') AS OpenTime
,IFNULL(FinishTime,'') AS FinishTime
,IFNULL(IsRead,'') AS IsRead 
,IFNULL(IsActive,'') AS IsActive 
FROM t_noticeread
where NoticeID=?p1 and UserID=?p2
and isdeleted=0
", notid, userid);


            PersonController user = new PersonController(null, null);
            res["user"] = user.GetPersonInfo(res["userid"]?.ToObject<int>()??0);

            NoticeController notice = new NoticeController(null, null);
            res["notice"] = notice.Get(notid);

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
            dict["noticeid"] = req["noticeid"]?.ToObject<int>();
            dict["userid"] = req["userid"]?.ToObject<int>();
            dict["FinishTime"] = DateTime.Now;
            dict["IsRead"] = req["isread"]?.ToObject<int>();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> condi = new Dictionary<string, object>();
                condi["id"] = req["id"];
                dict["LastUpdatedBy"] = StampUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_messageread", dict, condi);
            }
            else
            {
                dict["CreatedBy"] = StampUtil.GetUser(HttpContext);
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
            dict["LastUpdatedBy"] = StampUtil.GetUser(HttpContext);
            dict["LastUpdatedTime"] = DateTime.Now;
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