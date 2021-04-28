using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyUpdate.Entity
{
    public class FtpRemoteFile
    {
        /// <summary>
        /// 主程序全名（即带exe，不带路径）；
        /// </summary>
        public string FileFullName { get; set; }
        /// <summary>
        /// 服务器路径
        /// </summary>
        public string Src { get; set; }

        public string Version { get; set; }

        public int Size { get; set; }

        public string Hash { get; set; }

        public UpdateOption Option { get; set; }
    }

}
