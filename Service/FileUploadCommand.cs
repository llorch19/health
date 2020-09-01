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
    public class FileUploadCommand
    {
        public bool Execute(
            IFormFile[] files
            , CancellationToken ctrl_token
            , string server_dir
            , string[] allow_ext
            , int max_count
            , long max_size
            , out string[] server_file_list
            , out string err_msg)
        {
            if (files.Length > max_count)
            {
                err_msg = "不能同时上传 " + files.Length + " 个文件";
                server_file_list = new string[0];
                return false;
            }

            if (files
                .Where(f => f.Length == 0 || f.Length > max_size)
                .FirstOrDefault() != null)
            {
                err_msg = "不能上传大于 " + max_size + " 的文件";
                server_file_list = new string[0];
                return false;
            }


            if (!Directory.Exists(server_dir))
                Directory.CreateDirectory(server_dir);

            List<string> array = new List<string>();
            foreach (var f in files)
            {
                string filepath = Path.Combine(server_dir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(server_dir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));

                using (var memoryStream = new MemoryStream())
                {
                    f.CopyTo(memoryStream);
                    if (!FileHelpers.IsValidFileExtensionAndSignature(f.FileName, memoryStream, allow_ext))
                    {
                        err_msg = f.FileName + " 不能上传";
                        server_file_list = new string[0];
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


            err_msg = "上传完成";
            server_file_list = array.ToArray();
            return true;
        }
    }
}
