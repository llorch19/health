using health.web.common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.Controllers
{
    [Route("test")]
    public class TestController:ControllerBase
    {
        [HttpGet("GetInviteCode")]
        public string GetInviteCode()
        {
            return ShareCodeUtils.idToCode(DateTimeUtil.GetUnixTimeStamp());
        }
    }
}
