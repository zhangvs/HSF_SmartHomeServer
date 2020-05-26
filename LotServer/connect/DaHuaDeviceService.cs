using Hsf.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer.connect
{
    public class DaHuaDeviceService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("DaHuaDeviceService");

        #region 单元门
        /// <summary>
        /// 大华单元门口机
        /// </summary>
        /// <param name="devmac">设备编号大华平台</param>
        /// <returns>
        ///{"status":"1","resultMessage":"信息发送成功"}
        ///{"status":"0","resultMessage":"设备不存在"}
        ///var result2 = client.openDoor("{\"deviceCode\":\"1000001\"} ");
        ///var result3 = client.getRoomNumByPhone("1000033");
        /// </returns>
        public static bool UnitDoorSendMsg(string devmac)
        {
            using (DaHuaService.MobPhoneServiceClient client = new DaHuaService.MobPhoneServiceClient())
            {
                string sendmsg = "{\"deviceCode\":\"" + devmac + "\"}";
                string result1 = client.openDoor(sendmsg);
                log.Debug($"向大华平台发送指令{sendmsg}，返回结果{result1}");
                if (result1.Contains("1"))
                {
                    return true; 
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region 门禁
        /// <summary>
        /// 大华门禁
        /// </summary>
        /// <param name="devmac">设备编号大华平台</param>
        /// <returns></returns>
        public static bool EntranceGuardSendMsg(string devmac)
        {
            string st = PostOpenDoor(_token, devmac);//{"code":403,"data":"http://192.168.1.108:80/WPMS","errMsg":"login.timeout","success":false}
            if (string.IsNullOrEmpty(st))
            {
                GetLoginToken();
                string stt = PostOpenDoor(_token, devmac);//{"errMsg":"success","success":true}
                if (stt.Contains("true"))
                {
                    return true;
                }
            }
            else
            {
                if (st.Contains("403"))
                {
                    GetLoginToken();
                    string stt = PostOpenDoor(_token, devmac);//{"errMsg":"success","success":true}
                    if (stt.Contains("true"))
                    {
                        return true;
                    }
                }
                if (st.Contains("true"))
                {
                    return true;
                }
            }
            return false;
        }

        #region 设备操作接口
        /// <summary>
        /// 开门
        /// </summary>
        private static string PostOpenDoor(string token,string devmac)
        {
            if (!string.IsNullOrEmpty(token))//LoginUser.token
            {
                string url = $"http://192.168.1.108/CardSolution/card/accessControl/channelControl/openDoor?userId=1&userName=system&token={ token }&orgCode=001";
                var postData = "{\"channelCodeList\": [\""+devmac+"\"]}";
                return HttpUtil.PostUtil(url, postData);
            }
            return null;
        }

        #region 不启用
        ///// <summary>
        ///// 关门
        ///// </summary>
        //private static string PostCloseDoor(string token, string devmac)
        //{
        //    if (!string.IsNullOrEmpty(token))//LoginUser.token
        //    {
        //        string url = $"http://192.168.1.108/CardSolution/card/accessControl/channelControl/closeDoor?userId=1&userName=system&token={ token }&orgCode=001";
        //        var postData = "{\"channelCodeList\": [\"" + devmac + "\"]}";
        //        return HttpUtil.PostUtil(url, postData);
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// 常开
        ///// </summary>
        //private static string PoststayOpenDoor(string token, string devmac)
        //{
        //    if (!string.IsNullOrEmpty(token))//LoginUser.token
        //    {
        //        string url = $"http://192.168.1.108/CardSolution/card/accessControl/channelControl/stayOpen?userId=1&userName=system&token={ token }&orgCode=001";
        //        var postData = "{\"channelCodeList\": [\"" + devmac + "\"]}";
        //        return HttpUtil.PostUtil(url, postData);
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// 常关
        ///// </summary>
        //private static string PoststayCloseDoor(string token, string devmac)
        //{
        //    if (!string.IsNullOrEmpty(token))//LoginUser.token
        //    {
        //        string url = $"http://192.168.1.108/CardSolution/card/accessControl/channelControl/stayClose?userId=1&userName=system&token={ token }&orgCode=001";
        //        var postData = "{\"channelCodeList\": [\"" + devmac + "\"]}";
        //        return HttpUtil.PostUtil(url, postData);
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// 设备查询
        ///// </summary>
        //private static string PostDeviceSearch(string token)
        //{
        //    if (!string.IsNullOrEmpty(token))//LoginUser.token
        //    {
        //        string url = $"http://192.168.1.108/CardSolution/card/accessControl/device/bycondition/combined?userId=1&userName=system&token={ token }&orgCode=001";
        //        var postData = "{\"pageSize \": 20}";
        //        return HttpUtil.PostUtil(url, postData);
        //    }
        //    return null;
        //}


        ///// <summary>
        ///// 设备状态查询
        ///// </summary>
        //private static string PostDeviceStateSearch(string token, string devmac)
        //{
        //    if (!string.IsNullOrEmpty(token))//LoginUser.token
        //    {
        //        string url = $"http://192.168.1.108/CardSolution/card/accessControl/swingCardRecord/bycondition/combined?userId=1&userName=system&token={ token }&orgCode=001";
        //        var postData = "{\"pageNum\": 1,\"pageSize \": 20,\"deviceCode\": \"" + devmac + "\"}";
        //        return HttpUtil.PostUtil(url, postData);
        //    }
        //    return null;
        //} 
        #endregion

        #endregion

        #endregion

        #region  登录验证接口
        private static string _token = "";
        static System.Timers.Timer cl_timer = null;

        public static void ReToken()
        {
            log.Debug("重新获取token中……");
            if (cl_timer != null)
            {
                //先停止之前的重连
                cl_timer.Stop();
                cl_timer.Enabled = false;
                cl_timer.Dispose();
            }
            //每15分钟重新获取token一次
            cl_timer = new System.Timers.Timer(900000);
            //cl_timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>{});
            cl_timer.Elapsed += OnTimedEvent;
            cl_timer.Enabled = true;
            cl_timer.Start();
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            GetLoginToken();
        }

        public static void GetLoginToken()
        {
            string publicKeyJson = PostPublicKey();
            log.Debug($"大华获取公钥返回：{publicKeyJson}");
            var loginPublicKey = JsonConvert.DeserializeObject<LoginPublicKey>(publicKeyJson);
            string publickey = RSAUtil.RSAPublicKeyJava2DotNet(loginPublicKey.publicKey);

            //string publickey1 = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCmNBfRAvR3Eq5TlC8dp43LgNoyoRb6aMCSlLzlEM5ZJFtwLfiAi3wzPD3QXut0OazKnOLwNXfkWsIMMGsvpKR6C75HNUt8Rskd8d108pzCZv0p0sDLvIhVF1jjD4CPvfaA89o3W1DbDDfBevXBKn2lm27oKatySaFHq7CFZPnvGQIDAQAB";
            //string publickey = RSAUtil.RSAPublicKeyJava2DotNet(publickey1);

            string sign = RSAUtil.RSAEncryptMore(publickey, "qazwsx123");
            string LoginUserJson = PostLogin(sign);
            log.Debug($"大华用户登录返回：{LoginUserJson}");

            //string dd = "{\"success\":\"true\",\"loginName\":\"system\",\"errMsg\":null,\"token\":\"4540963af0c057e6f4389a90019cb4a2\",\"id\":\"1\",\"cmsIp\":\"192.168.1.108\",\"cmsPort\":\"9000\",\"orgCode\":\"001\",\"publicKey\":\"MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAJdvuhK0glofNSE+nvTZe4gL+v7ceM5zy91tk+NeDQM90l7HgW2z+5fvCwRww7EzAidR1ZSBhrciOs2SkxyPwfvBaaDaZpuu998AikrBjVl0S2GuDEOWP0Sh7BBrhlgnLB0xCTHV4bIrYVur6DRCwZgKTRV3ksq9yLPQHQAzdxqxAgMBAAECgYAf0FD+7P0VgcjfmxA50Barlhi8wgR/GsSRWBvhgDirnak8UB1Ytp78ZDOkUyxJZbXmHMMJ18w9XOuNlGVjcrAFpiStl/UN+ZjqpoEaEqqwb1EC1zMt84UDmKL1y0eUx7tNzvB/9eixCbKLrm+F5F0T0OAesZRZwiVgBhc+TSiLgQJBAP+ZQwBe4mUiZ2KzwNITylzvboM8Ny/kodgyxXE/FTMlmlNfZY3mYuuKY2AQs2ZJq3ec1LBiDuGQB29fq72oJkUCQQCXrJjRzLVMFfIyqvGxgYioXT+fBhkRXLweid7eXK43HEZIWVgi7j00w/qCX622Ly6B9ImiZG/gZwsRUf+9Ou99AkEAsEN2BCxq/gmSuGtzvqvtMtffI1uER1/pCJpCtM0nBoWY/oPcGdZWQ07FJzt9LD4DpFIgDp8g2gakSfb1Da6G7QJAPp5qZUufme8BlDuRF1jEQ8ZjytKorMtdezough0/a89HkP0Z7ynuqQc0OHkp7apjCBIedKYErl+8aQUykTxwvQJAU0POSC295Q8lBbtKX80TQmXtHuZLmVfO7O8fBUCI2oIMenxecrz9B4cNv2oOsSiUV2X7/9xXXYLan26wxEu0QQ\u003d\u003d\"}";
            var loginToken = JsonConvert.DeserializeObject<LoginPublicKey>(LoginUserJson);
            _token = loginToken.token;
        }

        /// <summary>
        /// 获取公钥
        /// </summary>
        /// <returns></returns>
        private static string PostPublicKey()
        {
            string url = "http://192.168.1.108/WPMS/getPublicKey?spm=1540175333825";
            var user = new
            {
                loginName = "system",
            };
            var postData = JsonConvert.SerializeObject(user);
            return HttpUtil.PostUtil(url, postData);
        }

        /// <summary>
        /// 登陆token
        /// </summary>
        private static string PostLogin(string sign)
        {
            string url = "http://192.168.1.108/WPMS/login";
            var user = new
            {
                loginName = "system",
                loginPass = sign
            };
            var postData = JsonConvert.SerializeObject(user);
            return HttpUtil.PostUtil(url, postData);

        }

        public class LoginPublicKey
        {
            /// <summary>
            /// 
            /// </summary>
            public string success { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string loginName { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string errMsg { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string token { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string cmsIp { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string cmsPort { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string orgCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string publicKey { get; set; }
        }

        #endregion
    }
}
