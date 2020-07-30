using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class FastFailException:Exception
    {
        public override string Message => "出现错误，请稍后再试。";
    }
}
