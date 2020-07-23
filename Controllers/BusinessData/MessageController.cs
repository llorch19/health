/*
 * Title : “公告”控制器
 * Author: zudan
 * Date  : 2020-07-14
 * Description: 对个人信息的增删查改
 * Comments
 * - 发布时间由服务器定，过期时间去掉，公告只针对Patient不针对User       @xuedi      2020-07-23  10:50
 */
using health.Controllers.BaseData;
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
            string sql = @"
SELECT
IFNULL(t_messagesent.ID,'') as ID
,IFNULL(t_messagesent.OrgnizationID,'') as OrgnizationID
,IFNULL(t_orgnization.OrgName,'') AS OrgName 
,IFNULL(t_user.ID,'') as PublishUserID
,IFNULL(t_user.FamilyName,'') as Publish 
,IFNULL(t_messagesent.PublishTime,'') as PublishTime
,IFNULL(t_messagesent.Title,'') as Title
,IFNULL(t_messagesent.Abstract,'') as Abstract
,IFNULL(t_messagesent.Thumbnail,'') as Thumbnail
,IFNULL(Content,'') as Content
FROM t_messagesent 
LEFT JOIN t_user
ON t_user.ID=t_messagesent.PublishUserID
LEFT JOIN t_orgnization
ON t_messagesent.OrgnizationID=t_orgnization.ID
WHERE 1 = 1
";
            if (!IsOrgUser())
            {
                sql += @"
AND t_messagesent.IsPublic = 1
";
            }
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
        /// 获取“公告”信息，点击[科普公告]中的一个栏目
        /// </summary>
        /// <param name="id">指定的id</param>
        /// <returns>JSON对象，包含相应的“公告”信息</returns>
        [HttpGet]
        [Route("GetMessage")]
        public JObject GetMessage(int id)
        {
            string sql = @"SELECT
IFNULL(ID,'') AS ID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,IFNULL(PublishUserID,'') AS PublishUserID
,IFNULL(PublishTime,'') as PublishTime
,IFNULL(Title,'') AS Title
,IFNULL(Abstract,'') AS Abstract
,IFNULL(Thumbnail,'') AS Thumbnail
,IFNULL(Content,'') AS Content
FROM t_messagesent 
WHERE t_messagesent.ID=?p1";

            if (!IsOrgUser())
                sql += @"
AND IsPublic=1
";

            JObject res = db.GetOne(sql,id);
            if (res["id"] != null)
            {
                OrgnizationController org = new OrgnizationController(null);
                res["orgnization"] = org.GetOrgInfo(res["orgnizationid"]?.ToObject<int>()??0);
                PersonController person = new PersonController(null,null);
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
        /// 更改“公告”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“公告”信息</param>
        /// <returns>响应状态信息</returns>
        [HttpPost]
        [Route("SetMessage")]
        public JObject SetMessage([FromBody] JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["OrgnizationID"] = 1;
            dict["PublishUserID"] = 4;
            dict["Title"] = req["title"]?.ToObject<string>();
            dict["Abstract"] = req["abstract"]?.ToObject<string>();
            dict["Thumbnail"] = req["thumbnail"]?.ToObject<string>();
            dict["Content"] = req["content"]?.ToObject<string>();
            dict["IsPublic"] = req["ispublic"]?.ToObject<string>();

            if (req["id"]?.ToObject<int>() > 0)
            {
                Dictionary<string, object> keys = new Dictionary<string, object>();
                keys["id"] = req["id"];

                dict["LastUpdatedBy"] = FilterUtil.GetUser(HttpContext);
                dict["LastUpdatedTime"] = DateTime.Now;
                var tmp = this.db.Update("t_messagesent", dict, keys);
            }
            else
            {
                dict["PublishTime"] = DateTime.Now;
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

        /// <summary>
        /// 只有机构用户可以查看并编辑未发布的公告
        /// </summary>
        /// <returns></returns>
        private bool IsOrgUser()
        {
            return true;
        }
    }
}