using health.common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace health.web.Service
{
    public class UploadService
    {
        public bool  UploadFile(IFormFile[] files, CancellationToken cancellationToken, string dirstore,int limitcount,long limitsize,out List<string> rlist,out string msg)
        {
            if (!Directory.Exists(dirstore))
                Directory.CreateDirectory(dirstore);

            List<string> array = new List<string>();
            foreach (var f in files)
            {
                string filepath = Path.Combine(dirstore, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(dirstore, Path.GetRandomFileName() + Path.GetExtension(f.FileName));

                using (var memoryStream = new MemoryStream())
                {
                    f.CopyTo(memoryStream);
                    if (!FileHelpers.IsValidFileExtensionAndSignature(f.FileName, memoryStream, new string[] { ".zip" }))
                    {
                        msg = f.FileName + " 不能上传";
                        rlist = new List<string>();
                        return false;
                    }
                    else
                    {
                        using (var fileStream = new FileStream(filepath, FileMode.Create))
                            f.CopyTo(fileStream);
                    }
                }

                Uri full = new Uri(filepath);
                array.Add(full.ToString());
                
            }


            msg = "上传完成";
            rlist = array;
            return true;
        }
    }
}
