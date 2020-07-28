using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.Middleware
{
    public class ModelException:Exception
    {
        public override string Message => "参数错误";
    }
}
