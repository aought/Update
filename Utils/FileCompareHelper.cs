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

        /// <summary>
        /// 获得文件Hash值
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string HashFile(string filepath)
        {
            using (HashAlgorithm hash = HashAlgorithm.Create())
            {
                using (FileStream firstStream = new FileStream(filepath, FileMode.Open))
                {
                    byte[] firstHashByte = hash.ComputeHash(firstStream);
                    string firstString = BitConverter.ToString(firstHashByte);
                    return firstString;
                }
            }
        }

        public static string SHA256File(string filepath)
        {
            string hash = null;
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filepath))
                {
                    byte[] bytes = sha256.ComputeHash(fileStream);
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    hash = builder.ToString().ToUpper();
                }
            }
            return hash;
        }
    }
}
