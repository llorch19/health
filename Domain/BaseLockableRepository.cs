using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public abstract class BaseLockableRepository : BaseRepository
    {
        protected BaseLockableRepository(dbfactory db) : base(db)
        {
        }

        /// <summary>
        /// 根据data中"isactive"的值，解锁或锁定一条记录
        /// </summary>
        /// <param name="personid"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual int LockPersonData(int personid, string username)
        {
            var value = new Dictionary<string, object>();
            value["IsActive"] = 0;
            value["LastUpdatedBy"] = username;
            value["LastUpdatedTime"] = DateTime.Now;
            var keys = new Dictionary<string, object>();
            keys["PatientID"] = personid;
            keys["OrgnizationID"] = personid;
            var rc = _db.Update(TableName, value, keys);
            return rc;
        }
    }
}
