using health.web.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace health.web.Service
{
    public class ReadNoticeCommand
    {
        NoticeRepository _srvNotice;
        ReadNoticeRepository _srvReadNotice;
        public ReadNoticeCommand(NoticeRepository noticeRepository,ReadNoticeRepository readnoticeRepository)
        {
            _srvNotice = noticeRepository;
            _srvReadNotice = readnoticeRepository;
        }

        public JObject UserReadNotice(string username,int userid,int noticeid)
        {
            JObject objNotice = new JObject();
            var objUserNotice = _srvReadNotice.GetOneRawByUserNoticeImp(userid, noticeid);
            if (objUserNotice.ToInt("noticeid")==noticeid)
                return _srvNotice.GetOneRawImp(noticeid);  //  已读

            JObject objReadNotice = new JObject();
            objReadNotice["noticeid"] = noticeid;
            objReadNotice["userid"] = userid;
            var rc = _srvReadNotice.AddOrUpdateRaw(objReadNotice, username); // 新增已读
            if (rc > 0)
                return _srvNotice.GetOneRawImp(noticeid);
            else
                return null;
        }


        public Array GetUnreadNotice(int userid)
        {
            JObject objNotice = new JObject();
            var objUserNotice = _srvReadNotice.GetOneRawByUserNoticeImp(userid, noticeid);
            if (objUserNotice.ToInt("noticeid") == noticeid)
                return _srvNotice.GetOneRawImp(noticeid);  //  已读

            JObject objReadNotice = new JObject();
            objReadNotice["noticeid"] = noticeid;
            objReadNotice["userid"] = userid;
            var rc = _srvReadNotice.AddOrUpdateRaw(objReadNotice, username); // 新增已读
            if (rc > 0)
                return _srvNotice.GetOneRawImp(noticeid);
            else
                return null;
        }
    }
}
