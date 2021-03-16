﻿using System;
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
        public static List<FileENT> GetUpdateList()
        {
            List<FileENT> list = new List<FileENT>();

            XmlDocument xml = new XmlDocument();
            // XML加载本地更新配置文件
            xml.Load(AppParameter.LocalUPdateConfig);
            // 此处获取结果实例："/updateFiles/file[@version>27]"
            XmlNodeList nodeList = xml.SelectNodes("/updateFiles/file[@version>" + AppParameter.Version + "]");
            
            FileENT ent = null;
            foreach (XmlNode node in nodeList)
            {
                ent = new FileENT();
                // 文件全名，示例："Smart.FormDesigner.Demo.exe"
                ent.FileFullName = node.Attributes["name"].Value;
                // 服务器路径，示例："ftp://192.168.2.113/"
                ent.Src = node.Attributes["src"].Value;
                // 版本号，示例："28"
                ent.Version = node.Attributes["version"].Value;
                // 文件大小，多线程用不到
                ent.Size =Convert.ToInt32( node.Attributes["size"].Value);
                // 可选参数
                ent.Option = (UpdateOption)Enum.Parse(typeof(UpdateOption), node.Attributes["option"].Value);


                list.Add(ent);
            }

            return list;
        }

        public static void UpdateAppConfig(string key,string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
