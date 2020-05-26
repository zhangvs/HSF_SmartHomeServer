using Hsf.EF.Model;
using Hsf.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer
{
    class SmsControler
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmsControler");
        public enum SmsType
        {
            注册 = 1,
            提醒 = 2
        }
        public class WebSiteConfig
        {
            /// <summary>
            /// 短信过期分钟
            /// </summary>
            public const int SMS_EXPIRE_MIN = 2;

            /// <summary>
            /// 每天短信发送次数
            /// </summary>
            public const int SMS_MAX_COUNT = 6;
        }



        //判断验证码是否正确
        public static string ValidateSmsCode(string tel, string smsCode)
        {
            using (HsfDBContext hsfDBContext = new HsfDBContext())
            {
                var model = hsfDBContext.smsinfo.Where(t => t.Tel == tel && t.Type == (int)SmsType.注册).OrderByDescending(t => t.CreateTime).FirstOrDefault();
                if (model == null)
                    return "error:Verification code error";//验证码不正确
                if (smsCode != model.Captcha)
                    return "error:Verification code error";

                //判断是否过期
                if (model.CreateTime.AddMonths(WebSiteConfig.SMS_EXPIRE_MIN) < DateTime.Now)
                    return "error:The validation code has expired. Please send it again."; //验证码已过期，请重新发送
                return "smsCode ok";
            }
        }
        //注册
        public static string SendJoin(string tel)
        {
            try
            {
                //1.判断是否已发送 是否过期
                //2.判断是否超过一天的发送次数
                using (HsfDBContext hsfDBContext = new HsfDBContext())
                {
                    var sms = hsfDBContext.smsinfo.FirstOrDefault(t => t.Tel == tel && t.Type == (int)SmsType.注册);//不加时间条件的话，第二条第一次再试还是超过的
                    if (sms != null)
                    {
                        if (sms.CreateTime.AddMinutes(WebSiteConfig.SMS_EXPIRE_MIN) > DateTime.Now)
                            throw new Exception("error:Please send it again in 1 minute.");//请1分钟后再次发送

                        int day = sms.CreateTime.Subtract(DateTime.Now).Days;
                        if (day == 0)
                        {
                            sms.SendCount += 1;//已经等于6
                        }
                        else
                        {
                            sms.SendCount = 1;
                        }
                        if (sms.SendCount >= WebSiteConfig.SMS_MAX_COUNT)
                            throw new Exception("error:It can only be sent " + WebSiteConfig.SMS_MAX_COUNT + " times a day.");//一天内只能发送" + WebSiteConfig.SMS_MAX_COUNT + "次
                    }

                    //6位的随机验证码
                    string code = SmsCore.GetNumberCode();
                    var bo = SmsCore.SendSms(tel, code);
                    if (bo == false)
                        throw new Exception("error:Failed to send SMS");//短信发送失败
                    if (sms == null)
                    {
                        sms = new smsinfo()
                        {
                            Captcha = code,
                            CreateTime = DateTime.Now,
                            Tel = tel,
                            SendCount = 1,
                            Status = bo ? 1 : 0,
                            Type = (int)SmsType.注册
                        };
                        hsfDBContext.smsinfo.Add(sms);
                    }
                    else
                    {
                        //int day = sms.CreateTime.Subtract(DateTime.Now).Days;
                        sms.CreateTime = DateTime.Now;
                        sms.Status = bo ? 1 : 0;
                        sms.Captcha = code;
                        //if (day == 0)
                        //{
                        //    sms.SendCount += 1;//已经等于6
                        //}
                        //else
                        //{
                        //    sms.SendCount = 1;
                        //}
                    }
                    hsfDBContext.SaveChanges();
                    log.Debug($"send ok");
                    return "send ok";
                }
            }
            catch (Exception ex)
            {
                log.Debug($"error:send fail {ex.Message}");
                return $"error:send fail {ex.Message}";
            }
        }
    }
}
