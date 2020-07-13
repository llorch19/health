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
        public JObject GetMenu(int id)
        
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            res["code"] = 200;
            JArray tmp = db.GetArray("select id,name,icon,label,pid from t_menu");
            JObject[] menus = new JObject[0];
            var input = tmp.ToObject<JObject[]>();
            BuildMenu(input.ToArray(), id,ref menus);
            JArray list = new JArray();
            foreach (var item in menus)
            {
                list.Add(item);
            }
            res.Add("list",list);
            return res;
        }

        [NonAction]
        public void BuildMenu(JObject[] flat,int rootid,ref JObject[] tree)
        {
            var children = flat.Where(t => t.Value<int>("pid") == rootid);
            if (rootid==0)
            {
                children = children.Union(flat.Where(t => t.GetValue("pid")==null));
            }
            
            var current = flat.FirstOrDefault(t=>t.Value<int>("id")==rootid);
            var parentOrBro = flat.Except(children).ToArray();

            if (current==null && children.Count()==0)
            {
                return;
            }else if(current==null)
            {
                current = new JObject();
            }

            if (children.Count()==0)
            {
                // current has no children
                JArray array = new JArray();
                if (!current.ContainsKey("children"))
                {
                    current.Add("children", array);
                }

                
            }
            else
            {
                JArray array = new JArray();
                foreach (var child in children)
                {
                    array.Add(child);
                }


                if (current.ContainsKey("children"))
                {
                    current.Remove("children");
                }
                current.Add("children", array);
            }

            tree = tree.Union(new JObject[] { current }).ToArray();

            // recursive children
            int[] pids = children.Select(rt => rt.Value<int>("id")).Distinct().ToArray();
            foreach (int p in pids)
            {
                BuildMenu(parentOrBro, p,ref tree);
            }
        }
        
    }
}
