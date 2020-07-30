using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using util.mysql;

namespace health
{
    public class ConstFunc
    {
        /// <summary>
        /// How to Get an identity from db
        /// </summary>
        public static readonly Func<dbfactory, object[], JObject> GetPersonIdentityFunc = 
            (db,args) => db.GetOne(@"
SELECT 
id
,OrgnizationID
,PrimaryOrgnizationID
,IDCardNO
,IDCategoryID
,OccupationCategoryID
,AddressCategoryID
,ProvinceID
,CityID
,CountyID 
FROM t_patient 
where id=?p1
", args);
        /// <summary>
        /// How to Get an identity from db
        /// </summary>
        public static readonly Func<dbfactory, object[], JObject> GetIdentityFunc =
            (db, args) => db.GetOne(@"
SELECT 
id
,OrgnizationID
,GroupId
,ProvinceID
,CityID
,CountyID 
FROM t_user 
where id=?p1
", args);

        /// <summary>
        /// How to Get function arguments from httpcontext
        /// </summary>
        public static readonly Func<HttpContext, object[]> GetPersonIdentityArgsFunc =
            context => new object[]
            {   context.User.Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
                ?.Value
                ,
            };

        /// <summary>
        /// How to Get function arguments from httpcontext
        /// </summary>
        public static readonly Func<HttpContext, object[]> GetIdentityArgsFunc =
            context => {
                var res =new List<object>();
                var strID = context.User.Claims
                .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
                ?.Value;
                res.Add(strID);
                return res.ToArray();
             };

        /// <summary>
        /// What's the name of 
        /// </summary>
        public const string IdentityEntry = "SysPerson_";
    }
}
