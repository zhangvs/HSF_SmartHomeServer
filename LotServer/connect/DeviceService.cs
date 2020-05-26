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
    public class DeviceService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("DeviceService");
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
                if (msg.Split(';').Length >= 4)
                {
                    using (RedisHashService service = new RedisHashService())
                    {
                        string appUser = msg.Split(';')[0];
                        string account = appUser.Split('_')[0];
                        string posid = msg.Split(';')[3].Replace("$/r$", "");//房间id  ,默认id为0，
                        List<host_device> deviceList = null;
                        //获取当前房间的设备列表，先找缓存
                        string deviceListJson = service.GetValueFromHash("RoomDevices", account + "|" + posid);
                        if (!string.IsNullOrEmpty(deviceListJson))
                        {
                            deviceList = JsonConvert.DeserializeObject<List<host_device>>(deviceListJson);
                        }
                        else
                        {
                            using (HsfDBContext hsfDBContext = new HsfDBContext())
                            {
                                if (posid == "0")//新app首页获取所有设备account
                                {
                                    //默认房间为0，查询当前账号所有设备列表
                                    deviceList = hsfDBContext.host_device.Where(t => t.account == account && t.deletemark == 0).OrderBy(t => t.createtime).ToList();
                                    ////包括大华的设备
                                    //List<host_device> outdeviceList = GetOutDevice(account);
                                    //foreach (var item in outdeviceList)
                                    //{
                                    //    deviceList.Add(item);//室内+室外
                                    //}
                                    //缓存当前房间的设备列表,不包括状态,不管空与否都缓存，防止第二次还查数据库RoomDevices
                                    service.SetEntryInHash("RoomDevices", account + "|" + posid, JsonConvert.SerializeObject(deviceList));//解决默认posid都为0的问题
                                }
                                else
                                {
                                    //posid房间id
                                    deviceList = hsfDBContext.host_device.Where(t => t.devposition == posid && t.deletemark == 0).OrderBy(t => t.createtime).ToList();
                                    //缓存当前房间的设备列表,不包括状态,不管空与否都缓存，防止第二次还查数据库
                                    service.SetEntryInHash("RoomDevices", account + "|" + posid, JsonConvert.SerializeObject(deviceList));//解决默认posid都为0的问题
                                }
                            }
                        }

                        //真正更新设备状态
                        string zipStr = "";
                        foreach (var item in deviceList)
                        {
                            if (!string.IsNullOrEmpty(item.cachekey))
                            {
                                //读取缓存状态
                                string status = service.GetValueFromHash("DeviceStatus", item.cachekey);
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
                        }
                        zipStr = EncryptionHelp.Encryption(JsonConvert.SerializeObject(deviceList), true);
                        string msgResult = $"{appUser};815;{posid};Zip;{zipStr}$/r$";//拼接
                        log.Debug($"815 OK,返回房间设备列表成功！返回信息：{msgResult}");
                        return msgResult;
                    }
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
        /// <summary>
        /// 获取室外设备大华
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static string GetOutDevice(string account)
        {
            try
            {
                string outdeviceListStr = "";
                //List<host_device> outdeviceList = new List<host_device>();
                using (RedisHashService service = new RedisHashService())
                {
                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                    {
                        if (account.Split('-').Length >= 3)//length:1开始
                        {
                            //室外，大华产品
                            string _residential = account.Split('-')[0];//小区
                            string _building = account.Split('-')[1];//楼号
                            string _unit = account.Split('-')[2];//单元
                            string cachekey = _residential + "-" + _building + "-" + _unit;//默认房间

                            //先查单元对应的缓存，没有再缓存
                            string outDeviceListJson = service.GetValueFromHash("OutDevices", cachekey);
                            if (!string.IsNullOrEmpty(outDeviceListJson))
                            {
                                //outdeviceList = JsonConvert.DeserializeObject<List<host_device>>(outDeviceListJson);
                                return outDeviceListJson;
                            }
                            else
                            {
                                List<hsf_outdevice> dh_deviceList = hsfDBContext.hsf_outdevice.Where(t => t.residential == _residential && t.deletemark == 0).ToList();
                                //foreach (var item in dh_deviceList)
                                //{
                                //    switch (item.devtype)
                                //    {
                                //        case "Dahua_EntranceGuard"://门禁
                                //            host_device EntranceGuard = new host_device()
                                //            {
                                //                deviceid = item.deviceid,
                                //                chinaname = item.chinaname,
                                //                devtype = item.devtype
                                //            };
                                //            outdeviceList.Add(EntranceGuard);
                                //            break;
                                //        case "Dahua_UnitDoor"://大华单元门口机
                                //            if (_building == item.building && _unit == item.unit)
                                //            {
                                //                host_device UnitDoor = new host_device()
                                //                {
                                //                    deviceid = item.deviceid,
                                //                    chinaname = item.chinaname,
                                //                    devtype = item.devtype
                                //                };
                                //                outdeviceList.Add(UnitDoor);
                                //            }
                                //            break;
                                //        case "Elevator"://电梯
                                //            if (_building == item.building && _unit == item.unit)
                                //            {
                                //                host_device Elevator = new host_device()
                                //                {
                                //                    deviceid = item.deviceid,
                                //                    chinaname = item.chinaname,
                                //                    devtype = item.devtype
                                //                };
                                //                outdeviceList.Add(Elevator);
                                //            }
                                //            break;
                                //        default:
                                //            break;
                                //    }
                                //}
                                outdeviceListStr = EncryptionHelp.Encryption(JsonConvert.SerializeObject(dh_deviceList), false);
                                service.SetEntryInHash("OutDevices", cachekey, outdeviceListStr);//缓存室外有权限控制的设备OutDevices
                                return outdeviceListStr;
                            }
                        }
                        else
                        {
                            return "error:host format error";
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
                    using (RedisHashService service = new RedisHashService())
                    {
                        string appUser = msg.Split(';')[0];
                        string account = appUser.Split('_')[0];
                        string devtypeStr = msg.Split(';')[4].Replace("$/r$", "");//设备类型列表
                        string[] devtypes = devtypeStr.Split(',');
                        List<host_device> deviceList = null;
                        //获取当前设备类型的设备列表，先找缓存
                        string deviceListJson = service.GetValueFromHash("TypeDevices", account + "|" + devtypeStr);
                        if (!string.IsNullOrEmpty(deviceListJson))
                        {
                            deviceList = JsonConvert.DeserializeObject<List<host_device>>(deviceListJson);
                        }
                        //如果缓存中没有，再查数据库
                        else
                        {
                            using (HsfDBContext hsfDBContext = new HsfDBContext())
                            {
                                deviceList = hsfDBContext.host_device.Where(t => t.account == account && devtypes.Contains(t.devtype) && t.deletemark == 0).OrderBy(t => t.createtime).ToList();
                                //缓存当前设备类型的设备列表,不包括状态,不管空与否都缓存，防止第二次还查数据库
                                service.SetEntryInHash("TypeDevices", account + "|" + devtypeStr, JsonConvert.SerializeObject(deviceList));//解决默认posid都为0的问题
                            }
                        }

                        //真正更新设备状态
                        string zipStr = "";
                        foreach (var item in deviceList)
                        {
                            //读取缓存状态
                            string status = service.GetValueFromHash("DeviceStatus", item.cachekey);
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
        /// [{"NewRecord":true,"_id":0,"chinaname":"ac83f314eaf7","classfid":"8","devalarm":"","devchannel":"","deviceid":"12151421044360","devip":"123","devmac":"192.168.88.102","devpara":{"close":"关闭,211,1,192.168.88.102","open":"开启,211,1,192.168.88.102"},"devport":"","devposition":"1211144601960","devregcode":"123","devstate":"","devstate1":"","devstate2":"","devtype":"AI_Mengdou","imageid":"dev105","lgsort":0,"powvalue":"","userid":"wali_Server"}]
        /// 
        /// </param>
        public static string AddDeviceToRoom(string msg, string code)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    using (RedisHashService service = new RedisHashService())
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
                                if (!string.IsNullOrEmpty(item.devport))
                                {
                                    cachekey = item.devmac + "_" + item.devport;//存在mac相同，端口不相同的多键设备
                                }
                                else
                                {
                                    cachekey = item.devmac;//存在mac相同，端口不相同的多键设备
                                }

                                var deviceEntity = hsfDBContext.host_device.Where(t => t.cachekey == cachekey && t.deletemark == 0).FirstOrDefault();
                                if (deviceEntity != null)
                                {
                                    //deviceEntity.deletemark = 1;
                                    //deviceEntity.modifiyuser = appUser;
                                    //deviceEntity.modifiytime = DateTime.Now;
                                    hsfDBContext.host_device.Remove(deviceEntity);//真实删除
                                    AddDeviceEntity(hsfDBContext, item, appUser, account, mac);
                                    log.Debug($"{code} OK,重新添加设备成功！");
                                }
                                else
                                {
                                    //当前房间id需要保存,网关房间id为0，不可以
                                    AddDeviceEntity(hsfDBContext, item, appUser, account, mac);
                                }

                                string statusStr = service.GetValueFromHash("DeviceStatus", cachekey);
                                if (string.IsNullOrEmpty(statusStr))
                                {
                                    //缓存状态不存在的，先设备状态默认为False，在线，关闭
                                    service.SetEntryInHash("DeviceStatus", cachekey, "False");
                                }
                            }
                            //如果设备网关字段不为空，则查询网关最初状态，可能不全部关闭？
                            if (!string.IsNullOrEmpty(devchannel) && devtype.Contains("Zigbee"))
                            {
                                YunZigClient.SendMsg($"{{\"code\" :5001,\"serial\": 11111,\"device\":[{{\"id\": \"{devmac}\"}}],\"zigbee\":\"{devchannel}\"}}");//zigbee查询初始状态，向网关
                            }

                            //1清除房间设备列表缓存
                            service.RemoveEntryFromHash("RoomDevices", account + "|" + posid);
                            if (posid!="0")
                            {
                                service.RemoveEntryFromHash("RoomDevices", account + "|0");//房间内设备变动时，0所有设备的缓存也随之清除
                            }
                            log.Debug($"1.添加设备,清除房间设备列表缓存RoomDevices {account}|{posid}");
                            //2清除当前设备类型的设备列表缓存
                            service.RemoveEntryFromHash("TypeDevices", account + "|" + devtype);
                            log.Debug($"2.添加设备,清除当前设备类型的设备列表缓存TypeDevices {account}|{devtype}");
                            //3同步DuerOS设备
                            DuerOSClient.PutDeviceChangeQueue(account);
                            log.Debug($"3.添加设备,同步DuerOS设备 {account}");

                            //主机返回app添加成功
                            string msgResult = $"{appUser};{code};ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$";//拼接
                            log.Debug($"{code} OK,添加设备成功！返回信息：{msgResult}");
                            return msgResult;
                        }
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

        #region 822删除设备
        /// <summary>
        ///1.告诉主机删除设备
        ///user:123_Server type:other msg:123_DCD9165057AD;8;822;1041656180510$/r$
        ///2.主机返回删除成功delok@105 124612 6590
        ///user:123_DCD9165057AD type:other msg:123_DCD9165057AD;822;1041656180510;Zip;H4sIAAAAAAAAAEtJzcnPdjA0MDE0MzUztDAwNTQAAJxecTETAAAA$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static string Host822(string msg)
        {
            string msgResult = "";
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
                        using (RedisHashService service = new RedisHashService())
                        {
                            //能否确定deviceid唯一，用时间来做标识？？隐患 :添加用户限定排除风险
                            var deviceEntity = hsfDBContext.host_device.Where(t => t.deviceid == deviceId && t.account == account && t.deletemark == 0).FirstOrDefault();
                            if (deviceEntity != null)
                            {
                                posid = deviceEntity.devposition;
                                cachekey = deviceEntity.devmac + "_" + deviceEntity.devport;//存在mac相同，端口不相同的多键设备

                                //deviceEntity.deletemark = 1;
                                //deviceEntity.modifiyuser = appUser;
                                //deviceEntity.modifiytime = DateTime.Now;
                                hsfDBContext.host_device.Remove(deviceEntity);//真实删除
                                hsfDBContext.SaveChanges();

                                //2.主机返回app删除成功
                                msgResult = $"{appUser};822;{deviceId};Zip;H4sIAAAAAAAEAEtJzcnPBgBd3KDfBQAAAA==$/r$";//拼接 delok

                                //1清除房间设备列表缓存
                                service.RemoveEntryFromHash("RoomDevices", account + "|" + posid);
                                log.Debug($"1.删除设备,清除房间设备列表缓存RoomDevices {account}|{posid}");
                                if (posid != "0")
                                {
                                    service.RemoveEntryFromHash("RoomDevices", account + "|0");//房间内设备变动时，0所有设备的缓存也随之清除
                                }
                                //2清除当前设备状态
                                service.RemoveEntryFromHash("DeviceStatus", cachekey);
                                log.Debug($"2.删除设备,清除当前设备状态DeviceStatus {cachekey}");
                                //3清除当前设备类型的设备列表缓存
                                service.RemoveEntryFromHash("TypeDevices", account + "|" + deviceEntity.devtype);
                                log.Debug($"3.删除设备,清除当前设备类型的设备列表缓存TypeDevices {account}|{deviceEntity.devtype}");
                                //4同步DuerOS设备
                                DuerOSClient.PutDeviceChangeQueue(account);
                                log.Debug($"4.删除设备,同步DuerOS设备 {account}");


                                log.Debug($"822 OK,删除设备成功！返回信息：{msgResult}");
                                return msgResult;
                            }
                            else
                            {
                                log.Debug($"822 Fail,删除设备失败，设备不存在！");
                                msgResult = $"{appUser};822;{deviceId};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//拼接 error
                                return msgResult;
                            }
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

        #region 823重命名设备
        /// <summary>
        ///1.告诉主机重命名设备名称  解压（射灯,0115170616344） 新名,房间id
        ///connect user:123_Server type:other msg:123_DCD9165057AD;8;823;01240943509560;5bCE54GvLDAxMTUxNzA2MTYzNDQ=$/r$
        ///2.主机返回改名成功renameok@1041140155612
        ///user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;823;1041140155612;Zip;H4sIAAAAAAAAACtKzUvMTc3PdjA0MDE0NDEwNDU1MzQCAFBUoP4WAAAA$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static string Host823(string msg)
        {
            string msgResult = "";
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
                        using (RedisHashService service = new RedisHashService())
                        {
                            //根据房间id+设备id查询要改名的设备
                            var deviceEntity = hsfDBContext.host_device.Where(t => t.deviceid == deviceId && t.devposition == posid && t.deletemark == 0).FirstOrDefault();
                            if (deviceEntity != null)
                            {

                                deviceEntity.chinaname = newName;//改名
                                deviceEntity.modifiyuser = appUser;
                                deviceEntity.modifiytime = DateTime.Now;
                                hsfDBContext.SaveChanges();

                                //2.主机返回app重命名成功
                                msgResult = $"{appUser};823;{deviceId};Zip;H4sIAAAAAAAEACtKzUvMTc3PBgC88yB7CAAAAA==$/r$";//拼接 renameok

                                //清除房间设备列表缓存
                                service.RemoveEntryFromHash("RoomDevices", account + "|" + posid);
                                log.Debug($"1.设备重命名,清除房间设备列表缓存RoomDevices {account}|{posid}");
                                if (posid != "0")
                                {
                                    service.RemoveEntryFromHash("RoomDevices", account + "|0");//房间内设备变动时，0所有设备的缓存也随之清除
                                }
                                //清除当前设备类型的设备列表缓存
                                service.RemoveEntryFromHash("TypeDevices", account + "|" + deviceEntity.devtype);
                                log.Debug($"2.设备重命名,清除当前设备类型的设备列表缓存TypeDevices {account}|{deviceEntity.devtype}");
                                //4同步DuerOS设备
                                DuerOSClient.PutDeviceChangeQueue(account);
                                log.Debug($"3.设备重命名,同步DuerOS设备 {account}");

                                log.Debug($"823 OK,设备重命名成功！返回信息：{msgResult}");
                                return msgResult;
                            }
                            else
                            {
                                log.Debug($"823 Fail,设备重命名失败，设备不存在！");
                                msgResult = $"{appUser};823;{deviceId};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//拼接 error
                                return msgResult;
                            }
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

        #region 设备状态改变8145/8135
        /// <summary>
        /// 8145关闭设备 8;8135;设备id   
        /// user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8;8145;01120925117040;3,0$/r$
        /// 8135打开设备 8;8135;设备id
        /// user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8;8135;01120925117040;2;8$/r$
        /// user:123_Server type:other msg:123_e0ddc0a405d9;8;8135;A$/r$
        /// {"code":1002,"id":"010000124b0014c6aaee","ep":1,"serial":1,"control":{"on":true},"result":0,"zigbee":"00ff2c2c2c6a6f005979"}
        /// user:DAJCHSF_% type:other msg:DAJCHSF_Server;devrefresh;1041656180510,true,DAJCHSF_2047DABEF936$/r$
        /// 
        /// user:MMSJ-1#1-5-501 type:other msg:MMSJ-1-1-5-501;8;8145;08$/r$
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static bool DeviceStateChange(string msg)//, string code, bool state, string success, out string relayUser
        {
            try
            {
                string appUser = msg.Split(';')[0];
                string code = msg.Split(';')[2];
                string deviceId = msg.Split(';')[3].Replace("$/r$", "");//新app设备id为最后一位
                bool state;
                if (code == "8135")
                {
                    state = true;
                }
                else if (code == "8145")
                {
                    state = false;
                }
                else
                {
                    log.Debug($"{msg}code指令不对返回失败！");
                    return false;//指令不对返回失败
                }

                using (RedisHashService service = new RedisHashService())
                {
                    string deviceEntityStr = service.GetValueFromHash("DeviceEntity", deviceId);//8231有关联，改mac的情况下，其它改状态，改名称，不需要清理
                    host_device deviceEntity = null;
                    if (!string.IsNullOrEmpty(deviceEntityStr))
                    {
                        deviceEntity = JsonConvert.DeserializeObject<host_device>(deviceEntityStr);//设备实体缓存
                    }
                    else
                    {
                        using (HsfDBContext hsfDBContext = new HsfDBContext())
                        {
                            deviceEntity = hsfDBContext.host_device.Where(t => t.deviceid == deviceId && t.deletemark == 0).FirstOrDefault();//注意device的唯一性
                            if (deviceEntity != null)
                            {
                                //缓存设备id与设备实体对应关系，避免查询数据库
                                service.SetEntryInHash("DeviceEntity", deviceId, JsonConvert.SerializeObject(deviceEntity));
                            }
                        }
                    }

                    if (deviceEntity != null)
                    {
                        //拼装1002指令，发送给网关，执行改变状态操作
                        return ChangeStateMain.StateChangeByType(deviceEntity, state);
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
        #endregion



        #region 室外设备
        /// <summary>
        /// user:MMSJ-1#1-5-501 type:other msg:MMSJ-1-1-5-501;8;8133;08$/r$
        /// connect user:MMSJ-1-1-30-3001_C40BCB80050A type:other msg:MMSJ-1-1-30-3001_C40BCB80050A;8;8133;08$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static bool OutDeviceStateChange(string appUser, string deviceId)
        {
            try
            {
                bool state = true;//室外设备全是打开
                using (RedisHashService service = new RedisHashService())
                {
                    string deviceEntityStr = service.GetValueFromHash("OutDeviceEntity", deviceId);//8231有关联，改mac的情况下，其它改状态，改名称，不需要清理
                    hsf_outdevice deviceEntity = null;
                    if (!string.IsNullOrEmpty(deviceEntityStr))
                    {
                        deviceEntity = JsonConvert.DeserializeObject<hsf_outdevice>(deviceEntityStr);//设备实体缓存
                    }
                    else
                    {
                        using (HsfDBContext hsfDBContext = new HsfDBContext())
                        {
                            deviceEntity = hsfDBContext.hsf_outdevice.Where(t => t.deviceid == deviceId && t.deletemark == 0).FirstOrDefault();//注意device的唯一性
                            if (deviceEntity != null)
                            {
                                //缓存设备id与设备实体对应关系，避免查询数据库
                                service.SetEntryInHash("OutDeviceEntity", deviceId, JsonConvert.SerializeObject(deviceEntity));
                            }
                        }
                    }

                    if (deviceEntity != null)
                    {
                        //拼装1002指令，发送给网关，执行改变状态操作
                        return ChangeStateMain.OutStateChangeByType(appUser, deviceEntity, state);
                    }
                    else
                    {
                        return false;//error
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
