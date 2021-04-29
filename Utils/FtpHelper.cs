﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MyUpdate.Utils
{
    public class FtpHelper
    {
        public static void FTPDownLoadFile(string url, string dir, string fileName)
        {


            string downloadUrl = String.Format("{0}/{1}", url, fileName);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(downloadUrl);
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.Credentials = new NetworkCredential("open", "open");
            req.UseBinary = true;
            req.Proxy = null;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[2048];
                // 传入的主程序目录后面没有\\
                FileStream fs = new FileStream(dir + "\\" + fileName, FileMode.Create);
                int ReadCount = stream.Read(buffer, 0, buffer.Length);
                while (ReadCount > 0)
                {
                    fs.Write(buffer, 0, ReadCount);
                    ReadCount = stream.Read(buffer, 0, buffer.Length);
                }
                fs.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            

        }

        public static void FTPDownLoadFile(string url, string fileNameSer, string fileNameCli, string dir)
        {

            // downloadUrl示例："ftp://192.168.2.113//updateconfig.xml"
            // 此时downloadUrl = "ftp://localhost/bin/updateconfig.xml"
            string downloadUrl = String.Format("{0}/{1}", url, fileNameSer);
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create(downloadUrl);
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.Credentials = new NetworkCredential("open", "open");
            req.UseBinary = true;
            req.Proxy = null;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[2048];
                // dir："C:\\Users\\Empty\\Documents\\GitHub\\Update\\bin\\Debug\\"
                // fileNameCli："temp_config.xml"
                FileStream fs = new FileStream(dir + fileNameCli, FileMode.Create);
                int ReadCount = stream.Read(buffer, 0, buffer.Length);
                while (ReadCount > 0)
                {
                    fs.Write(buffer, 0, ReadCount);
                    ReadCount = stream.Read(buffer, 0, buffer.Length);
                }
                fs.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }

    }
}
