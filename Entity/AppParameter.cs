using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace MyUpdate.Entity
{
    public class AppParameter
    {
        public static string currentFolder = new DirectoryInfo(".").FullName;
        public static string fatherFolder = new DirectoryInfo("..").FullName;
        public static string proName = new DirectoryInfo("..").Name;
        public static string appName = Path.Combine(fatherFolder, proName + ".exe");

        /// <summary>
        /// 备份路径
        /// </summary>
        // public static string BackupPath = ConfigurationManager.AppSettings["backupPath"];
        public static string BackupPath = Path.Combine(fatherFolder, "Backup");


        /// <summary>
        /// 更新的URL
        /// </summary>
        // public static string ServerURL = ConfigurationManager.AppSettings["serverURL"];
        public static string ServerURL = String.Format("{0}/{1}", ConfigurationManager.AppSettings["serverURL"], new DirectoryInfo("..").Name);

        /// <summary>
        /// 本地更新文件全名
        /// </summary>
        // public static string LocalUPdateConfig = ConfigurationManager.AppSettings["localUPdateConfig"];
        public static string LocalUPdateConfig = Path.Combine(currentFolder, "updateconfig.xml");

        /// <summary>
        /// 版本号
        /// </summary>
        public static string Version = ConfigurationManager.AppSettings["version"];

        /// <summary>
        /// 更新程序路径
        /// </summary>
        // public static string LocalPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string LocalPath = @currentFolder + "\\";

        /// <summary>
        /// 主程序路径
        /// </summary>
        // public static string MainPath = ConfigurationManager.AppSettings["mainPath"];
        public static string MainPath = currentFolder;

        /// <summary>
        /// 有否启动主程序
        /// </summary>
        public static bool IsRunning = false;

        /// <summary>
        /// 主程序名
        /// </summary>
        public static List<string> AppNames = appName.Split(';').ToList();
    }
}