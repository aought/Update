using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MyUpdate.Utils
{
    public class FtpHelper
    {
        /// <summary>
        /// ftp下载文件
        /// </summary>
        /// <param name="ftpServer">ftp服务器+端口</param>
        /// <param name="ftpUser">登陆用户</param>
        /// <param name="ftpPwd">登陆密码</param>
        /// <param name="sourceFileName">远程源文件</param>
        /// <param name="localFolder">本地保存目录</param>
        /// <param name="targetFileName">本地目标文件</param>
        public static bool FTPDownLoadFile(string ftpServer, string ftpUser, string ftpPwd, string sourceFileName, string localFolder, string targetFileName)
        {

            // downloadUrl示例："ftp://192.168.2.113//updateconfig.xml"
            // 此时downloadUrl = "ftp://localhost/bin/updateconfig.xml"
            string downloadUrl = String.Format("{0}/{1}", ftpServer, sourceFileName);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(downloadUrl);
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.Credentials = new NetworkCredential(ftpUser, ftpPwd);
            req.UseBinary = true;
            req.Proxy = null;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[2048];

                string localUrl = String.Format("{0}/{1}", localFolder.TrimEnd(new char[] { '\\', '/' }), targetFileName);
                FileStream fs = new FileStream(localUrl, FileMode.Create);
                int ReadCount = stream.Read(buffer, 0, buffer.Length);
                while (ReadCount > 0)
                {
                    fs.Write(buffer, 0, ReadCount);
                    ReadCount = stream.Read(buffer, 0, buffer.Length);
                }
                fs.Close();
                stream.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }


        }

    }
}
