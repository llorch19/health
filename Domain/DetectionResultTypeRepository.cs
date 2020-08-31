﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class DetectionResultTypeRepository : BaseRepository
    {
        public DetectionResultTypeRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "data_detectionresulttype";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public override JArray GetListJointImp(int pageSize, int pageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
select 
ID
,ResultName
,IFNULL(data_detectionresulttype.control1,'') AS CType
,IFNULL(data_detectionresulttype.control2,'') AS CValue
,IsActive 
from data_detectionresulttype 
where isdeleted=0
limit ?p1,?p2
",offset,pageSize);
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
select 
ID
,ResultName
,IFNULL(data_detectionresulttype.control1,'') AS CType
,IFNULL(data_detectionresulttype.control2,'') AS CValue
,IsActive 
from data_detectionresulttype 
where id=?p1 
and isdeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["Code"] = data["code"]?.ToObject<string>();
            dict["ResultName"] = data["resultname"]?.ToObject<string>();
            dict["control1"] = data["ctype"]?.ToObject<string>();
            dict["control2"] = data["cvalue"]?.ToObject<string>();
            return dict;
        }

        public override Dictionary<string, object> GetKey(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ID"] = data.ToInt("id");
            dict["IsDeleted"] = 0;
            dict["IsActive"] = 1;  // IsActive=1 的记录可以被修改和删除
            return dict;
        }

        public override int GetId(JObject data)
        {
            return data.ToInt("id") ?? 0;
        }

        public override JObject GetAltInfo(int? id)
        {
            return _db.GetOne(@"
select id
,ResultName text 
,control1 CType
,control2 CValue
from data_detectionresulttype where id=?p1 and isdeleted=0"
, id);
        }
    }
}