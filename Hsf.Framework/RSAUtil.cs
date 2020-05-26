using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Encodings;

namespace Hsf.Framework
{
    /// <summary>
    /// RSA签名工具类。
    /// </summary>
    public class RSAUtil
    {

        /// <summary>
        /// java公钥转C#所需公钥
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        public static string RSAEncryptMore(string xmlPublicKey, string m_strEncryptString)
        {
            if (string.IsNullOrEmpty(m_strEncryptString))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(xmlPublicKey))
            {
                throw new ArgumentException("Invalid Public Key");
            }

            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                var inputBytes = Encoding.UTF8.GetBytes(m_strEncryptString);//有含义的字符串转化为字节流
                rsaProvider.FromXmlString(xmlPublicKey);//载入公钥
                int bufferSize = (rsaProvider.KeySize / 8) - 11;//单块最大长度
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes),
                     outputStream = new MemoryStream())
                {
                    while (true)
                    { //分段加密
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }

                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var encryptedBytes = rsaProvider.Encrypt(temp, false);
                        outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                    return Convert.ToBase64String(outputStream.ToArray());//转化为字节流方便传输
                }
            }
        }





        #region  加密  
        /// <summary>  
        /// RSA加密  
        /// </summary>  
        /// <param name="publicKeyJava"></param>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static string EncryptJava(string publicKeyJava, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //byte[] cipherbytes;
            rsa.FromPublicKeyJavaString(publicKeyJava);

            //☆☆☆☆.NET 4.6以后特有☆☆☆☆  
            //HashAlgorithmName hashName = new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm);  
            //RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA512;//RSAEncryptionPadding.CreateOaep(hashName);//.NET 4.6以后特有                 
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);  
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆  

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆  
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);

            using (var rsaProvider = new RSACryptoServiceProvider())
            {
                var inputBytes = Encoding.UTF8.GetBytes(data);//有含义的字符串转化为字节流                              
                int bufferSize = (rsa.KeySize / 8) - 11;//单块最大长度
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes),
                     outputStream = new MemoryStream())
                {
                    while (true)
                    { //分段加密
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }

                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var encryptedBytes = rsaProvider.Encrypt(temp, false);
                        outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                    return Convert.ToBase64String(outputStream.ToArray());//转化为字节流方便传输
                }
            }
        }
        /// <summary>  
        /// RSA加密  
        /// </summary>  
        /// <param name="publicKeyCSharp"></param>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static string EncryptCSharp(string publicKeyCSharp, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publicKeyCSharp);

            //☆☆☆☆.NET 4.6以后特有☆☆☆☆  
            //HashAlgorithmName hashName = new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm);  
            //RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA512;//RSAEncryptionPadding.CreateOaep(hashName);//.NET 4.6以后特有                 
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);  
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆  

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆  
            cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>  
        /// RSA加密PEM秘钥  
        /// </summary>  
        /// <param name="publicKeyPEM"></param>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static string EncryptPEM(string publicKeyPEM, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.LoadPublicKeyPEM(publicKeyPEM);

            //☆☆☆☆.NET 4.6以后特有☆☆☆☆  
            //HashAlgorithmName hashName = new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm);  
            //RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA512;//RSAEncryptionPadding.CreateOaep(hashName);//.NET 4.6以后特有                 
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);  
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆  

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆  
            cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);

            return Convert.ToBase64String(cipherbytes);
        }
        #endregion

        /// <summary>
        /// 解密公钥
        /// </summary>
        /// <param name="s"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <summary>
        /// 解密公钥
        /// </summary>
        /// <param name="s"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptByPublicKey(string s, string key)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            //非对称加密算法，加解密用  
            IAsymmetricBlockCipher engine = new Pkcs1Encoding(new RsaEngine());
            //解密  
            try
            {
                engine.Init(false, GetPublicKeyParameter(key));
                byte[] byteData = Convert.FromBase64String(s);
                var ResultData = engine.ProcessBlock(byteData, 0, byteData.Length);
                return System.Text.Encoding.UTF8.GetString(ResultData);
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }

        //#region 私有属性
        ////private RSAParameters RSAKeyInfo;
        //private static RSACryptoServiceProvider RSA = null;
        ////私钥
        //private const string NET_PRIVATE_KEY = @"<RSAKeyValue><Modulus>ndSLc+4nW6DJbZKjs+UrQynUjxca1IPOIyfcZxPB7lpEQFUJWwpN+hDabWdVeFldNhaNSFg1UlQz4N2wPR030ui62ayyD66yEm0KCvAUOfw0fVhiEf/5CmoLSz+co6fAYvCf5GymwB0fjziiIorNvmZiAJyBNrm4JLbbvsoNDIU=</Modulus><Exponent>AQAB</Exponent><P>zS4nps270U327EPDQjcCQVQXSnOQILtJyiH8V0QoImQpT6a1dhFwLfe/bl/3L7nBr3PLk9nkPMtUdwXnZ6lrcQ==</P><Q>xOwSJfUODzVETrMc2D2947krqcR+XYubvPIsiDyeYqqMFQMYA+ONZKoExn3o1tb1ORvunTApH2d/f5qq6aJgVQ==</Q><DP>vwHio+QOnrDn19bVZUT0coCoFgUy/WWdMfElis/GVQ3Nb3sQntNpDUIAEe6AnQtehclUkVVcpkPbY9o5LEWJ4Q==</DP><DQ>JB0zOtjVSj63l0NL7/Bqyb+k3U6W6ir3VdCIEDglx+yFIjleByCNRr/Tfl+K+xOTB3Uy7ortj7/YZxuDarOHvQ==</DQ><InverseQ>Ueugp68z1cKJXLXSFz/LRJNd+uh4vVOBt6ndBtmJ+H4gI0JgBoL8QmR5X1iiD7v9LD+5cJng5k4uriil6cAeFw==</InverseQ><D>InIDqV59inrR2y8YuSc3xOW5NS1mtqC5eWS2rmxac8mRgbTNYOgj0oKhGSVnOufN9wL+/J37rSchV18qmnvo9bABSEMYNlTkViTgmAWdU3sIXa8EmFVS6sf6Ba+SBTYQLv8PyzxWXU3aXFdLGvU/WIY2QRYtIIL/mHsLrw3/p0E=</D></RSAKeyValue>";
        ////公钥参数
        //private const string PUB_KEY_MODULES = @"1lpnLvumD8/NedJ7s4WS8UO9OORbXVTgJXmfa72bI4A1L1l6Np91BETQ+yB8Fq6iGWw5OR8OB2UbRBcopb2etepDqWd7kmCtbVT36kTW+E8dWdaVjbI2BCXEGaXuzPPdGOlp52OaawYR5zyG0MiCvJ4jE7RDJax4Cl24ZqPUs4U=";
        ////公钥参数
        //private const string PUB_KEY_EXP = @"AQAB";
        //public RSAUtil()
        //{
        //}
        //#endregion
        //#region 私有方法
        ////取大头的数据
        //private static ushort readBEUInt16(BinaryReader br)
        //{
        //    byte[] b = br.ReadBytes(2);
        //    byte[] rb = new byte[2];
        //    rb[0] = b[1];
        //    rb[1] = b[0];
        //    return BitConverter.ToUInt16(rb, 0);
        //}
        //private static bool equalBytes(byte[] first, byte[] second)
        //{
        //    try
        //    {
        //        if (first.Length != second.Length)
        //        {
        //            return false;
        //        }
        //        for (int i = 0; i < first.Length; i++)
        //        {
        //            if (first[i] != second[i])
        //            {
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return false;
        //    }
        //}
        //private static int getHead(BinaryReader br, byte elementFlag)
        //{
        //    try
        //    {
        //        int count = 0;
        //        byte bt = 0;
        //        bt = br.ReadByte();
        //        if (elementFlag != 0x00 && bt != elementFlag)
        //        {
        //            throw (new Exception("pem format err,element head : " + bt + " != " + elementFlag));
        //        }
        //        count = getElementLen(br);
        //        return count;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return -1;
        //    }
        //}
        //private static int getElementLen(BinaryReader br)
        //{
        //    try
        //    {
        //        ushort count = 0;
        //        byte bt = 0;
        //        bt = br.ReadByte();
        //        if (bt == 0x81)
        //        {
        //            count = br.ReadByte();
        //        }
        //        else if (bt == 0x82)
        //        {
        //            count = readBEUInt16(br); ;
        //        }
        //        else
        //        {
        //            count = bt;
        //        }
        //        return (int)count;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return -1;
        //    }
        //}
        //private static byte[] loadBytesFromPemFile(String fileName)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    using (StreamReader sr = new StreamReader(fileName))
        //    {
        //        String line;
        //        do
        //        {
        //            line = sr.ReadLine();
        //        } while (line != null && (line.Length == 0 || line.Substring(0, 1) != "-"));
        //        do
        //        {
        //            line = sr.ReadLine();
        //        } while (line != null && (line.Length == 0 || line.Substring(0, 1) == "-"));
        //        while (line != null && (line.Length == 0 || line.Substring(0, 1) != "-"))
        //        {
        //            sb.Append(line);
        //            line = sr.ReadLine();
        //        }
        //    }
        //    //Response.Write("base64:" + sb.ToString() + "<br>\n");
        //    return Convert.FromBase64String(sb.ToString());
        //}
        //private static byte[] stripLeftZeros(byte[] a)
        //{
        //    int lastZero = -1;
        //    for (int i = 0; i < a.Length; i++)
        //    {
        //        if (a[i] == 0)
        //        {
        //            lastZero = i;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    lastZero++;
        //    byte[] result = new byte[a.Length - lastZero];
        //    Array.Copy(a, lastZero, result, 0, result.Length);
        //    return result;
        //}
        //private static byte[] getElement(BinaryReader br, byte elementFlag)
        //{
        //    try
        //    {
        //        int count = 0;
        //        byte bt = 0;
        //        bt = br.ReadByte();
        //        if (elementFlag != 0x00 && bt != elementFlag)
        //        {
        //            throw (new Exception("pem format err,element head : " + bt + " != " + elementFlag));
        //        }
        //        count = getElementLen(br);
        //        byte[] value = stripLeftZeros(br.ReadBytes(count));
        //        return value;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return null;
        //    }
        //}
        //#endregion
        //#region 公有方法
        ///// <summary>
        ///// 通过私key文件 获取RSAParameters
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public static RSAParameters getPrivateKeyFromPem(String fileName)
        //{
        //    byte[] keyBytes = loadBytesFromPemFile(fileName);
        //    RSAParameters para = new RSAParameters();
        //    BinaryReader br = new BinaryReader(new MemoryStream(keyBytes));
        //    byte bt = 0;
        //    ushort twoBytes = 0;
        //    twoBytes = readBEUInt16(br);
        //    if (twoBytes == 0x3081)
        //    {
        //        br.ReadByte();
        //    }
        //    else if (twoBytes == 0x3082)
        //    {
        //        br.ReadInt16();
        //    }
        //    else
        //    {
        //        throw (new Exception("pem format err,head 1: " + twoBytes + " != 0x3081 or 0x3082," + 0x3082));
        //    }
        //    twoBytes = readBEUInt16(br);
        //    bt = br.ReadByte();
        //    if (twoBytes != 0x0201 || bt != 0x00)
        //    {
        //        throw (new Exception("pem format err,head 2: " + twoBytes + " != 0x0201 or " + bt + " != 0x00"));
        //    }
        //    para.Modulus = getElement(br, 0x02);
        //    para.Exponent = getElement(br, 0x02);
        //    para.D = getElement(br, 0x02);
        //    para.P = getElement(br, 0x02);
        //    para.Q = getElement(br, 0x02);
        //    para.DP = getElement(br, 0x02);
        //    para.DQ = getElement(br, 0x02);
        //    para.InverseQ = getElement(br, 0x02);
        //    if (para.Equals(""))
        //    {
        //        throw (new Exception("pem format err,para=null!"));
        //    }
        //    return para;
        //}
        ///// <summary>
        ///// 通过公key文件 获取RSAParameters
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public static RSAParameters getPublicKeyFromPem(String fileName)
        //{
        //    byte[] keyBytes = loadBytesFromPemFile(fileName);
        //    RSAParameters para = new RSAParameters();
        //    BinaryReader br = new BinaryReader(new MemoryStream(keyBytes));
        //    byte bt = 0;
        //    ushort twoBytes = 0;
        //    //两个30开头的Sequence
        //    getHead(br, 0x30);
        //    getHead(br, 0x30);
        //    //{ 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 }
        //    byte[] correctOid = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
        //    byte[] oid = getElement(br, 0x06);
        //    if (!equalBytes(correctOid, oid))
        //    {
        //        throw (new Exception("pem format err,oid err"));
        //    }
        //    bt = br.ReadByte();
        //    //05 00
        //    if (bt == 0x05)
        //    {
        //        br.ReadByte();
        //    }
        //    else
        //    {
        //        //已经获取了一个字节，只能调用两个函数组合，不能用getElement
        //        int len = getElementLen(br);
        //        br.ReadBytes(len);
        //    }
        //    //03开头的BitString，03+len+00
        //    getHead(br, 0x03);
        //    br.ReadByte();
        //    //30开头的Sequence
        //    getHead(br, 0x30);
        //    para.Modulus = getElement(br, 0x02);
        //    para.Exponent = getElement(br, 0x02);
        //    if (para.Equals(""))
        //    {
        //        throw (new Exception("pem format err,para=null!"));
        //    }
        //    return para;
        //}
        //public static bool verifySignature(byte[] signature, string signedData, RSAParameters pubPara)
        //{
        //    try
        //    {
        //        RSA = new RSACryptoServiceProvider();
        //        RSAParameters RSAParams = RSA.ExportParameters(false);
        //        RSACryptoServiceProvider RSA2 = new RSACryptoServiceProvider();
        //        //RSA2.ImportParameters(priPara)
        //        RSA2.ImportParameters(pubPara);
        //        byte[] hash = Encoding.UTF8.GetBytes(signedData);
        //        if (RSA2.VerifyData(hash, "SHA1", signature))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return false;
        //    }
        //}
        ///// <summary>
        ///// 验证签名数据
        ///// </summary>
        ///// <param name="signature">秘钥</param>
        ///// <param name="signedData">明文</param>
        ///// <param name="pubFileName">公钥文件</param>
        ///// <returns></returns>
        //public static bool verifySignature(string signature, string signedData, string pubFileName)
        //{
        //    RSAParameters pubPara;
        //    pubPara = getPublicKeyFromPem(pubFileName);
        //    byte[] sign = Convert.FromBase64String(signature);
        //    //Convert.FromBase64String(signature);
        //    return verifySignature(sign, signedData, pubPara);
        //}
        ///// <summary>
        ///// 数据签名
        ///// </summary>
        ///// <param name="dataToBeSigned">需要加密的字符串</param>
        ///// <param name="priFileName">私钥文件</param>
        ///// <returns></returns>
        //public static string signData(string dataToBeSigned, string priFileName)
        //{
        //    RSAParameters priPara;
        //    priPara = getPrivateKeyFromPem(priFileName);
        //    RSA = new RSACryptoServiceProvider();
        //    //RSA.FromXmlString(NET_PRIVATE_KEY);
        //    RSAParameters RSAParams = RSA.ExportParameters(false);
        //    RSACryptoServiceProvider RSA2 = new RSACryptoServiceProvider();
        //    RSA2.ImportParameters(priPara);
        //    byte[] data = Encoding.UTF8.GetBytes(dataToBeSigned);
        //    byte[] endata = RSA2.SignData(data, "SHA1");
        //    return Convert.ToBase64String(endata);
        //}
        ///// <summary>
        ///// 数据加密
        ///// </summary>
        ///// <param name="dataSigned"></param>
        ///// <param name="pubFileName"></param>
        ///// <returns></returns>
        //public static string RSAEncrypt(string dataSign, string publicFileName)
        //{
        //    RSAParameters priPara;
        //    string hyxfmes = "";
        //    priPara = getPublicKeyFromPem(publicFileName);
        //    try
        //    {
        //        RSA = new RSACryptoServiceProvider();
        //        RSAParameters RSAParams = RSA.ExportParameters(false);
        //        RSACryptoServiceProvider RSA2 = new RSACryptoServiceProvider();
        //        RSA2.ImportParameters(priPara);
        //        byte[] hash = Encoding.UTF8.GetBytes(dataSign);
        //        byte[] de = RSA2.Encrypt(hash, false);
        //        hyxfmes = Convert.ToBase64String(de, Base64FormattingOptions.None);
        //        return hyxfmes;
        //    }
        //    catch (Exception e)
        //    {
        //        return "数据加密失败！";
        //    }
        //}
        ///// <summary>
        ///// 数据解密
        ///// </summary>
        ///// <param name="dataSigned"></param>
        ///// <param name="pubFileName"></param>
        ///// <returns></returns>
        //public static string RSADecrypt(string dataSigned, string privateFileName)
        //{
        //    RSAParameters pubPara;
        //    pubPara = getPrivateKeyFromPem(privateFileName);
        //    try
        //    {
        //        RSA = new RSACryptoServiceProvider();
        //        RSAParameters RSAParams = RSA.ExportParameters(false);
        //        RSACryptoServiceProvider RSA2 = new RSACryptoServiceProvider();
        //        RSA2.ImportParameters(pubPara);
        //        byte[] hash = Convert.FromBase64String(dataSigned);
        //        byte[] de = RSA2.Decrypt(hash, false);
        //        return Encoding.UTF8.GetString(de);
        //    }
        //    catch (Exception e)
        //    {
        //        return e.ToString();
        //    }
        //}
        //#endregion
        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static AsymmetricKeyParameter GetPublicKeyParameter(string s)
        {
            s = s.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            byte[] publicInfoByte = Convert.FromBase64String(s);
            Asn1Object pubKeyObj = Asn1Object.FromByteArray(publicInfoByte);//这里也可以从流中读取，从本地导入
            AsymmetricKeyParameter pubKey = PublicKeyFactory.CreateKey(publicInfoByte);
            return pubKey;
        }
    }
}