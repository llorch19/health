using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using util.mysql;

namespace health.web.Domain
{
    public class CheckRepository : BaseLockableRepository
    {
        public CheckRepository(dbfactory db) : base(db) { }
        public override Func<JObject, bool> IsAddAction => req => req.ToInt("id") == 0;
        public override string TableName => "t_check";
        public override Func<JObject, bool> IsLockAction => req => false;

        public override JArray GetListByOrgJointImp(int orgid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
t_check.ID
,IFNULL(CType,'') AS CheckType
,t_check.OperTime AS OperationTime
,t_check.ReportTime AS ReportTime
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_check.OrgnizationID
,t_orgnization.OrgName AS OrgName
,t_check.Result AS Result
,IFNULL(t_check.Recommend,'') AS Recommend
,IFNULL(t_check.Chosen,'') AS Chosen 
,t_patient.Tel AS PersonTel
,IFNULL(t_check.IsRexam,'') AS IsReexam
,t_orgnization.OrgCode
,IFNULL(CheckNO,'') AS DetectionNO
,IFNULL(t_check.ResultTypeID,'') AS ResultTypeID
,data_detectionresulttype.ResultName
, IFNULL(data_detectionresulttype.control1,'') AS CType
, IFNULL(data_detectionresulttype.control2,'') AS CValue
, IFNULL(t_check.IsActive,'') AS IsActive
FROM 
t_check
LEFT JOIN t_patient
ON t_check.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_check.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_check.ResultTypeID=data_detectionresulttype.ID
WHERE t_check.OrgnizationID =?p1
AND t_check.IsDeleted=0
LIMIT ?p2,?p3", orgid, offset, pageSize);
        }

        public override JArray GetListByPersonJointImp(int personid, int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            int offset = 0;
            if (pageIndex > 0)
                offset = pageSize * (pageIndex - 1);
            return _db.GetArray(@"
SELECT 
t_check.ID
,IFNULL(CType,'') AS CheckType
,t_check.OperTime AS OperationTime
,t_check.ReportTime AS ReportTime
,PatientID AS PersonID
,t_patient.FamilyName AS PersonName
,t_check.OrgnizationID
,t_orgnization.OrgName AS OrgName
,t_check.Result AS Result
,IFNULL(t_check.Recommend,'') AS Recommend
,IFNULL(t_check.Chosen,'') AS Chosen 
,t_patient.Tel AS PersonTel
,IFNULL(t_check.IsRexam,'') AS IsReexam
,t_orgnization.OrgCode
,IFNULL(CheckNO,'') AS DetectionNO
,IFNULL(t_check.ResultTypeID,'') AS ResultTypeID
,data_detectionresulttype.ResultName
, IFNULL(data_detectionresulttype.control1,'') AS CType
, IFNULL(data_detectionresulttype.control2,'') AS CValue
, IFNULL(t_check.IsActive,'') AS IsActive
FROM 
t_check
LEFT JOIN t_patient
ON t_check.PatientID=t_patient.ID
LEFT JOIN t_orgnization
ON t_check.OrgnizationID=t_orgnization.ID
LEFT JOIN data_detectionresulttype
ON t_check.ResultTypeID=data_detectionresulttype.ID
WHERE t_check.PatientID =?p1
AND t_check.IsDeleted=0
ORDER BY ReportTime,ID DESC
LIMIT ?p2,?p3", personid, offset, pageSize);
        }

        public override JArray GetListJointImp(int pageSize = Const.defaultPageSize, int pageIndex = Const.defaultPageIndex)
        {
            throw new NotImplementedException();
        }

        public override JObject GetOneRawImp(int id)
        {
            return _db.GetOne(@"
SELECT
ID
,IFNULL(CType,'') AS CheckType
,IFNULL(PatientID,'') AS PatientID
,IFNULL(OrgnizationID,'') AS OrgnizationID
,t_check.Result AS Result
,IFNULL(t_check.IsRexam,'') AS IsReexam
,Recommend
,Chosen
,IFNULL(CheckNO,'') AS DetectionNO
,IFNULL(Pics,'') AS Pics
,IFNULL(Pdf,'') AS Pdf
,IFNULL(ResultTypeID,'') AS ResultTypeID
,IFNULL(PName,'') AS ProductName
,IFNULL(Spec,'') AS Specification
,IFNULL(Batch,'') AS BatchNumber
,IFNULL(OperTime,'') AS OperationTime
,IFNULL(ReportTime,'') AS ReportTime
FROM t_check
WHERE ID=?p1
AND t_check.IsDeleted=0
", id);
        }

        public override Dictionary<string, object> GetValue(JObject data)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["PatientID"] = data.ToInt("patientid");
            dict["OrgnizationID"] = data.ToInt("orgnizationid");
            dict["ResultTypeID"] = data.ToInt("resulttypeid");
            dict["IsRexam"] = data["isreexam"]?.ToObject<int?>();
            dict["CType"] = data["checktype"]?.ToObject<string>();
            dict["CheckNO"] = data["detectionno"]?.ToObject<string>();
            dict["PName"] = data["productname"]?.ToObject<string>();
            dict["Spec"] = data["specification"]?.ToObject<string>();
            dict["Batch"] = data["batchnumber"]?.ToObject<string>();
            dict["Result"] = data["result"]?.ToObject<string>();
            dict["OperTime"] = data["operationtime"]?.ToObject<DateTime?>();
            dict["ReportTime"] = data["reporttime"]?.ToObject<DateTime?>();
            dict["Recommend"] = data["recommend"]?.ToObject<string>();
            dict["Chosen"] = data["chosen"]?.ToObject<string>();
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
            throw new NotImplementedException();
        }
    }
}
