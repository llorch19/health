using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace health.common
{
    public static class FileHelpers
    {
        // If you require a check on specific characters in the IsValidFileExtensionAndSignature
        // method, supply the characters in the _allowedChars field.
        private static readonly byte[] _allowedChars = { };
        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.
        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
            { ".pdf",new List<byte[]>
                {
                    new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D },
                } 
            },
        };


        public static readonly Dictionary<string, string> mimetype = new Dictionary<string, string>()
        {
        {".323", "text/h323"},
        {".asx", "video/x-ms-asf"},
        {".acx", "application/internet-property-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aiff", "audio/aiff"},
        {".axs", "application/olescript"},
        {".aifc", "audio/aiff"},
        {".asr", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".asf", "video/x-ms-asf"},
        {".au", "audio/basic"},
        {".application", "application/x-ms-application"},
        {".bin", "application/octet-stream"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bmp", "image/bmp"},
        {".cdf", "application/x-cdf"},
        {".cat", "application/vndms-pkiseccat"},
        {".crt", "application/x-x509-ca-cert"},
        {".c", "text/plain"},
        {".css", "text/css"},
        {".cer", "application/x-x509-ca-cert"},
        {".crl", "application/pkix-crl"},
        {".cmx", "image/x-cmx"},
        {".csh", "application/x-csh"},
        {".cod", "image/cis-cod"},
        {".cpio", "application/x-cpio"},
        {".clp", "application/x-msclip"},
        {".crd", "application/x-mscardfile"},
        {".deploy", "application/octet-stream"},
        {".dll", "application/x-msdownload"},
        {".dot", "application/msword"},
        {".doc", "application/msword"},
        {".dvi", "application/x-dvi"},
        {".dir", "application/x-director"},
        {".dxr", "application/x-director"},
        {".der", "application/x-x509-ca-cert"},
        {".dib", "image/bmp"},
        {".dcr", "application/x-director"},
        {".disco", "text/xml"},
        {".exe", "application/octet-stream"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".eml", "message/rfc822"},
        {".eps", "application/postscript"},
        {".flr", "x-world/x-vrml"},
        {".fif", "application/fractals"},
        {".gtar", "application/x-gtar"},
        {".gif", "image/gif"},
        {".gz", "application/x-gzip"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htt", "text/webviewhtml"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hlp", "application/winhlp"},
        {".html", "text/html"},
        {".htm", "text/html"},
        {".hqx", "application/mac-binhex40"},
        {".isp", "application/x-internet-signup"},
        {".iii", "application/x-iphone"},
        {".ief", "image/ief"},
        {".ivf", "video/x-ivf"},
        {".ins", "application/x-internet-signup"},
        {".ico", "image/x-icon"},
        {".jpg", "image/jpeg"},
        {".jfif", "image/pjpeg"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".lsx", "video/x-la-asf"},
        {".latex", "application/x-latex"},
        {".lsf", "video/x-la-asf"},
        {".manifest", "application/x-ms-manifest"},
        {".mhtml", "message/rfc822"},
        {".mny", "application/x-msmoney"},
        {".mht", "message/rfc822"},
        {".mid", "audio/mid"},
        {".mpv2", "video/mpeg"},
        {".man", "application/x-troff-man"},
        {".mvb", "application/x-msmediaview"},
        {".mpeg", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".mdb", "application/x-msaccess"},
        {".mpp", "application/vnd.ms-project"},
        {".m1v", "video/mpeg"},
        {".mpa", "video/mpeg"},
        {".me", "application/x-troff-me"},
        {".m13", "application/x-msmediaview"},
        {".movie", "video/x-sgi-movie"},
        {".m14", "application/x-msmediaview"},
        {".mpe", "video/mpeg"},
        {".mp2", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpg", "video/mpeg"},
        {".ms", "application/x-troff-ms"},
        {".nc", "application/x-netcdf"},
        {".nws", "message/rfc822"},
        {".oda", "application/oda"},
        {".ods", "application/oleobject"},
        {".pmc", "application/x-perfmon"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7s", "application/pkcs7-signature"},
        {".pmw", "application/x-perfmon"},
        {".ps", "application/postscript"},
        {".p7c", "application/pkcs7-mime"},
        {".pbm", "image/x-portable-bitmap"},
        {".ppm", "image/x-portable-pixmap"},
        {".pub", "application/x-mspublisher"},
        {".pnm", "image/x-portable-anymap"},
        {".png", "image/png"},
        {".pml", "application/x-perfmon"},
        {".p10", "application/pkcs10"},
        {".pfx", "application/x-pkcs12"},
        {".p12", "application/x-pkcs12"},
        {".pdf", "application/pdf"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".p7m", "application/pkcs7-mime"},
        {".pko", "application/vndms-pkipko"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pmr", "application/x-perfmon"},
        {".pma", "application/x-perfmon"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".prf", "application/pics-rules"},
        {".pgm", "image/x-portable-graymap"},
        {".qt", "video/quicktime"},
        {".ra", "audio/x-pn-realaudio"},
        {".rgb", "image/x-rgb"},
        {".ram", "audio/x-pn-realaudio"},
        {".rmi", "audio/mid"},
        {".ras", "image/x-cmu-raster"},
        {".roff", "application/x-troff"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".sv4crc", "application/x-sv4crc"},
        {".spc", "application/x-pkcs7-certificates"},
        {".setreg", "application/set-registration-initiation"},
        {".snd", "audio/basic"},
        {".stl", "application/vndms-pkistl"},
        {".setpay", "application/set-payment-initiation"},
        {".stm", "text/html"},
        {".shar", "application/x-shar"},
        {".sh", "application/x-sh"},
        {".sit", "application/x-stuffit"},
        {".spl", "application/futuresplash"},
        {".sct", "text/scriptlet"},
        {".scd", "application/x-msschedule"},
        {".sst", "application/vndms-pkicertstore"},
        {".src", "application/x-wais-source"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".tex", "application/x-tex"},
        {".tgz", "application/x-compressed"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tr", "application/x-troff"},
        {".tif", "image/tiff"},
        {".txt", "text/plain"},
        {".texinfo", "application/x-texinfo"},
        {".trm", "application/x-msterminal"},
        {".tiff", "image/tiff"},
        {".tcl", "application/x-tcl"},
        {".texi", "application/x-texinfo"},
        {".tsv", "text/tab-separated-values"},
        {".ustar", "application/x-ustar"},
        {".uls", "text/iuls"},
        {".vcf", "text/x-vcard"},
        {".wps", "application/vnd.ms-works"},
        {".wav", "audio/wav"},
        {".wrz", "x-world/x-vrml"},
        {".wri", "application/x-mswrite"},
        {".wks", "application/vnd.ms-works"},
        {".wmf", "application/x-msmetafile"},
        {".wcm", "application/vnd.ms-works"},
        {".wrl", "x-world/x-vrml"},
        {".wdb", "application/vnd.ms-works"},
        {".wsdl", "text/xml"},
        {".xap", "application/x-silverlight-app"},
        {".xml", "text/xml"},
        {".xlm", "application/vnd.ms-excel"},
        {".xaf", "x-world/x-vrml"},
        {".xla", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xof", "x-world/x-vrml"},
        {".xlt", "application/vnd.ms-excel"},
        {".xlc", "application/vnd.ms-excel"},
        {".xsl", "text/xml"},
        {".xbm", "image/x-xbitmap"},
        {".xlw", "application/vnd.ms-excel"},
        {".xpm", "image/x-xpixmap"},
        {".xwd", "image/x-xwindowdump"},
        {".xsd", "text/xml"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
        {".*", "application/octet-stream"},
        };

        // **WARNING!**
        // In the following file processing methods, the file's content isn't scanned.
        // In most production scenarios, an anti-virus/anti-malware scanner API is
        // used on the file before making the file available to users or other
        // systems. For more information, see the topic that accompanies this sample
        // app.

        public static async Task<byte[]> ProcessFormFile<T>(IFormFile formFile,
            ModelStateDictionary modelState, string[] permittedExtensions,
            long sizeLimit)
        {
            var fieldDisplayName = string.Empty;

            // Use reflection to obtain the display name for the model
            // property associated with this IFormFile. If a display
            // name isn't found, error messages simply won't show
            // a display name.
            MemberInfo property =
                typeof(T).GetProperty(
                    formFile.Name.Substring(formFile.Name.IndexOf(".",
                    StringComparison.Ordinal) + 1));

            if (property != null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute)) is
                    DisplayAttribute displayAttribute)
                {
                    fieldDisplayName = $"{displayAttribute.Name} ";
                }
            }

            // Don't trust the file name sent by the client. To display
            // the file name, HTML-encode the value.
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                formFile.FileName);

            // Check the file length. This check doesn't catch files that only have 
            // a BOM as their content.
            if (formFile.Length == 0)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");

                return new byte[0];
            }

            if (formFile.Length > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / 1048576;
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) exceeds " +
                    $"{megabyteSizeLimit:N1} MB.");

                return new byte[0];
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);

                    // Check the content length in case the file's only
                    // content was a BOM and the content is actually
                    // empty after removing the BOM.
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");
                    }

                    if (!IsValidFileExtensionAndSignature(
                        formFile.FileName, memoryStream, permittedExtensions))
                    {
                        modelState.AddModelError(formFile.Name,
                            $"{fieldDisplayName}({trustedFileNameForDisplay}) file " +
                            "type isn't permitted or the file's signature " +
                            "doesn't match the file's extension.");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) upload failed. " +
                    $"Please contact the Help Desk for support. Error: {ex.HResult}");
                // Log the exception
            }

            return new byte[0];
        }

        public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

                    // Check if the file is empty or exceeds the size limit.
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError("File", "The file is empty.");
                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabyteSizeLimit = sizeLimit / 1048576;
                        modelState.AddModelError("File",
                        $"The file exceeds {megabyteSizeLimit:N1} MB.");
                    }
                    else if (!IsValidFileExtensionAndSignature(
                        contentDisposition.FileName, memoryStream,
                        permittedExtensions))
                    {
                        modelState.AddModelError("File",
                            "The file type isn't permitted or the file's " +
                            "signature doesn't match the file's extension.");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("File",
                    "The upload failed. Please contact the Help Desk " +
                    $" for support. Error: {ex.HResult}");
                // Log the exception
            }

            return new byte[0];
        }

        public static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                if (ext.Equals(".txt") || ext.Equals(".csv") || ext.Equals(".prn"))
                {
                    if (_allowedChars.Length == 0)
                    {
                        // Limits characters to ASCII encoding.
                        for (var i = 0; i < data.Length; i++)
                        {
                            if (reader.ReadByte() > sbyte.MaxValue)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // Limits characters to ASCII encoding and
                        // values of the _allowedChars array.
                        for (var i = 0; i < data.Length; i++)
                        {
                            var b = reader.ReadByte();
                            if (b > sbyte.MaxValue ||
                                !_allowedChars.Contains(b))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                // Uncomment the following code block if you must permit
                // files whose signature isn't provided in the _fileSignature
                // dictionary. We recommend that you add file signatures
                // for files (when possible) for all file types you intend
                // to allow on the system and perform the file signature
                // check.
                /*
                if (!_fileSignature.ContainsKey(ext))
                {
                    return true;
                }
                */

                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature
                // dictionary, the following code tests the input content's
                // file signature.
                var signatures = _fileSignature[ext];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    
        public static bool CheckFiles(IFormFile[] files, string[] permittedExtensions, CancellationToken cancellationToken)
        {
            List<string> results = new List<string>();
            for (int checkIndex = 0; checkIndex < files?.Length && !cancellationToken.IsCancellationRequested; checkIndex++)
            {
                using (var memory = new MemoryStream())
                {
                    files[checkIndex].CopyTo(memory);
                    if (FileHelpers.IsValidFileExtensionAndSignature(files[checkIndex].FileName, memory, permittedExtensions))
                    {
                        results.Add(files[checkIndex].FileName);
                    }
                }
            }
            return files.Length == results.Count;
        }

        public static string[] UploadStorage(IFormFile[] files,string desDirectory,CancellationToken cancellationToken)
        {
            List<string> uploadResults = new List<string>();

            if (!Directory.Exists(desDirectory) && !cancellationToken.IsCancellationRequested)
                Directory.CreateDirectory(desDirectory);

            for (int actionIndex = 0; actionIndex < files?.Length && !cancellationToken.IsCancellationRequested; actionIndex++)
            {
                string filepath = Path.Combine(desDirectory, Path.GetRandomFileName() + Path.GetExtension(files[actionIndex].FileName));
                while (System.IO.File.Exists(filepath))
                    filepath = Path.Combine(desDirectory, Path.GetRandomFileName() + Path.GetExtension(files[actionIndex].FileName));

                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    files[actionIndex].CopyTo(fileStream);
                }

                uploadResults.Add(Path.GetFullPath(filepath));
            }
            return uploadResults.ToArray();           
        }
    }
}
