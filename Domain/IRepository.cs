using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web
{
    public interface IRepository
    {

        JArray GetListJointImp();
        JArray GetListByOrgJointImp(int orgid);
        JArray GetListByPersonJointImp(int personid);
        JObject GetOneRawImp(int id);
        int AddOrUpdateRaw(JObject data,string username);
        bool DelRaw(JObject data,string username);
    }
}
