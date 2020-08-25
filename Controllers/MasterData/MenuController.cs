/*
 * Title : “菜单”控制器
 * Author: zudan
 * Date  : 2020-07-13
 * Description: 获取菜单，需要在中间件判断用户组并加以过滤
 * Comments
 * - Get返回值应该**递归**包含：菜单信息 @xuedi 2020-07-10 17:40
 * sample
 * Mock.mock('/menu',{
    'code': 200,
    'list': [{
        'id': 1,
        'name': 'analysis',
        'icon': 'dashboard',
        'label': '统计分析',
        'children': [],
    },{
        'id': 2,
        'name': 'pulish',
        'icon': 'container',
        'label': '科普公告',
        'children': [],
    },{
        'id': 3,
        'name': 'persion',
        'icon': 'solution',
        'label': '人员信息管理',
        'children': [
            {
                'id': 4,
                'name': 'add',
                'label': '人员信息录入',
                'pid': 3,
            },{
                'id': 5,
                'name': 'search',
                'label': '人员信息查询',
                'pid': 3,
            },{
                'id': 6,
                'name': 'transfer',
                'label': '人员转诊',
                'pid': 3,
            },
        ],
    },{
        'id': 7,
        'name': 'company',
        'icon': 'home',
        'label': '管理我的单位',
        'pid': 0,
        'children': [
            {
                'id': 8,
                'name': 'search',
                'label': '单位查询',
                'pid': 7,
            },{
                'id': 9,
                'name': 'add',
                'label': '单位新增',
                'pid': 7,
            },
        ],
    },{
        'id': 10,
        'name': 'setting',
        'icon': 'setting',
        'label': '系统设置',
        'children': [],
    },{
        'id': 11,
        'name': 'notification',
        'icon': 'notification',
        'label': '通知',
        'children': [],
    }, {'id': 10,
        'name': 'argsetting',
        'icon': 'tool',
        'label': '参数设置',
        'children': [
            {
                'id': 11,
                'name': 'AddressCategory',
                'label': '地址类型',
                'pid': 10,
            },{
                'id': 12,
                'name': 'Area',
                'label': '区域类型',
                'pid': 10,
            },{
                'id': 13,
                'name': 'DetectionResultType',
                'label': '感染类型',
                'pid': 10,
            },
            {
                'id': 14,
                'name': 'Gender',
                'label': '性别类型',
                'pid': 10,
            },
            {
                'id': 15,
                'name': 'IdCategory',
                'label': '证件类型',
                'pid': 10,
            },
            {
                'id': 16,
                'name': 'MedicationDosageForm',
                'label': '药物剂量',
                'pid': 10,
            },
            {
                'id': 17,
                'name': 'MedicationFreqCategory',
                'label': '药物频率',
                'pid': 10,
            },
            {
                'id': 18,
                'name': 'MedicationPathway',
                'label': '药物途径',
                'pid': 10,
            },
            {
                'id': 19,
                'name': 'Menu',
                'label': '菜单',
                'pid': 10,
             }
         ]
         }
            ]
})
 * - 代码风格方面，单行的if和循环语句要删除前后大括号 @norway  2020-07-13 17:31
 * - 给菜单项添加一个seq字段，方便前端操作           @xuedi   2020-07-14 11:50
 */
