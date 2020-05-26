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
    public class RoomService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("RoomService");
        #region 房间操作

        #region 835获取家庭房间列表
        /// <summary>
        /// 1.请求主机房间列表
        /// user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$
        /// 2.主机返回房间列表
        /// user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;835;admin;Zip;H4sIAAAAAAAAAIuuViotTi3KTFGyUlLSUSouSSxJBTJLikpTgdzkjMy8xLzEXJDQs46Jz2e1PF23DSiemZyfB9GQmZuYngrTXZBfDGYaQNgFiUWJSlbVSimpZSX5JYk5QBlDS5AliWmpxaklJZl56TCrasEaSioLUqHa40EGGego+aWWB6Um5xcBeSCFtTrY3Yvm1qfrFj3ta8Xh0KL8/FxDJNcaGhgbmpoYG1iam5iiOJzajupd/nTdEtIcBcRmBjR1VNe8p61rSHaXkampBW0Da13nyxmbSHOUsYmBkTl5jooFAHQFerEIAwAA$/r$
        /// </summary>
        /// <param name="msg">user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$</param>
        public static string Host835(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string appUser = msg.Split(';')[0];
                    if (appUser.Contains("_"))
                    {
                        string account = appUser.Split('_')[0];//DAJCHSF,一个家庭可能有多个用户，mac不同，只取账户
                        List<host_room> roomList = null;
                        string roomListJson = "";
                        string msgResult = "";
                        using (RedisHashService service = new RedisHashService())
                        {
                            //获取当前房间的设备列表，先找缓存
                            roomListJson = service.GetValueFromHash("Room", account);
                            if (string.IsNullOrEmpty(roomListJson))
                            {
                                using (HsfDBContext hsfDBContext = new HsfDBContext())
                                {
                                    roomList = hsfDBContext.host_room.Where(t => t.Account == account && t.DeleteMark == 0).ToList();
                                    roomListJson = JsonConvert.SerializeObject(roomList);
                                    //缓存当前账户房间列表返回字符串
                                    service.SetEntryInHash("Room", account, JsonConvert.SerializeObject(roomList));
                                }
                            }
                            msgResult = $"{appUser};835;admin;Zip;{EncryptionHelp.Encryption(roomListJson, true)}$/r$";//带上用户信息
                            log.Debug($"835 OK,返回房间列表成功！返回信息：{msgResult}");
                            return msgResult;
                        }
                    }
                    else
                    {
                        log.Debug($"835 Fail,命令不符合规范！");
                        return null;
                    }
                }
                else
                {
                    log.Debug($"835 Fail,命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region 836新增房间
        /// <summary>
        /// 新增房间
        /// </summary>
        /// <param name="msg">user:123_Server type:other msg:
        /// 123_DCD9165057AD;8;836;H4sIAAAAAAAAAC2Muw7CIBSG3+XMDBxavLAZmR18gYbgiTIADdAQ0/Tdhdbt+68rPKg+ycb0AlXSQgwm15AzsB8XTDCeQEGtwMDZGBp38uZNvQYpRo/NmWPeNQohceQ4ohguhz+bZECt2y7Kd+5/vEW5mELH35Ip/efDpO/6iifJ5fmmYfsBAlhUH6EAAAA=$/r$
        /// {"NewRecord":true,"_id":0,"chinaname":"ww","icon":"","imageid":"room1","posid":"1225140141238","pospara":{},"postype":"0","state":"","userid":"123_DCD9165057AD"}</param>
        public static string Host836(string msg)
        {
            try
            {
                string appUser = msg.Split(';')[0];
                if (appUser.Contains("_"))
                {
                    string account = appUser.Split('_')[0];
                    string mac = appUser.Split('_')[1];
                    string zipStr = msg.Split(';')[3].Replace("$/r$", "");
                    string base64j = EncryptionHelp.Decrypt(zipStr, true);
                    var room = JsonConvert.DeserializeObject<host_room>(base64j);
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        using (RedisHashService service = new RedisHashService())
                        {
                            var roomEntity = hsfDBContext.host_room.Where(t => t.posid == room.posid && t.DeleteMark == 0).FirstOrDefault();
                            if (roomEntity != null)
                            {
                                //roomEntity.DeleteMark = 1;
                                //roomEntity.ModifyUser = appUser;
                                //roomEntity.ModifyTime = DateTime.Now;
                                hsfDBContext.host_room.Remove(roomEntity);//真实删除
                                AddRoom(hsfDBContext, room, appUser, account, mac);
                                log.Debug($"836 Ok,房间信息修改成功！");
                            }
                            else
                            {
                                //当前房间id需要保存,网关房间id为0，不可以
                                AddRoom(hsfDBContext, room, appUser, account, mac);
                                log.Debug($"836 OK,添加房间成功！");
                            }
                            //清除房间缓存信息，等待查询之后再次缓存
                            service.RemoveEntryFromHash("Room", account);//解决默认posid都为0的问题
                            log.Debug($"清除家庭缓存{account}");
                            return $"{appUser};836;ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$";//Addok

                        }
                    }
                }
                else
                {
                    log.Debug($"836 Fail,添加房间失败，命令不符合规范！");
                    return $"{appUser};836;ALL;Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//error
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 新增房间数据库操作
        /// </summary>
        /// <param name="hsfDBContext"></param>
        /// <param name="_SoundHost"></param>
        public static void AddRoom(HsfDBContext hsfDBContext, host_room room, string appUser, string account, string mac)
        {
            room.CreateUser = appUser;
            room.Account = account;
            room.Mac = mac;
            room.id = Guid.NewGuid().ToString();
            room.CreateTime = DateTime.Now;
            room.DeleteMark = 0;
            hsfDBContext.host_room.Add(room);
            hsfDBContext.SaveChanges();
        }
        #endregion

        #region 837删除房间
        /// <summary>
        /// 删除房间
        /// </summary>
        /// <param name="msg">123_DCD9165057AD;8;837;+posid</param>
        public static string Host837(string msg)
        {
            try
            {
                string appUser = msg.Split(';')[0];
                string account = appUser.Split('_')[0];
                string posid = msg.Split(';')[3].Replace("$/r$", "");

                using (HsfDBContext hsfDBContext = new HsfDBContext())
                {
                    using (RedisHashService service = new RedisHashService())
                    {
                        var roomEntity = hsfDBContext.host_room.Where(t => t.posid == posid && t.DeleteMark == 0).FirstOrDefault();
                        if (roomEntity != null)
                        {
                            //roomEntity.DeleteMark = 1;
                            //roomEntity.ModifyUser = appUser;
                            //roomEntity.ModifyTime = DateTime.Now;
                            hsfDBContext.host_room.Remove(roomEntity);//真实删除
                            hsfDBContext.SaveChanges();
                            //清除房间缓存信息，等待查询之后再次缓存
                            service.RemoveEntryFromHash("Room", account);//解决默认posid都为0的问题
                            log.Debug($"837 Ok,删除房间成功！清除家庭缓存：{account}");
                            return $"{appUser};837;ALL;Zip;H4sIAAAAAAAEAHNJzcnPBgBZ82EeBQAAAA==$/r$";//Delok
                        }
                        else
                        {
                            //当前房间id需要保存,网关房间id为0，不可以
                            log.Debug($"837 Fail,删除房间失败，房间id不存在！");
                            return $"{appUser};837;ALL;Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//error
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #endregion
    }
}
