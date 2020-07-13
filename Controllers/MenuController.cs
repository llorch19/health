using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using util;
using util.mysql;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Localization;

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
            BuildMenu(input.ToArray(), pid,ref menus);
            JArray list = new JArray();
            foreach (var item in menus)
            {
                list.Add(item);
            }
            res.Add("list",list);
            res["msg"] = "读取成功";
            return res;
        }

        [NonAction]
        public void BuildMenu(JObject[] flat,int parentid,ref JObject[] tree)
        {
            var selfAndBro = flat.Where(t => t.Value<int>("pid") == parentid);
            JObject parent = flat.FirstOrDefault(t=>t.Value<int>("id")==parentid);
            var childrenOrNephew = flat.Except(selfAndBro).ToArray();

            if (selfAndBro.Count()==0)
            {
                return;
            }

            foreach (var cur in selfAndBro)
            {
                var children = flat.Where(t=> t.Value<int>("pid")==cur.Value<int>("id"));
                
                // add <children> to <cur>
                JArray array = new JArray();
                foreach (var child in children)
                {
                    array.Add(child);
                }


                if (cur.ContainsKey("children"))
                {
                    cur.Remove("children");
                }
                cur.Add("children", array);


                var anchor = parent?.Value<JArray>("children")?.ToArray<JToken>()?.FirstOrDefault(t=>t.Value<int>("id")==cur.Value<int>("id"));
                if (anchor==null)
                {
                    // only unanchored <cur> should be unioned
                    tree = tree.Union(new JObject[] { cur }).ToArray();
                }

              

                foreach (var child in children)
                {
                    var pidChild = cur.Value<int>("id");
                    var flatChild = childrenOrNephew.Union(new JObject[] { cur }).ToArray();
                    BuildMenu(flatChild, pidChild,ref tree);
                }
            }
        }
        
    }
}
