using Baidu.Aip.Nlp;
using Baidu.Aip.Speech;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Framework
{
    public class BaiduSDK
    {
        public static Tts tts;
        public static Nlp nlp;
        public static string mp3Path = ConfigAppSettings.GetValue("mp3Path");
        public static string mp3Url = ConfigAppSettings.GetValue("mp3Url");
        public static string mp3Fail = ConfigAppSettings.GetValue("mp3Fail");

        /// <summary>
        /// 语音合成1024字节最大
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Tts(string text)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    string url = "";
                    if (text.Contains('{'))
                    {
                        //int uu = text.IndexOf("url\":\"");
                        //if (uu > 0)
                        //{
                        //    text = text.Substring(uu + 6, text.Length - uu - 6);
                        //    int yy = text.IndexOf('\"');
                        //    url = text.Substring(0, yy);
                        //    return url;//还有不是MP3的url
                        //}
                        //else
                        //{
                        //    return null;
                        //}
                        return null;//json字符串的不生成url
                    }
                    else
                    {
                        if (tts == null)
                        {
                            string APP_ID = "14884741";
                            string API_KEY = "UNBsgAYhfLciqDsWRApMmSTn";
                            string SECRET_KEY = "bLpvCoLguv8Xg04c6E8W8LVgwluC4GC4";
                            tts = new Tts(API_KEY, SECRET_KEY);
                            tts.Timeout = 60000;  // 修改超时时间
                        }

                        // 可选参数
                        var option = new Dictionary<string, object>()
                    {
                        {"spd", 6}, // 语速
                        {"vol", 5}, // 音量
                        {"per", 4}  // 发音人，发音人选择, 0为普通女声，1为普通男生，2为普通男生，3为情感合成-度逍遥，4为情感合成-度丫丫，默认为普通女声
                    };
                        var result = tts.Synthesis(text, option);
                        string path = "";
                        if (result.ErrorCode == 0)  // 或 result.Success
                        {
                            string filename = DateTime.Now.ToString("HHmmss-fff") + ".mp3";
                            path = $"{ mp3Path}{ DateTime.Now.Date.ToString("yyyyMMdd")}\\{filename}";//C:\IIS\MP3\20181216\150225-266.mp3
                            if (!Directory.Exists($"{ mp3Path}{ DateTime.Now.Date.ToString("yyyyMMdd")}"))
                            {
                                Directory.CreateDirectory($"{ mp3Path}{ DateTime.Now.Date.ToString("yyyyMMdd")}");
                            }
                            File.WriteAllBytes(path, result.Data);

                            url = $"{mp3Url}{ DateTime.Now.Date.ToString("yyyyMMdd")}/{filename}";//http://47.107.66.121:8044/20181216/150225-266.mp3

                        }
                        return url;
                    }


                }
                else
                {
                    return null;
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary>
        /// 分词
        /// </summary>
        public static void Nlp(string text)
        {
            try
            {
                if (nlp == null)
                {
                    string APP_ID = "14902717";
                    string API_KEY = "1qHCYskEsmQyMYYwGa2b4RI9";
                    string SECRET_KEY = "34uRO8hYapy7OTKGzuEDK3EeLyDsZOMt";
                    nlp = new Nlp(API_KEY, SECRET_KEY);
                    nlp.Timeout = 60000;  // 修改超时时间
                }

                // 调用词法分析，可能会抛出网络等异常，请使用try/catch捕获
                var result = nlp.Lexer(text);
                BaiduNlp nlpEntity = JsonConvert.DeserializeObject<BaiduNlp>(result.ToString());
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
    public class BaiduNlpItem
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> loc_details { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int byte_offset { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uri { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ne { get; set; }
        /// <summary>
        /// 百度
        /// </summary>
        public string item { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> basic_words { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int byte_length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string formal { get; set; }
    }

    public class BaiduNlp
    {
        /// <summary>
        /// 
        /// </summary>
        public long log_id { get; set; }
        /// <summary>
        /// 百度是一家高科技公司
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<BaiduNlpItem> items { get; set; }
    }
}