using health.common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace health.Controllers
{
    [Route("api")]
    public class MenuController : AbstractBLLController
    {
        private readonly ILogger<MenuController> _logger;
        public override string TableName => "t_menu";

        public MenuController(ILogger<MenuController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取系统菜单列表
        /// </summary>
        /// <param name="pid">指定根菜单的id</param>
        /// <returns>JSON对象，递归地包含了相应的系统菜单</returns>
        [HttpGet]
        [Route("GetMenu")]
        public JObject GetMenu(int pid)
        {
            JObject res = new JObject();
            res["status"] = 200;
            //  在这里添加判断usergroup的中间件，并将usergroup应用于筛选菜单的条件
            JArray tmp = db.GetArray("select id,name,icon,label,pid,seq from t_menu where isdeleted=0");
            JObject[] menus = new JObject[0];
            var input = tmp.ToObject<JObject[]>();
            BuildMenu(input.ToArray(), pid, ref menus);
            JArray list = new JArray();
            foreach (var item in menus)
                list.Add(item);

            res.Add("list", list);
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取系统菜单列表
        /// </summary>
        /// <returns>JSON对象，递归地包含了相应的系统菜单</returns>
        [HttpGet]
        [Route("GetMenuList")]
        public override JObject GetList()
        {
            JObject res = new JObject();
            res["status"] = 200;
            //  在这里添加判断usergroup的中间件，并将usergroup应用于筛选菜单的条件
            var groupid = HttpContext.GetIdentityInfo<int?>("groupid");
            JArray tmp = db.GetArray("select id,name,icon,label,pid,seq from t_menu where isdeleted=0 and usergroup=?p1",groupid);
            JObject[] menus = new JObject[0];
            var input = tmp.ToObject<JObject[]>();
            BuildMenu(input.ToArray(), 0, ref menus);
            JArray list = new JArray();
            foreach (var item in menus)
                list.Add(item);

            res.Add("list", list);
            res["msg"] = "读取成功";
            return res;
        }


        /// <summary>
        /// 获取系统菜单列表
        /// </summary>
        /// <returns>JSON对象，递归地包含了相应的系统菜单</returns>
        [HttpGet]
        [Route("GetMenuId")]
        public override JObject Get(int id)
        {
            //  在这里添加判断usergroup的中间件，并将usergroup应用于筛选菜单的条件
            JObject res = db.GetOne("select id,name,icon,label,pid,seq,usergroup from t_menu where id=?p1 and isdeleted=0",id);
            res["status"] = 200;
            res["msg"] = "读取成功";
            return res;
        }



        [NonAction]
        public void BuildMenu(JObject[] flat, int parentid, ref JObject[] tree)
        {
            var selfAndBro = flat.Where(t => t.Value<int>("pid") == parentid);
            JObject parent = flat.FirstOrDefault(t => t.Value<int>("id") == parentid);
            var childrenOrNephew = flat.Except(selfAndBro).ToArray();

            if (selfAndBro.Count() == 0) return;

            foreach (var cur in selfAndBro)
            {
                var children = flat.Where(t => t.Value<int>("pid") == cur.Value<int>("id"));

                // add <children> to <cur>
                JArray array = new JArray();
                foreach (var child in children)
                    array.Add(child);

                if (cur.ContainsKey("children"))
                    cur.Remove("children");
                cur.Add("children", array);

                var anchor = parent?
                    .Value<JArray>("children")?
                    .ToArray<JToken>()?
                    .FirstOrDefault(t => t.Value<int>("id") == cur.Value<int>("id"));
                if (anchor == null)
                    tree = tree.Union(new JObject[] { cur }).ToArray();// only unanchored <cur> should be unioned

                foreach (var child in children)
                {
                    var pidChild = cur.Value<int>("id");
                    var flatChild = childrenOrNephew.Union(new JObject[] { cur }).ToArray();
                    BuildMenu(flatChild, pidChild, ref tree);
                }
            }
        }

        /// <summary>
        /// 更改“菜单”信息。如果id=0新增，如果id>0修改。
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“菜单”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("SetMenu")]
        public override JObject Set([FromBody] JObject req)
        {
            return base.Set(req);
        }

        /// <summary>
        /// 删除“菜单”信息
        /// </summary>
        /// <param name="req">在请求body中JSON形式的“菜单”信息</param>
        /// <returns>JSON形式的响应状态信息</returns>
        [HttpPost]
        [Route("DelMenu")]
        public override JObject Del([FromBody] JObject req)
        {
            return base.Del(req);
        }

        public override Dictionary<string, object> GetReq(JObject req)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["name"] = req["name"]?.ToString();
            dict["icon"] = req["icon"]?.ToString();
            dict["label"] = req["label"]?.ToString();
            dict["pid"] = req["pid"]?.ToString();
            dict["usergroup"] = req["usergroup"]?.ToString();
            dict["seq"] = req["seq"]?.ToString();

            return dict;
        }
    }
}
