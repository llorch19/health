/*
 * “仓储”的基础类型
 * Author : zudan@zhifeishengwu.cn
 * Date:  2020-08-28
 * 规定了IsActive和IsDeleted的用法。
 * - IsActive: 代表记录是否可以写更新。客户端尽量不调用SetLock来修改IsActive。
 * - IsDeleted: 代表记录是否可以读取。客户端可以通过Del函数置位。
 * - 如果一个Repo不包含这两个控制字段，重写所有方法即可
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using util.mysql;

namespace health.web.Domain
{
    public abstract class BaseRepository : IRepository
    {
        protected dbfactory _db;
        public BaseRepository(dbfactory db)
        {
            _db = db;
        }

        /// <summary>
        /// 数据库中数据表的名称
        /// </summary>
        public abstract string TableName { get; }
        /// <summary>
        /// 判断提交的数据是否触发新增动作
        /// </summary>
        public abstract Func<JObject, bool> IsAddAction { get; }
        /// <summary>
        /// 判断提交的数据是否触发IsActive字段，形成锁定/解锁的效果
        /// </summary>
        public abstract Func<JObject,bool> IsLockAction { get; }

        /// <summary>
        /// 新增或修改一条数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual int AddOrUpdateRaw(JObject data,string username)
        {
            if (IsAddAction(data))
            {
                var dict = GetValue(data);
                dict["CreatedBy"] = username;
                dict["CreatedTime"] = DateTime.Now;
                dict["IsActive"] = 1;  // 新增的默认是激活的,如果Repository需要自动锁定新增，在AddOrUpdate之后调用SetLock()
                dict["IsDeleted"] = 0;
                return _db.Insert(TableName, dict);
            }
            else
            {
                if (IsLockAction(data))
                    return SetLock(data, username) > 0 
                        ? GetId(data) : 0;
                else
                {
                    var valuedata = GetValue(data);
                    var keydata = GetKey(data);
                    valuedata["LastUpdatedBy"] = username;
                    valuedata["LastUpdatedTime"] = DateTime.Now;
                    valuedata["IsActive"] = 1;  // 修改后为 IsActive = true
                    valuedata["IsDeleted"] = 0;
                    return _db.Update(TableName, valuedata, keydata)>0
                        ?GetId(data)
                        :0;
                }
            }
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual int DelRaw(JObject data,string username)
        {
            var valuedata = new Dictionary<string,object>();
            valuedata["IsDeleted"] = 1;
            valuedata["IsActive"] = 0;
            valuedata["LastUpdatedBy"] = username;
            valuedata["LastUpdatedTime"] = DateTime.Now;
            return _db.Update(TableName, valuedata, GetKey(data)) > 0
                        ? GetId(data)
                        : 0;
        }

        /// <summary>
        /// 根据data中"isactive"的值，解锁或锁定一条记录
        /// </summary>
        /// <param name="data"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual int SetLock(JObject data,string username)
        {
            var value = new Dictionary<string, object>();
            value["IsActive"] = data.ToInt("isactive");
            value["LastUpdatedBy"] = username;
            value["LastUpdatedTime"] = DateTime.Now;
            var keys = GetKey(data);
            var rc = _db.Update(TableName, value, keys);
            return rc;
        }

        /// <summary>
        /// 根据组织，获取数据列表
        /// </summary>
        /// <param name="orgid"></param>
        /// <returns></returns>
        public abstract JArray GetListByOrgJointImp(int orgid);
        /// <summary>
        /// 根据患者，获取数据列表
        /// </summary>
        /// <param name="personid"></param>
        /// <returns></returns>
        public abstract JArray GetListByPersonJointImp(int personid);
        /// <summary>
        /// 获取全表数据的列表
        /// </summary>
        /// <returns></returns>
        public abstract JArray GetListJointImp();
        /// <summary>
        /// 获取特定的一条数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract JObject GetOneRawImp(int id);
        /// <summary>
        /// 从请求的数据中，获取对象的值域
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Dictionary<string, object> GetValue(JObject data);
        /// <summary>
        /// 从请求的数据中，获取对象的可用性标识
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Dictionary<string, object> GetKey(JObject data);
        /// <summary>
        /// 从请求的数据中，获取对象的主体标识
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract int GetId(JObject data);
        /// <summary>
        /// 获取指定的对象，用于前端提示信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract JObject GetAltInfo(int? id);
    }
}
