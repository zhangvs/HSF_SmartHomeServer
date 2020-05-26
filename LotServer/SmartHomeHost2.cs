using Newtonsoft.Json;
using Hsf.EF.Model;
using Hsf.Framework;
using Hsf.StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LotServer
{
    /// <summary>
    /// 智能家居主机
    /// </summary>
    public class SmartHomeHost2
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartHomeHost2");
        private static RedisHashService service = new RedisHashService();
        
        #region 111更改是否自身播放音乐状态反转
        /// <summary>
        /// 更改是否自身播放音乐状态反转
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

                    //如果发送到音响失败，则不修改数据库
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //获取主机和设备，删除标记为0，未删除的
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
                        //获取当前房间的设备列表，先找缓存
                        string msgResult = service.HashGet("Room", account);
                        if (string.IsNullOrEmpty(msgResult))
                        {
                            using (HsfDBContext hsfDBContext = new HsfDBContext())
                            {
                                roomList = hsfDBContext.host_room.Where(t => t.Account == account && t.DeleteMark == 0).ToList();
                                msgResult = $";835;admin;Zip;{EncryptionHelp.Encryption(JsonConvert.SerializeObject(roomList), true)}$/r$";//不能缓存用户信息
                                                                                                                                           //缓存当前账户房间列表返回字符串
                                service.HashSet("Room", account, msgResult);
                            }
                        }
                        msgResult = appUser + msgResult;//带上用户信息
                        log.Debug($"835 OK,返回房间列表成功！返回信息：{msgResult}");
                        return msgResult;
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
        public static void Host836(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
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
                            var roomEntity = hsfDBContext.host_room.Where(t => t.posid == room.posid && t.DeleteMark == 0).FirstOrDefault();
                            if (roomEntity != null)
                            {
                                roomEntity.DeleteMark = 1;
                                roomEntity.ModifyUser = appUser;
                                roomEntity.ModifyTime = DateTime.Now;
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
                            service.HashDelete("Room", account);//解决默认posid都为0的问题
                            log.Debug($"清除家庭缓存{account}");
                        }
                    }
                    else
                    {
                        log.Debug($"836 Fail,添加房间失败，命令不符合规范！");
                    }
                }
                else
                {
                    log.Debug($"836 Fail,添加房间失败，命令不符合规范！");
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
        public static void Host837(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string appUser = msg.Split(';')[0];
                    string account = appUser.Split('_')[0];

                    string posid = msg.Split(';')[3].Replace("$/r$", "");
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        var roomEntity = hsfDBContext.host_room.Where(t => t.posid == posid && t.DeleteMark == 0).FirstOrDefault();
                        if (roomEntity != null)
                        {
                            roomEntity.DeleteMark = 1;
                            roomEntity.ModifyUser = appUser;
                            roomEntity.ModifyTime = DateTime.Now;
                            hsfDBContext.SaveChanges();
                            //清除房间缓存信息，等待查询之后再次缓存
                            service.HashDelete("Room", account);//解决默认posid都为0的问题
                            log.Debug($"837 Ok,删除房间成功！清除家庭缓存：{account}");
                        }
                        else
                        {
                            //当前房间id需要保存,网关房间id为0，不可以
                            log.Debug($"837 Fail,删除房间失败，房间id不存在！");
                        }
                    }
                }
                else
                {
                    log.Debug($"837 Fail,删除房间失败，命令不符合规范！");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #endregion

        #region 设备操作

        #region 815获得当前房间的设备列表的命令
        /// <summary>
        /// 获得当前房间的设备列表的命令8;815;+ posid
        /// </summary>
        /// <param name="msg">user:123_Server type:other msg:
        /// 123_DCD9165057AD;8;815;1225155025360$/r$</param>
        public static string Host815(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string appUser = msg.Split(';')[0];
                    string account = appUser.Split('_')[0];
                    string posid = msg.Split(';')[3].Replace("$/r$", "");//房间id  ,默认id为0，
                    List<host_device> deviceList = null;
                    //获取当前房间的设备列表，先找缓存
                    string deviceListJson = service.HashGet("RoomDevices", account + "|" + posid);
                    if (!string.IsNullOrEmpty(deviceListJson))
                    {
                        deviceList = JsonConvert.DeserializeObject<List<host_device>>(deviceListJson);
                    }
                    //如果缓存中没有，再查数据库
                    else
                    {
                        using (HsfDBContext hsfDBContext = new HsfDBContext())
                        {
                            deviceList = hsfDBContext.host_device.Where(t => t.devposition == posid && t.deletemark == 0).OrderBy(t => t.createtime).ToList();
                            //缓存当前房间的设备列表,不包括状态,不管空与否都缓存，防止第二次还查数据库
                            service.HashSet("RoomDevices", account + "|" + posid, JsonConvert.SerializeObject(deviceList));//解决默认posid都为0的问题
                        }
                    }

                    //真正更新设备状态
                    string zipStr = "";
                    foreach (var item in deviceList)
                    {
                        //读取缓存状态
                        string status = service.HashGet("DeviceStatus", item.cachekey);
                        if (string.IsNullOrEmpty(status))
                        {
                            //离线
                            item.powvalue = "离线";
                            item.devstate = "false";
                        }
                        else
                        {
                            item.powvalue = "在线";
                            item.devstate = status.ToLower();
                        }
                    }
                    zipStr = EncryptionHelp.Encryption(JsonConvert.SerializeObject(deviceList), true);
                    string msgResult = $"{appUser};815;{posid};Zip;{zipStr}$/r$";//拼接
                    log.Debug($"815 OK,返回房间设备列表成功！返回信息：{msgResult}");
                    return msgResult;
                }
                else
                {
                    log.Debug($"815 Fail,命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 855获得当前设备类型的设备列表的命令
        /// <summary>
        /// 获得当前设备类型的设备列表的命令8;855;+ devtype
        /// </summary>
        /// <param name="msg">1.查询设备类型
        /// user:DAJCHSF_Server type:other 
        /// msg:DAJCHSF_2047DABEF936;8;855;0;Panel_Smartsix,Panel_Wired_Control,Panel_Zigbee,Panel_SingleZigbee,Panel_485Gao,Panel_485Hotel,Panel_485Bus$/r$  面板
        /// user:JYWH_Server type:other 
        /// msg:JYWH_2047DABEF936;8;855;0;Panel_Smartsix,Panel_Wired_Control,Panel_Zigbee,Panel_SingleZigbee,Panel_485Gao,Panel_485Hotel,Panel_485Bus$/r$
        /// user:123_Server type:other 
        /// msg:123_DCD9165057AD;8;855;0;Zigbee_Gate$/r$  网关
        /// 2.返回结果
        /// user:DAJCHSF_2047DABEF936 type:other 
        /// msg:DAJCHSF_2047DABEF936;855;0;Zip;H4sIAAAAAAAAAOWZb2vTQBjAv4rkdZG7y92l2YcQ8aUi45pdukD/kaQOGQOH5oWFjb0Qq0wcOtGiMgcbUoqyT3Pr+i28a9LON2240kObNjTluculye8JvzyXPNq12hEPgy1rw7JKVqu584TV2lxG4l1vOLiSbd520GANVh83nn0Uh4k46gz3f6iuGosifzJ4i8uxLKxPI2+bNRq8JmMAfB95aqGM+gAQl5J0m8Dj4/EQ2JCiMoYUY5B1tVQ7cu4CucC0rc48tTsI5AciXJFrB/kU80ra32IhszZ299KgGcZqF1lXMwrioNnI/oxgG7gOzg4jilmsTtBntYj/1QSnJzMO0TSMn7bU9veZPMHNh0G1wtWwoM6qfMKjVo3GBwBK1qZqk7/3+M4D7jVDGcVhm++VFsJ//sIkfgKXix8VDL9kL/qfzeFHy8VvFwy/ON03dPW7kABXWz7cdytkHeRz/XZw8/y3+PVMJBfQGH9t++Tw17GPovLf4k+v/uvX56PTQ2P0qa58cugXSD4H34avTkbdS3HUH334aUZBuIwAwlBPQdgDNuFgHRSkbr3JS5H0pIWM4Ud6BsrFX5z65xZ/cmEOv56CcvEXR0E3V8ejbi+dfN1ZPn8HIgApcBewj78W9kn5myr/U/wQLGCfefiLY5/s8jc0+c3wL2KfefiLYx9x8EWcfTL16MexCSJ2mejKh7g2dBeQj/xSsHr4jcknxU915ZODf6Z8VhS/Mfmk+B1d+eTgnymfFcVvavI7wW8vFz8uGP7h167ov1k2fgxcRCB0gOaTN3XrdZk9G3+B3H98KQYdM+6f4td88JaLv3DuNzLtvcWvX3jOxV8g93fei+S7sdoTQ4gBJIRoTnzVixfqLVJ7IkLKq1X6TzNgpP6ZZIBqzn1zMzBTQaubAUM3gUkGoP7Lx7kZmGmhf5+Bx38AI07FAvsgAAA=$/r$</param>
        public static string Host855(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    string appUser = msg.Split(';')[0];
                    string account = appUser.Split('_')[0];
                    string devtypeStr = msg.Split(';')[4].Replace("$/r$", "");//设备类型列表
                    string[] devtypes = devtypeStr.Split(',');
                    List<host_device> deviceList = null;
                    //获取当前设备类型的设备列表，先找缓存
                    string deviceListJson = service.HashGet("TypeDevices", account + "|" + devtypeStr);
                    if (!string.IsNullOrEmpty(deviceListJson))
                    {
                        deviceList = JsonConvert.DeserializeObject<List<host_device>>(deviceListJson);
                    }
                    //如果缓存中没有，再查数据库
                    else
                    {
                        using (HsfDBContext hsfDBContext = new HsfDBContext())
                        {
                            deviceList = hsfDBContext.host_device.Where(t => t.account == account && devtypes.Contains(t.devtype) && t.deletemark == 0).OrderBy(t=>t.createtime).ToList();
                            //缓存当前设备类型的设备列表,不包括状态,不管空与否都缓存，防止第二次还查数据库
                            service.HashSet("TypeDevices", account + "|" + devtypeStr, JsonConvert.SerializeObject(deviceList));//解决默认posid都为0的问题
                        }
                    }

                    //真正更新设备状态
                    string zipStr = "";
                    foreach (var item in deviceList)
                    {
                        //读取缓存状态
                        string status = service.HashGet("DeviceStatus", item.cachekey);
                        if (string.IsNullOrEmpty(status))
                        {
                            //离线
                            item.powvalue = "离线";
                            item.devstate = "false";
                        }
                        else
                        {
                            item.powvalue = "在线";
                            item.devstate = status.ToLower();
                        }
                    }
                    zipStr = EncryptionHelp.Encryption(JsonConvert.SerializeObject(deviceList), true);
                    string msgResult = $"{appUser};855;0;Zip;{zipStr}$/r$";//拼接
                    log.Debug($"855 OK,返回设备类型的设备列表成功！返回信息：{msgResult}");
                    return msgResult;
                }
                else
                {
                    log.Debug($"855 Fail,命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 8211,8212添加设备
        /// <summary>
        /// 添加设备 8;8211;All;+Base64(zip(设备对象jhson串))
        /// </summary>
        /// <param name="msg">user:123_Server type:other msg:
        /// 123_DCD9165057AD;8;8211;ALL;H4sIAAAAAAAAAH2QPU7EMBCF7zJ1CjuRs2wuQEdBuQitBmectZTEke2wQqutEAeg5Bx0SHscfo6Bf6JINMiNv/eeZ8Zzd4IbOt6SNLaFxtuZCtjrcGUFyIMeccSBoIGvt4+f58v35fXz5Z1B8Hp0TsUgXAVs6RF7tEPATPKA40j9ylpSCvOyFFyIWvCKbdniTcFgTKlSxlNjrRgTG6WyPaD815/QIjSncwZjfWxTic12sY3TXpsx1siKpU6altI0VZacR0/rsIn4XyxX9E9TjO5090C0v44PC9ADdvmHIcGZCFLfuTRM2ORkjmE/89JhdmRTFM73v4aKUN2AAQAA$/r$
        /// user:123_DCD9165057AD type:other msg:
        /// 123_DCD9165057AD;8;8212;ALL;H4sIAAAAAAAAAG2RsU7DMBCG38VzVOUcN6TZGBlggBGh6ORcUkuJHdlJI1RVYmFn4ykQAyuv04G3wIlLRBHy4u/+/3y/zvd7dkPjLUljS5b3dqCIFcpf44jJrdKosSWWM5RZUiUgCKsL5qUGnasmH8s8lrTDBm3rMZDcotbULKwkzWbgsAbBIRYiSeOT1s1CEqhFOeGGryDNVlm2gpgHpUOLLN/72cZNkY7PH1+vbxEHiPz522A60pPp8+n48v6/6RCeNbZfcnbGqV4ZHaICCJHGsPlJaqmWpqTfcV2PPS3tM8E58gX7x26yXl4V16Tr0gy+rFqsw2q8AeK1LzW1myP5H+jM6Bc7nAYMjuxsHbFRxR3ZHVl2ePgGQ/XuYsMBAAA=$/r$
        /// 
        /// </param>
        public static string AddDeviceToRoom(string msg,string code)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    string appUser = msg.Split(';')[0];
                    string account = appUser.Split('_')[0];
                    string mac = appUser.Split('_')[1];
                    string zipStr = msg.Split(';')[4].Replace("$/r$", "");
                    string base64j = EncryptionHelp.Decrypt(zipStr, true);
                    var deviceLists = JsonConvert.DeserializeObject<List<host_device>>(base64j);//list多件开关,ALL数组
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        string posid = "";
                        string cachekey = "";
                        string devchannel = "";
                        string devtype = "";
                        string devmac = "";
                        foreach (var item in deviceLists)
                        {
                            devtype = item.devtype;
                            devchannel = item.devchannel;
                            posid = item.devposition;
                            devmac = item.devmac;
                            cachekey = item.devmac + "_" + item.devport;//存在mac相同，端口不相同的多键设备

                            var deviceEntity = hsfDBContext.host_device.Where(t => t.cachekey == cachekey && t.deletemark == 0).FirstOrDefault();
                            if (deviceEntity != null)
                            {
                                deviceEntity.deletemark = 1;
                                deviceEntity.modifiyuser = appUser;
                                deviceEntity.modifiytime = DateTime.Now;
                                AddDeviceEntity(hsfDBContext, item, appUser, account, mac);
                                log.Debug($"{code} OK,重新添加设备成功！");
                            }
                            else
                            {
                                //当前房间id需要保存,网关房间id为0，不可以
                                AddDeviceEntity(hsfDBContext, item, appUser, account, mac);
                            }
                        }
                        //如果设备网关字段不为空，则查询网关最初状态，可能不全部关闭？
                        if (!string.IsNullOrEmpty(devchannel) && devtype.Contains("Zigbee"))
                        {
                            YunZigClient.SendMsg($"{{\"code\" :5001,\"serial\": 11111,\"device\":[{{\"id\": \"{devmac}\"}}],\"zigbee\":\"{devchannel}\"}}");//发送
                        }

                        //清除房间设备列表缓存
                        service.HashDelete("RoomDevices", account + "|" + posid);
                        log.Debug($"清除房间设备列表缓存RoomDevices {account}|{posid}");
                        //清除当前设备类型的设备列表缓存
                        service.HashDelete("TypeDevices", account + "|" + devtype);
                        log.Debug($"清除当前设备类型的设备列表缓存TypeDevices {account}|{devtype}");

                        //2.主机返回app添加成功
                        string msgResult = $"{appUser};{code};ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$";//拼接
                        log.Debug($"{code} OK,添加设备成功！返回信息：{msgResult}");
                        return msgResult;
                    }
                }
                else
                {
                    log.Debug($"{code} Fail,添加设备失败，命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 添加新的设备数据库
        /// </summary>
        /// <param name="hsfDBContext"></param>
        /// <param name="_SoundHost"></param>
        public static void AddDeviceEntity(HsfDBContext hsfDBContext, host_device item, string appUser, string account, string mac)
        {
            item.createuser = appUser;
            item.account = account;
            item.mac = mac;
            item.Id = Guid.NewGuid().ToString();
            item.createtime = DateTime.Now;
            item.deletemark = 0;
            item.cachekey = item.devmac + "_" + item.devport;
            hsfDBContext.host_device.Add(item);
            hsfDBContext.SaveChanges();
        }
        #endregion

        #region 删除设备
        /// <summary>
        ///1.告诉主机删除设备
        ///user:123_Server type:other msg:123_DCD9165057AD;8;822;1041656180510$/r$
        ///2.主机返回删除成功delok@105 124612 6590
        ///user:123_DCD9165057AD type:other msg:123_DCD9165057AD;822;1041656180510;Zip;H4sIAAAAAAAAAEtJzcnPdjA0MDE0MzUztDAwNTQAAJxecTETAAAA$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static string Host822(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    string appUser = msg.Split(';')[0];
                    string account = appUser.Split('_')[0];
                    string deviceId = msg.Split(';')[3].Replace("$/r$", "");
                    string posid = "";
                    string cachekey = "";
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //能否确定deviceid唯一，用时间来做标识？？隐患 :添加用户限定排除风险
                        var deviceEntity = hsfDBContext.host_device.Where(t => t.deviceid == deviceId && t.account == account && t.deletemark == 0).FirstOrDefault();
                        if (deviceEntity != null)
                        {
                            posid = deviceEntity.devposition;
                            cachekey = deviceEntity.devmac + "_" + deviceEntity.devport;//存在mac相同，端口不相同的多键设备

                            deviceEntity.deletemark = 1;
                            deviceEntity.modifiyuser = appUser;
                            deviceEntity.modifiytime = DateTime.Now;
                            hsfDBContext.SaveChanges();

                            //2.主机返回app删除成功
                            string msgResult = $"{appUser};822;{deviceId};Zip;H4sIAAAAAAAEAEtJzcnPBgBd3KDfBQAAAA==$/r$";//拼接 delok

                            //清除房间设备列表缓存
                            service.HashDelete("RoomDevices", account + "|" + posid);
                            log.Debug($"清除房间设备列表缓存RoomDevices {account}|{posid}");
                            //清除当前设备状态
                            service.HashDelete("DeviceStatus", cachekey);
                            log.Debug($"清除当前设备状态DeviceStatus {cachekey}");
                            //清除当前设备类型的设备列表缓存
                            service.HashDelete("TypeDevices", account + "|" + deviceEntity.devtype);
                            log.Debug($"清除当前设备类型的设备列表缓存TypeDevices {account}|{deviceEntity.devtype}");

                            log.Debug($"822 OK,删除设备成功！返回信息：{msgResult}");
                            return msgResult;
                        }
                        else
                        {
                            log.Debug($"822 Fail,删除设备失败，设备不存在！");
                            return null;
                        }
                    }
                }
                else
                {
                    log.Debug($"822 Fail,删除设备失败，命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 重命名设备
        /// <summary>
        ///1.告诉主机重命名设备名称  解压（射灯,0115170616344） 新名,房间id
        ///connect user:123_Server type:other msg:123_DCD9165057AD;8;823;01240943509560;5bCE54GvLDAxMTUxNzA2MTYzNDQ=$/r$
        ///2.主机返回改名成功renameok@1041140155612
        ///user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;823;1041140155612;Zip;H4sIAAAAAAAAACtKzUvMTc3PdjA0MDE0NDEwNDU1MzQCAFBUoP4WAAAA$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static string Host823(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    string appUser = msg.Split(';')[0];
                    string account = appUser.Split('_')[0];
                    string deviceId = msg.Split(';')[3];
                    string newName_Pid_ya = msg.Split(';')[4].Replace("$/r$", "");
                    string newName_Pid = EncryptionHelp.Decrypt(newName_Pid_ya, false);

                    string newName = newName_Pid.Split(',')[0];
                    string posid = newName_Pid.Split(',')[1];
                    
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        //根据房间id+设备id查询要改名的设备
                        var deviceEntity = hsfDBContext.host_device.Where(t => t.deviceid == deviceId && t.devposition == posid && t.deletemark == 0).FirstOrDefault();
                        if (deviceEntity != null)
                        {

                            deviceEntity.chinaname = newName;//改名
                            deviceEntity.modifiyuser = appUser;
                            deviceEntity.modifiytime = DateTime.Now;
                            hsfDBContext.SaveChanges();

                            //2.主机返回app删除成功
                            string msgResult = $"{appUser};823;{deviceId};Zip;H4sIAAAAAAAEACtKzUvMTc3PBgC88yB7CAAAAA==$/r$";//拼接 renameok

                            //清除房间设备列表缓存
                            service.HashDelete("RoomDevices", account + "|" + posid);
                            log.Debug($"清除房间设备列表缓存RoomDevices {account}|{posid}");
                            //清除当前设备类型的设备列表缓存
                            service.HashDelete("TypeDevices", account + "|" + deviceEntity.devtype);
                            log.Debug($"清除当前设备类型的设备列表缓存TypeDevices {account}|{deviceEntity.devtype}");

                            log.Debug($"823 OK,设备重命名成功！返回信息：{msgResult}");
                            return msgResult;
                        }
                        else
                        {
                            log.Debug($"823 Fail,设备重命名失败，设备不存在！");
                            return null;
                        }
                    }
                }
                else
                {
                    log.Debug($"823 Fail,设备重命名失败，命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 设备状态改变
        /// <summary>
        /// 8145关闭设备 8;8135;设备id   
        /// user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8;8145;01120925117040;3,0$/r$
        /// 8135打开设备 8;8135;设备id
        /// user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8;8135;01120925117040;2;8$/r$
        /// {"code":1002,"id":"010000124b0014c6aaee","ep":1,"serial":1,"control":{"on":true},"result":0,"zigbee":"00ff2c2c2c6a6f005979"}
        /// user:DAJCHSF_% type:other msg:DAJCHSF_Server;devrefresh;1041656180510,true,DAJCHSF_2047DABEF936$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static bool DeviceStateChange(string msg)//, string code, bool state, string success, out string relayUser
        {
            try
            {
                //string appUser = msg.Split(';')[0];
                string code = msg.Split(';')[2];
                string device = msg.Split(';')[3];
                bool state;
                if (code=="8135")
                {
                    state = true;
                }
                else if (code == "8145")
                {
                    state = false;
                }
                else
                {
                    return false;//指令不对返回失败
                }

                using (HsfDBContext hsfDBContext = new HsfDBContext())
                {
                    var deviceEntity = hsfDBContext.host_device.Where(t => t.deviceid == device && t.deletemark == 0).FirstOrDefault();//注意device的唯一性
                    if (deviceEntity != null)
                    {
                        string cachekey = deviceEntity.cachekey;//缓存处理--

                        Zigbee1002 zigbee1002 = new Zigbee1002()
                        {
                            code = 1002,
                            id = deviceEntity.devmac,//010000124b0014c6aaee
                            ep = Convert.ToInt32(deviceEntity.devport),
                            serial = 1,
                            control = new Control { on = state },
                            result = 0,
                            zigbee = deviceEntity.devchannel//00ff2c2c2c6a6f005979
                        };
                        YunZigClient.SendMsg(JsonConvert.SerializeObject(zigbee1002));//发送
                        if (StateResult(cachekey, state))
                        {
                            //返回广播消息,直接开始广播
                            //user:hiddenpath_% type:other msg:hiddenpath_Server;devrefresh;924150429051,false,hiddenpath_ASDFDSSE123$/r$
                            //relayUser = appUser.Split('_')[0] + "_%";
                            //relayUser = appUser;
                            //return $"{appUser};{code};{device};Zip;{success}$/r$";//ok//closeok@808 181248576
                            //return $"{relayUser};devrefresh;{device},{state},{appUser}$/r$";//后面的发给前面的，与请求的对调一下
                            return true;
                        }
                        else
                        {
                            log.Debug($"设备状态改变超时失败！ cachekey：{cachekey}！");
                            //relayUser = appUser;
                            //return $"{appUser};{code};{device};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//error
                            return false;
                        }
                    }
                    else
                    {
                        //relayUser = appUser;
                        return false;//error
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 确认器，读取缓存状态，半秒读取一次，共读取3秒左右，没反应就返回失败
        /// </summary>
        /// <param name="cachekey"></param>
        /// <param name="ok"></param>
        /// <returns></returns>
        public static bool StateResult(string cachekey, bool ok)
        {
            bool state = false;
            int i = 0;
            while (i < 10)
            {
                i++;
                Thread.Sleep(300);
                //读取缓存状态
                string status = service.HashGet("DeviceStatus", cachekey);
                if (status == ok.ToString())
                {
                    log.Debug($"设备状态改变成功！ cachekey：{cachekey}！轮询次数： {i}");
                    state = true;
                    break;
                }
            }
            return state;
        }
        #endregion

        #endregion



        #region 处理Nlp通讯
        /// <summary>
        /// 处理Nlp请求req
        /// </summary>
        /// <param name="msg">
        ///1.二楼全开
        ///connect user:Nlp_Server type:home msg:Nlp_Server
        ///connect user:Nlp_Server type:other msg:123_25995;5;513;5omT5byA5byA5YWz$/r$
        ///
        ///connect user:Nlp_Server type:other msg:123_17920;5;513;5omT5byA6ZiB5qW8$/r$
        ///123_17920;513;5omT5byA6ZiB5qW8;Zip;H4sIAAAAAAAEAAEYAOf/5omT5byA77ya6ZiB5qW844CC5aSx6LSlgj8c4BgAAAA=$/r$
        ///
        ///2.返回结果  开启所有灯光、音乐、窗帘4
        ///connect user:25995_ac83f317b8c7 type:other msg:25995_ac83f317b8c7;513;5LqM5qW85YWo5byA;Zip;H4sIAAAAAAAAAHu6p+HphPXPOhuezel83rj+aWvn44bGl/M3P9k5Ach4vmr60x0zTAAnYIhxJQAAAA==$/r$
        ///123_25995;513;5omT5byA5byA5YWz;Zip;H4sIAAAAAAAEAHvWOfnpnob3e2Y9m7nrRfNeIPtp62YTk8cNTcgCpqZoAmZmaAIGBmgChoZoAkZGaALGxmgCieh8dIEkdD66QDI6H10gBY2fisZPQ+OnA/lPl2x8sWUpAHyIeM8pAQAA$/r$
        /// </param>
        public static string HostNlpRequest(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 3)
                {
                    string session_account = msg.Split(';')[0];
                    if (session_account.Contains("_"))
                    {
                        string account = session_account.Split('_')[1];
                        string req = msg.Split(';')[3].Replace("$/r$", "");
                        string deviceStr = EncryptionHelp.Decrypt(req, false);//解码无zip
                        string code = "";
                        string msgResult = "";
                        string actionStr = "";
                        if (deviceStr.Contains("开"))
                        {
                            actionStr = "打开";
                            code = "8135";
                            deviceStr = deviceStr.Replace("打开", "");
                        }
                        else if (deviceStr.Contains("关"))
                        {
                            actionStr = "关闭";
                            code = "8145";
                            deviceStr = deviceStr.Replace("关闭", "");
                        }


                        if (!string.IsNullOrEmpty(code))
                        {
                            List<host_device> deviceList = null;
                            //获取当前房间的设备列表，先找缓存
                            string devices = service.HashGet("AccountDevices", account);

                            if (!string.IsNullOrEmpty(devices))
                            {
                                deviceList = JsonConvert.DeserializeObject<List<host_device>>(devices);//list多件开关,ALL数组
                            }
                            else
                            {
                                using (HsfDBContext hsfDBContext = new HsfDBContext())
                                {
                                    deviceList = hsfDBContext.host_device.Where(t => t.account == account && t.deletemark == 0).ToList();
                                    service.HashSet("AccountDevices", account, JsonConvert.SerializeObject(deviceList));
                                }
                            }
                            if (deviceList.Count() != 0)
                            {
                                var deviceControl= deviceList.Where(t => t.chinaname.Contains(deviceStr)).ToList();//包含“开关”名称的所有设备
                                if (deviceControl.Count() != 0)
                                {
                                    string okDevices = "";
                                    string failDevices = "";
                                    foreach (var item in deviceControl)
                                    {
                                        if(DeviceStateChange($"{session_account};8;{code};{item.deviceid};$/r$"))
                                        {
                                            okDevices += item.chinaname + "。";
                                        }
                                        else
                                        {
                                            failDevices += item.chinaname + "。";
                                        }
                                    }
                                    string resultDevices = "";
                                    if (!string.IsNullOrEmpty(okDevices))
                                    {
                                        resultDevices += $"已经为您{actionStr}：{okDevices}";
                                    }
                                    if (!string.IsNullOrEmpty(failDevices))
                                    {
                                        resultDevices += $"{actionStr}：{failDevices}失败";
                                    }
                                    msgResult = $"{session_account};513;{req};Zip;{EncryptionHelp.Encryption(resultDevices, true)}$/r$";
                                    log.Debug($"设备状态改变成功！ cachekey：{msgResult}");
                                    return msgResult;
                                }
                                else
                                {

                                    log.Debug($"不存在该设备！ {req}");
                                    return null;
                                }
                                
                            }
                            else
                            {
                                log.Debug($"不存在该账户的设备列表！ {msg}");
                                return null;
                            }
                        }
                        else
                        {
                            log.Debug($"code不符合规范！ {msg}");
                            return null;
                        }
                    }
                    else
                    {
                        log.Debug($"命令不符合规范！ {msg}");
                        return null;
                    }
                }
                else
                {
                    log.Debug($"命令不符合规范！ {msg}");
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

    }
}