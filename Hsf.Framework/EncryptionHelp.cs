using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Framework
{

    public class EncryptionHelp
    {
        /// <summary>
        /// 加密Base64
        /// </summary>
        /// <param name="str"></param>
        /// <param name="iszip"></param>
        /// <returns></returns>
        public static string Encryption(string str, bool iszip)
        {
            if (iszip)
            {
                var compressBeforeByte = Encoding.GetEncoding("UTF-8").GetBytes(str);
                var compressAfterByte = Compress(compressBeforeByte);
                string compressString = Convert.ToBase64String(compressAfterByte);
                return compressString;
            }
            else
            {
                byte[] plaindata = Encoding.UTF8.GetBytes(str);//将要加密的字符串转换为字节数组
                return Convert.ToBase64String(plaindata);//将加密后的字节数组转换为字符串
            }

        }
        /// <summary>
        /// 解密Base64
        /// </summary>
        /// <param name="str"></param>
        /// <param name="iszip"></param>
        /// <returns></returns>
        public static string Decrypt(string str, bool iszip)
        {
            if (iszip)
            {
                var compressBeforeByte = Convert.FromBase64String(str);
                var compressAfterByte = Decompress(compressBeforeByte);
                string compressString = Encoding.GetEncoding("UTF-8").GetString(compressAfterByte);
                return compressString;
            }
            else
            {
                byte[] encryptdata = Convert.FromBase64String(str);
                return Encoding.UTF8.GetString(encryptdata);
            }
        }

        /// <summary>
        /// zip压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] Compress(byte[] data)
        {
            try
            {
                var ms = new MemoryStream();
                var zip = new GZipStream(ms, CompressionMode.Compress, true);
                zip.Write(data, 0, data.Length);
                zip.Close();
                var buffer = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(buffer, 0, buffer.Length);
                ms.Close();
                return buffer;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// zip解压
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] Decompress(byte[] data)
        {
            try
            {
                var ms = new MemoryStream(data);
                var zip = new GZipStream(ms, CompressionMode.Decompress, true);
                var msreader = new MemoryStream();
                var buffer = new byte[0x1000];
                while (true)
                {
                    var reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }
                    msreader.Write(buffer, 0, reader);
                }
                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }





        ////加密
        //public static string Encryption(string express)
        //{
        //    byte[] plaindata = Encoding.UTF8.GetBytes(express);//将要加密的字符串转换为字节数组
        //    return Convert.ToBase64String(plaindata);//将加密后的字节数组转换为字符串
        //    //CspParameters param = new CspParameters();
        //    //param.KeyContainerName = "yicheng";//密匙容器的名称，保持加密解密一致才能解密成功
        //    //using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
        //    //{
        //    //    byte[] plaindata = Encoding.Default.GetBytes(express);//将要加密的字符串转换为字节数组
        //    //    byte[] encryptdata = rsa.Encrypt(plaindata, false);//将加密后的字节数据转换为新的加密字节数组
        //    //    return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为字符串
        //    //}
        //}

        ////解密
        //public static string Decrypt(string ciphertext)
        //{
        //    byte[] encryptdata = Convert.FromBase64String(ciphertext);
        //    return Encoding.UTF8.GetString(encryptdata);
        //    //CspParameters param = new CspParameters();
        //    //param.KeyContainerName = "yicheng";
        //    //using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
        //    //{
        //    //    byte[] encryptdata = Convert.FromBase64String(ciphertext);
        //    //    byte[] decryptdata = rsa.Decrypt(encryptdata, false);
        //    //    return Encoding.Default.GetString(decryptdata);
        //    //}
        //}

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验数据</param>
        /// <returns>高低8位</returns>
        public static string CRCCalc(string data)
        {
            string[] datas = data.Split(' ');
            List<byte> bytedata = new List<byte>();

            foreach (string str in datas)
            {
                bytedata.Add(byte.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            byte[] crcbuf = bytedata.ToArray();
            //计算并填写CRC校验码
            int crc = 0xffff;
            int len = crcbuf.Length;
            for (int n = 0; n < len; n++)
            {
                byte i;
                crc = crc ^ crcbuf[n];
                for (i = 0; i < 8; i++)
                {
                    int TT;
                    TT = crc & 1;
                    crc = crc >> 1;
                    crc = crc & 0x7fff;
                    if (TT == 1)
                    {
                        crc = crc ^ 0xa001;
                    }
                    crc = crc & 0xffff;
                }

            }
            string[] redata = new string[2];
            redata[1] = Convert.ToString((byte)((crc >> 8) & 0xff), 16);
            redata[0] = Convert.ToString((byte)((crc & 0xff)), 16);
            return redata[0] + " " + redata[1];
        }
    }
}
