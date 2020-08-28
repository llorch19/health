using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web
{
    public interface IRepository
    {
        JArray GetListJointImp(int pageSize,int pageIndex);
        JArray GetListByOrgJointImp(int orgid,int pageSize,int pageIndex);
        JArray GetListByPersonJointImp(int personid,int pageSize,int pageIndex);
        JObject GetOneRawImp(int id);
        JObject GetAltInfo(int? id);
        int AddOrUpdateRaw(JObject data,string username);
        int DelRaw(JObject data,string username);
    }
}
