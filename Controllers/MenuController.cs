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

namespace health.Controllers
{
    [ApiController]
    public class MenuController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        JObject[] data;

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
        public JObject GetMenu()
        
        {
            dbfactory db = new dbfactory();
            JObject res = new JObject();
            res["code"] = 200;
            JArray tmp = db.GetArray("select id,name,icon,label,pid from t_menu");
            JObject[] menus = new JObject[0];
            var input = tmp.ToObject<JObject[]>();
            BuildMenu(input.ToArray(), 0,ref menus);
            JArray list = new JArray();
            foreach (var item in menus)
            {
                list.Add(item);
            }
            res.Add("list",list);
            return res;
        }

        [NonAction]
        public void BuildMenu(JObject[] tokens,int pid,ref JObject[] output)
        {
            var parent = output.FirstOrDefault(t => t.Value<int>("id") == pid);
            var curChildren = tokens.Where(t=>t.Value<int>("pid")==pid);

            if (parent==null)
            {
                foreach (var current in curChildren)
                {
                    JArray array = new JArray();
                    if (!current.ContainsKey("children"))
                    {
                        current.Add("children", array);
                    }
                }
                
                output = output.Concat(curChildren).ToArray();
            }
            else
            {
                JArray array = new JArray();

                foreach (var current in curChildren)
                {
                    array.Add(current);
                }
                if (parent.ContainsKey("children"))
                {
                    parent.Remove("children");
                }
                parent.Add("children", array);
            }
            

            tokens = tokens.Except(curChildren).ToArray();


            int[] pids = tokens.Select(rt => rt.Value<int>("pid")).Distinct().ToArray();
            foreach (int p in pids)
            {
                BuildMenu(tokens,p,ref output);
            }
        }
        
    }
}
