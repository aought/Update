using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MyUpdate.Entity;
using System.Configuration;

namespace MyUpdate.Utils
{
    public class ConfigHelper
    {
        /// <summary>
        /// 解析xml配置文件
        /// </summary>
        /// <returns></returns>
        public static List<FtpRemoteFile> ParseXmlFileList()
        {
            List<FtpRemoteFile> list = new List<FtpRemoteFile>();

            XmlDocument xml = new XmlDocument();
            // XML加载本地更新配置文件
            xml.Load(AppParameter.LocalUPdateConfig);
            // TODO
            // xml.Load(AppParameter.oldConfig);
            // 此处获取结果实例："/updateFiles/file[@version>27]"
            // 此时获取到：AppParameter.Version	"13"
            // 返回 string.Concat	"/updateFiles/file[@version>13]"
            // 此时是获取版本号大于Config中存储的版本号的文件，Count是用来计数有几个文件的；
            // Count=0就意味着没有文件大于该版本号，所以不会更新；
            XmlNodeList nodeList = xml.SelectNodes("/updateFiles/file[@version>" + AppParameter.Version + "]");
            
            FtpRemoteFile ent = null;
            foreach (XmlNode node in nodeList)
            {
                ent = new FtpRemoteFile();
                // 文件全名，示例："Smart.FormDesigner.Demo.exe"
                ent.FileFullName = node.Attributes["name"].Value;
                // 服务器路径，示例："ftp://192.168.2.113/"
                ent.Src = node.Attributes["src"].Value;
                // 版本号，示例："28"
                ent.Version = node.Attributes["version"].Value;
                // 文件大小，多线程用不到
                ent.Size =Convert.ToInt32( node.Attributes["size"].Value);
                //hash
                ent.Hash = node.Attributes["hash"].Value;
                // 可选参数
                ent.Option = (UpdateOption)Enum.Parse(typeof(UpdateOption), node.Attributes["option"].Value);

                list.Add(ent);
            }
            
            return list;
        }

        /// <summary>
        /// 覆盖本地配置信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void RecoverAppConfig(string key,string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        // 获取最低版本号
        public static int GetVersion()
        {
            int version = 0;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(AppParameter.LocalUPdateConfig);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("updateFiles");
            XmlNodeList xmlNodeList = xmlNode.ChildNodes;
            foreach (XmlNode singleXmlNode in xmlNodeList)
            {
                version = Convert.ToInt32(singleXmlNode.Attributes["version"].Value);
                int tempVersion = Convert.ToInt32(singleXmlNode.Attributes["version"].Value);
                if (tempVersion < version)
                {
                    version = tempVersion;
                }
            }

            return version;
        }
    }
}
