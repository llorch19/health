using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.common
{
    public class DateTimeUtil
    {
        public static long GetUnixTimeStamp()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(DateTime.Now);
            return dateTimeOffset.ToUnixTimeMilliseconds();
        }
    }
}
