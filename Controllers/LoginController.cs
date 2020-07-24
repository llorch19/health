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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using util.mysql;

namespace health.Controllers
{
    [ApiController]
    [Route("api")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        dbfactory db = new dbfactory();
        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// 机构用户登录接口
        /// </summary>
        /// <param name="username">机构用户登录帐号</param>
        /// <param name="password">机构用户登录密码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Get[controller]")]
        public JObject Login(string username = "lqq", string password = "zf300122")
        {
            JObject res = new JObject();
            JObject user = db.GetOne(@"
SELECT 
ID
,OrgnizationID
,ProvinceID
,CityID
,CountyID
,PasswordHash
FROM
t_user
WHERE
Username=?p1",username);
            if (user["id"]==null 
                || user["passwordhash"].ToObject<string>()!= util.Security.String2MD5(password))
            {
                res["status"] = 201;
                res["msg"] = "登录失败";
                return res;
            }


           

            var claimsIdentity = new ClaimsIdentity(new[]{
                new Claim(ClaimTypes.Name, username),
                new Claim("userid",user["id"]?.ToObject<string>()),
                new Claim("orgnizationid",user["orgnizationid"]?.ToObject<string>()),
                new Claim("provinceid",user["provinceid"]?.ToObject<string>()),
                new Claim("cityid",user["cityid"]?.ToObject<string>()),
                new Claim("countyid",user["countyid"]?.ToObject<string>())
            });

            var handler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretsecretsecret")), SecurityAlgorithms.HmacSha256),
            };
            var securityToken = handler.CreateToken(tokenDescriptor);
            var token = handler.WriteToken(securityToken);
            res["token"] = token;
            res["status"] = 200;
            res["msg"] = "登录成功";
            return res;
        }

        /// <summary>
        /// 打印机构用户的系统凭据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Show[controller]")]
        [Authorize]
        public JObject Show()
        {
            //这是获取自定义参数的方法
            var authenticateResult = HttpContext.AuthenticateAsync().Result;
            var auth = authenticateResult?.Principal?.Claims;
            JObject res = new JObject();
            foreach (var c in auth)
            {
                res[c.Type]=c.Value;
                if (c.Type=="exp")
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(res[c.Type].ToObject<double>()).ToLocalTime();
                    res["expiry"] = dtDateTime;
                }
            }

            if (res.HasValues)
            {
                res["status"] = 200;
                res["msg"] = "鉴权成功";
            }
            else
            {
                res["status"] = 201;
                res["msg"] = "鉴权失败";
            }

            return res;
        }
    }
}
