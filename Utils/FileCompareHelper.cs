using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyUpdate.Utils
{
    public class FileCompareHelper
    {
        public static bool FileCompare(string firstFile, string secondFile)
        {
            // 采用哈希方式比对文件
            if (!File.Exists(firstFile) || !File.Exists(secondFile))
            {
                return false;
            }
            if (firstFile == secondFile)
            {
                return true;
            }
            using (HashAlgorithm hash = HashAlgorithm.Create())
            {
                using (FileStream firstStream = new FileStream(firstFile, FileMode.Open),
                      secondStream = new FileStream(secondFile, FileMode.Open))
                {
                    byte[] firstHashByte = hash.ComputeHash(firstStream);
                    byte[] secondHashByte = hash.ComputeHash(secondStream);
                    string firstString = BitConverter.ToString(firstHashByte);
                    string secondString = BitConverter.ToString(secondHashByte);
                    return (firstString == secondString);
                }
            }
        }
    }
}
