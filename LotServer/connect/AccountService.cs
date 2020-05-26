using Newtonsoft.Json;
using Hsf.EF.Model;
using Hsf.Framework;
using Hsf.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hsf.Redis.Service;

namespace LotServer.connect
{
    public class AccountService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("AccountService");

        #region 111主机登录信息
        /// <summary>
        /// 主机登录信息
        /// </summary>
        /// <param name="msg">user:zys_Server type:other msg:
        /// zys_DCD9165057AD;1;111;all;admin,admin,shouquanma,zys_Server,DCD9165057AD,192.168.88.101$/r$</param>
        /// ssxy_C40BCB80050A;111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$
        public static string Host111(string msg)
        {
            try
            {
                string account_mac = msg.Split(';')[0];
                if (account_mac.Contains("_"))
                {
                    string account = account_mac.Split('_')[0];
                    string mac = account_mac.Split('_')[1];

                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //判断主机家庭账号是否存在
                        var accountEntity = hsfDBContext.host_account.Where(t => t.Account == account && t.DeleteMark == 0).FirstOrDefault();
                        if (accountEntity != null)
                        {
                            return $"H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA";//user:{account_mac} type:other msg:{account_mac};111;all;Zip;  $/r$
                        }
                        else
                        {
                            return $"H4sIAAAAAAAAAEstKsovAgBxvN1dBQAAAA==";//user:{account_mac} type:other msg:{account_mac};111;all;Zip;  $/r$
                        }
                    }
                }
                else
                {
                    return $"Command Fail";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        #region 注册
        /// <summary>
        /// 新增业主
        /// </summary>
        /// <param name="msg">Reg H4sIAAAAAAAEAKtWKknNKcjIz0tVslIytDAzM7C0NLMwtlTSUSpILC4uzy9KAUkYGZuYmgHFkjMy8xLzEnNBqp9O6H05fcWTHdOertsGlEoqzcxJycxLBykHckvzMkugzLSc/PwiKLsoPz8XxDQwVKoFAL/J9yR8AAAA
        /// {"telphone":"18660996839","password":"123456","chinaname":"名门世家","building":"1","unit":"1","floor":"1","room":"101"}</param>
        public static string RegOwner(string msg)
        {
            try
            {
                if (msg.Contains("H4"))
                {
                    string base64j = EncryptionHelp.Decrypt(msg, true);
                    var owner = JsonConvert.DeserializeObject<hsf_owner>(base64j);
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        using (RedisHashService service = new RedisHashService())
                        {

                            ////先验证小区名称是否存在
                            //string residentialStr = service.GetValueFromHash("Residential", owner.chinaname);
                            //if (string.IsNullOrEmpty(residentialStr))
                            //{
                            //    var residentialEntity = hsfDBContext.hsf_residential.Where(t => t.chinaname == owner.chinaname && t.deletemark == 0).FirstOrDefault();
                            //    if (residentialEntity != null)
                            //    {
                            //        residentialStr = residentialEntity.residential;
                            //        //缓存小区中文名-拼音缩写
                            //        service.SetEntryInHash("Residential", owner.chinaname, residentialStr);
                            //    }
                            //    else
                            //    {
                            //        log.Debug($"注册 Fail,添加业主失败，不存在小区！");
                            //        return "Residential error 小区名称不对！";//error
                            //    }
                            //}
                            //owner.residential = residentialStr;

                            //2.不验证小区，有零散客户购买的情况，自己填写小区
                            if (!string.IsNullOrEmpty(owner.chinaname))
                            {
                                owner.residential = PingYinHelper.GetFirstSpell(owner.chinaname);//生成小区首字母
                            }
                            else
                            {
                                log.Debug($"error:小区不能为空！");
                                return "error:小区不能为空！";
                            }

                            var ownerEntity = hsfDBContext.hsf_owner.Where(t => t.telphone == owner.telphone && t.deletemark == 0).FirstOrDefault();
                            if (ownerEntity != null)
                            {
                                ownerEntity.deletemark = 1;//删除老的，新增新的
                                AddReg(hsfDBContext, service, owner);
                                log.Debug($"注册 Ok,业主信息修改成功！");
                            }
                            else
                            {
                                //当前业主id需要保存,网关业主id为0，不可以
                                AddReg(hsfDBContext, service, owner);
                                log.Debug($"注册 OK,添加业主成功！");
                            }
                            return "Regok";//Addok

                        }
                    }
                }
                log.Debug($"注册失败,不是zip数据！");
                return "error:Zip data error";//error

            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 新增业主数据库操作
        /// </summary>
        /// <param name="hsfDBContext"></param>
        /// <param name="_SoundHost"></param>
        public static void AddReg(HsfDBContext hsfDBContext, RedisHashService service, hsf_owner owner)
        {
            owner.Id = Guid.NewGuid().ToString();
            owner.host = owner.residential + "-" + owner.building + "-" + owner.unit + "-" + owner.floor + "-"+owner.room;
            owner.createtime = DateTime.Now;
            owner.deletemark = 0;
            hsfDBContext.hsf_owner.Add(owner);
            hsfDBContext.SaveChanges();

            //缓存业主json
            service.SetEntryInHash("Owner", owner.telphone, JsonConvert.SerializeObject(owner));
            //缓存账号密码
            service.SetEntryInHash("Login", owner.telphone, owner.password);
        }
        #endregion

        /// <summary>
        /// 登录验证
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string CheckLogin(string account, string password)
        {
            hsf_owner ownerEntity = null;
            using (HsfDBContext hsfDBContext = new HsfDBContext())
            {
                using (RedisHashService service = new RedisHashService())
                {
                    string _password = service.GetValueFromHash("Login", account);
                    if (string.IsNullOrEmpty(_password))
                    {
                        ownerEntity = hsfDBContext.hsf_owner.Where(t => t.telphone == account && t.deletemark == 0).FirstOrDefault();
                        if (ownerEntity != null)
                        {
                            service.SetEntryInHash("Login", account, ownerEntity.password);
                            if (ownerEntity.password == password)
                            {
                                return EncryptionHelp.Encryption(JsonConvert.SerializeObject(ownerEntity), false);
                            }
                            else
                            {
                                return "error:Password error!";
                            }
                        }
                        else
                        {
                            return "error:No account exists!";
                        }
                    }
                    else
                    {
                        if (_password == password)
                        {
                            string _Owner = service.GetValueFromHash("Owner", account);
                            if (string.IsNullOrEmpty(_Owner))
                            {
                                ownerEntity = hsfDBContext.hsf_owner.Where(t => t.telphone == account && t.deletemark == 0).FirstOrDefault();
                                if (ownerEntity!=null)
                                {
                                    service.SetEntryInHash("Owner", ownerEntity.telphone, JsonConvert.SerializeObject(ownerEntity));
                                    return EncryptionHelp.Encryption(JsonConvert.SerializeObject(ownerEntity), false);
                                }
                                else
                                {
                                    return "error:No account exists!";
                                }
                            }
                            else
                            {
                                return EncryptionHelp.Encryption(_Owner, false);
                            }
                        }
                        else
                        {
                            return "error:Password error!";
                        }
                    }
                }
            }
        }
        

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Password(string account, string password)
        {
            hsf_owner ownerEntity = null;
            using (HsfDBContext hsfDBContext = new HsfDBContext())
            {
                using (RedisHashService service = new RedisHashService())
                {
                    ownerEntity = hsfDBContext.hsf_owner.Where(t => t.telphone == account && t.deletemark == 0).FirstOrDefault();
                    if (ownerEntity != null)
                    {
                        ownerEntity.password = password;
                        hsfDBContext.SaveChanges();
                        service.SetEntryInHash("Login", account, password);
                        service.SetEntryInHash("Owner", ownerEntity.telphone, JsonConvert.SerializeObject(ownerEntity));
                        return "password ok";
                    }
                    else
                    {
                        return "error:No account exists!";
                    }
                }
            }
        }

    }
}
