/*
 * Title : “Id生成”器
 * Author: zudan
 * Date  : 2020-07-20
 * Description: 获取一个“新增Id”
 * Comments
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace health.common
{
    public class IdGenerator
    {
        int serialNumber;
        DateTime cacheDateTime;
        private object lockObject = new object();

        public string GetNextId(string prefix)
        {
            if (Monitor.TryEnter(lockObject,300))
            {
                try
                {
                    if (cacheDateTime.Date != DateTime.Today.Date)
                    {
                        serialNumber = 1;
                        cacheDateTime = DateTime.Today.Date;
                    }
                    else
                    {
                        System.Threading.Interlocked.Increment(ref serialNumber);
                    }
                    StringBuilder builder = new StringBuilder();
                    builder.Append(prefix);
                    builder.Append(cacheDateTime.ToString("%yMM"));
                    builder.Append(serialNumber.ToString("D6"));
                    return builder.ToString();
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }
            }
            else
            {
                return default(string);
            }
        }
    }
}
