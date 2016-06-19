using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.Utilities
{
    public class Utils
    {

        public static String getApplicationEXEFolderPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string FormatUptime(Int32 secs)
        {
            return FormatUptime(TimeSpan.FromSeconds(secs));
        }

        public static string FormatUptime(TimeSpan t)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", t.TotalDays, t.Hours, t.Minutes, t.Seconds);
        }

        public static void writeEncrypted(string data, string path)
        {

            byte[] encrypted;
            byte[] IV;
            byte[] key;

            using (Aes aes = Aes.Create())
            {
                IV = aes.IV;
                key = aes.Key;

                ICryptoTransform cryptor = aes.CreateEncryptor(key, IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(data);
                        }
                        encrypted = memoryStream.ToArray();

                        List<byte[]> bytes = new List<byte[]> { IV, key, encrypted };
                        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                        {
                            foreach (byte[] b in bytes)
                            {
                                fs.WriteByte((byte)1);
                                fs.WriteByte((byte)b.Length);
                                fs.Write(b, 0, b.Length);
                            }
                            fs.WriteByte((byte)0);
                            fs.Flush(true);
                        }
                    }
                }
            }

        }

        public static string readEncrypted(string path)
        {

            List<byte[]> bytes = new List<byte[]>();
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                int b;
                while ((b = fs.ReadByte()) != 0)
                {
                    int len = fs.ReadByte();
                    byte[] @in = new byte[len];
                    fs.Read(@in, 0, len);
                    bytes.Add(@in);
                }
            }
            string decrypted;
            using (Aes aes = Aes.Create())
            {

                aes.IV = bytes[0];
                aes.Key = bytes[1];

                ICryptoTransform cryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(bytes[2]))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            decrypted = streamReader.ReadToEnd();
                        }
                    }
                }

            }
            return decrypted;

        }


        internal static void createDirectoriesIfNotExists(string basePath, string[] directoryNames, Logging.Logger logger = null)
        {
            foreach (string dN in directoryNames) 
            {
                string path = Path.Combine(basePath, dN);
                if (Directory.Exists(path))
                {
                    if (logger != null)
                        logger.Log("Directory '{0}' exists.", path);
                    continue;
                }
                if (logger != null)
                    logger.Log("Directory '{0}' has been created.", path);
                Directory.CreateDirectory(path);
            }
        }
    }
}
