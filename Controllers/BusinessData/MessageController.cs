/*
 * Title : 个人信息管理控制器
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
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取公告列表，[科普公告]菜单
        /// </summary>
        /// <returns>JSON数组形式的公告信息</returns>
        [HttpGet]
        [Route("GetMessageList")]
        public JObject GetMessageList(int pageIndex)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            JArray list = db.GetArray(@"select
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
LIMIT ?p1,10
",
            pageIndex);
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
        /// 获取公告信息，点击[科普公告]中的一个栏目
        /// </summary>
        /// <returns>JSON数组形式的公告信息</returns>
        [HttpGet]
        [Route("GetMessage")]
        public JObject GetMessage(int id)
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
        /// 更改公告信息。如果id=0新增公告，如果id>0修改公告。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的公告信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetMessage")]
        public JObject SetMessage([FromBody] JObject req)
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
        /// 删除公告。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的公告信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelMessage")]
        public JObject DelMessage([FromBody] JObject req)
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