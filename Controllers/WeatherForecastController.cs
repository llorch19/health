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

namespace health.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAreaList")]
        public JObject GetAreaList(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            JObject res = new JObject();
            res["status"] = 200;
            res["msg"] = "读取成功";
           
            dbfactory db = new dbfactory();
            JArray rows = db.GetArray("select id,AreaCode,AreaName from data_area where parentID=?p1",id); 
            
            res["list"] = rows;
            return res;
        }
        /// <summary>
        /// 获取区域信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetArea")]
        public JObject GetArea(int id)
        {
            //int id = 0;
            //int.TryParse(HttpContext.Request.Query["id"],out id);
            dbfactory db = new dbfactory();
            JObject res = db.GetOne("select id,AreaCode,AreaName from data_area where id=?p1",id); 
            if(res["id"] != null)
                res["status"] = 200;
            else{
                res["status"] = 201;
                res["msg"] = "查询不到对应的数据";
            }
            return res;
        }


        [HttpGet("SetArea")]
        public JObject SetArea([FromBody] JObject req)
        {
            dbfactory db=new dbfactory();
            JObject res=new JObject();
            if(req["id"] !=null)
            {
                int id=req["id"].ToObject<int>();
                if(id==0)
                {
                    var dict=req.ToObject<Dictionary<string,object>>();
                    var rows=db.Insert("data_area",dict);
                    if(rows>0)
                    {
                        res["status"]=200;
                        res["msg"]="新增成功";
                    }
                    else
                    {
                        res["status"]=201;
                        res["msg"]="无法新增数据";
                    }
                }
                else if(id>0)
                {
                    var dict = req.ToObject<Dictionary<string,object>>();
                    dict.Remove("id");
                    var keys = new Dictionary<string,object>();
                    keys["id"]=req["id"];
                    var rows=db.Update("data_area",dict,keys);
                    if(rows>0)
                    {
                       res["status"]=200;
                       res["msg"]="修改成功";
                    }
                    else{
                        res["status"]=201;
                        res["msg"]="修改失败";
                    }
                }
            }
            else{
                res["status"]=201;
                res["message"]="非法的请求";
            }
            return res;
        }


        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            /*
            string basePath = "";
            basePath = string.IsNullOrWhiteSpace(basePath) ? 
                AppDomain.CurrentDomain.BaseDirectory + "config/AppConfig.json" :
                AppDomain.CurrentDomain.BaseDirectory + "config/" + basePath;

            IConfigurationRoot configroot = new ConfigurationBuilder().
              AddJsonFile(basePath).Build();

            _logger.LogInformation("数据库连接信息:{0}", configroot["ConnectStrings:default"]);
            */
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }



        [HttpGet]
        [Route("auth")]
        public string auth(string userName="dddd", string pwd = "123")
        {
            IDictionary<string, string> Tokens = new Dictionary<string, string>();
            var claimsIdentity = new ClaimsIdentity(new[]{
                new Claim(ClaimTypes.Name, "admin")
            });
            
            var handler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.Now.AddMinutes(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretsecretsecret")), SecurityAlgorithms.HmacSha256),
            };
            var securityToken = handler.CreateToken(tokenDescriptor);
            var token = handler.WriteToken(securityToken);
            Tokens.Add(token, "admin");
            return token;
        }
        [HttpGet]
        [Route("GetAuth")]
        [Authorize]
        public ActionResult<IEnumerable<string>> CheckAuth()
        {
            //这是获取自定义参数的方法
            var auth = HttpContext.AuthenticateAsync().Result.Principal.Claims;
            var userName = auth.FirstOrDefault(t => t.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
            return new string[] { "这个接口登陆过的用户都可以访问", $"userName={userName}" };
        }
    }
}
