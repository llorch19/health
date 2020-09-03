using health.common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
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
        IWebHostEnvironment _env;
        public FileUploadCommand(IWebHostEnvironment env)
        {
            _env = env;
        }

        public bool ExecuteRelative(
            IFormFile[] arrFormFile
            , CancellationToken objCancelToken
            , string strPhysicalDir
            , string[] arrExtsAllowed
            , int intMaxCount
            , long lngMaxSize
            , out string[] lstPhysicalPath
            , out string strError)
        {
            if (!objCancelToken.IsCancellationRequested && arrFormFile.Length > intMaxCount)
            {
                strError = "不能同时上传 " + arrFormFile.Length + " 个文件";
                lstPhysicalPath = new string[0];
                return false;
            }

            if (!objCancelToken.IsCancellationRequested &&
                arrFormFile
                .Where(f => f.Length == 0 || f.Length > lngMaxSize)
                .FirstOrDefault() != null)
            {
                strError = "不能上传大于 " + lngMaxSize + " 的文件";
                lstPhysicalPath = new string[0];
                return false;
            }


            if (!objCancelToken.IsCancellationRequested &&
                !Directory.Exists(strPhysicalDir))
                Directory.CreateDirectory(strPhysicalDir);

            List<string> array = new List<string>();
            foreach (var f in arrFormFile)
            {
                string filepath = Path.Combine(strPhysicalDir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (!objCancelToken.IsCancellationRequested && System.IO.File.Exists(filepath))
                    filepath = Path.Combine(strPhysicalDir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                FileInfo fInfo = new FileInfo(filepath);
                using (var memoryStream = new MemoryStream())
                {
                    f.CopyTo(memoryStream);
                    if (!objCancelToken.IsCancellationRequested && !FileHelpers.IsValidFileExtensionAndSignature(f.FileName, memoryStream, arrExtsAllowed))
                    {
                        strError = f.FileName + " 不能上传";
                        lstPhysicalPath = new string[0];
                        return false;
                    }
                    else
                    {
                        using (var fileStream = fInfo.Create())
                            f.CopyTo(fileStream);
                    }
                }

                Uri fUri = new Uri(fInfo.FullName);
                Uri rootUri = new Uri(_env.ContentRootPath+Path.DirectorySeparatorChar);
                Uri rUri = rootUri.MakeRelativeUri(fUri);
                array.Add(rUri.ToString());
            }


            strError = "上传完成";
            lstPhysicalPath = array.ToArray();
            return true;
        }


        public bool ExecuteAbsolute(
            IFormFile[] arrFormFile
            , CancellationToken objCancelToken
            , string strPhysicalDir
            , string[] arrExtsAllowed
            , int intMaxCount
            , long lngMaxSize
            , out string[] lstPhysicalPath
            , out string strError)
        {
            if (!objCancelToken.IsCancellationRequested && arrFormFile.Length > intMaxCount)
            {
                strError = "不能同时上传 " + arrFormFile.Length + " 个文件";
                lstPhysicalPath = new string[0];
                return false;
            }

            if (!objCancelToken.IsCancellationRequested &&
                arrFormFile
                .Where(f => f.Length == 0 || f.Length > lngMaxSize)
                .FirstOrDefault() != null)
            {
                strError = "不能上传大于 " + lngMaxSize + " 的文件";
                lstPhysicalPath = new string[0];
                return false;
            }


            if (!objCancelToken.IsCancellationRequested &&
                !Directory.Exists(strPhysicalDir))
                Directory.CreateDirectory(strPhysicalDir);

            List<string> array = new List<string>();
            foreach (var f in arrFormFile)
            {
                string filepath = Path.Combine(strPhysicalDir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                while (!objCancelToken.IsCancellationRequested && System.IO.File.Exists(filepath))
                    filepath = Path.Combine(strPhysicalDir, Path.GetRandomFileName() + Path.GetExtension(f.FileName));
                FileInfo fInfo = new FileInfo(filepath);
                using (var memoryStream = new MemoryStream())
                {
                    f.CopyTo(memoryStream);
                    if (!objCancelToken.IsCancellationRequested && !FileHelpers.IsValidFileExtensionAndSignature(f.FileName, memoryStream, arrExtsAllowed))
                    {
                        strError = f.FileName + " 不能上传";
                        lstPhysicalPath = new string[0];
                        return false;
                    }
                    else
                    {
                        using (var fileStream = fInfo.Create())
                            f.CopyTo(fileStream);
                    }
                }

                Uri fUri = new Uri(fInfo.FullName);
                array.Add(fUri.ToString());
            }


            strError = "上传完成";
            lstPhysicalPath = array.ToArray();
            return true;
        }
    }
}
