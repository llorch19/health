/*
 * Title : 菜单获取控制器
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
    }]
})
 * - 代码风格方面，单行的if和循环语句要删除前后大括号 @norway  2020-07-13 17:31
 * 
 */
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Linq;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly ILogger<MenuController> _logger;
        public MenuController(ILogger<MenuController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取区域信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMenu")]
        public JObject GetMenu(int pid)
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            res["status"] = 200;
            //  在这里添加判断usergroup的中间件，并将usergroup应用于筛选菜单的条件
            JArray tmp = db.GetArray("select id,name,icon,label,pid from t_menu");
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

    }
}
