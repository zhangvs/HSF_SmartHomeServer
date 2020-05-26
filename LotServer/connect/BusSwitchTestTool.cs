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
    public class BusSwitchTestTool
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("BusSwitchTestTool");

        #region 总线开关测试工具相关

        #region 876返回总线网关当前的总线开关
        /// <summary>
        /// 获得总线网关要添加的设备信息sendsocket(8, "8;876;"+zhujitype+";"+devmac);//按一下总线开关，缓存总线开关mac地址，点击总线网关，返回当前要添加的几键开关
        /// </summary>
        /// <param name="msg">1.查询总线网关下的开关connect user:123_Server type:other msg:123_e0ddc0a405d9;8;876;Safe_Center;58.57.32.162$/r$
        /// 2.返回结果 123_e0ddc0a405d9;876;all;Zip;H4sIAAAAAAAEAGWQyw6CMBRE/6VrQ3gENLI38RuMi5tykRv6SlswxPjvoryqLufMTDvt5cHOFTuqTogdq7AnjrRq3pACBRJXIMC5msKCH8xoszhjU98EngQ+WvkhyvdRlkZJkZanpIyLKWq09evBwBtscQjKRjvypFWAeANKoVgISbgFazuHdlNG33sQHQZ158H/6eQXpAsAznWn1omfx8xrLY7B933fxNP2VVJXVNMQhmYUpioU6FGCbSfyvL4AcWAUMpEBAAA=$/r$
        /// </param>
        public static string Host876(string msg)
        {
            try
            {
                if (msg.Split(';').Length >= 4)
                {
                    using (RedisStringService service = new RedisStringService())
                    {
                        string appUser = msg.Split(';')[0];
                        string account = appUser.Split('_')[0];
                        string devmac = msg.Split(';')[4].Replace("$/r$", "");//本地外网ip
                        string BusSwitchMac = service.Get("BusSwitch_" + devmac);//先提前按一下，瑞瀛网关服务器缓存开关mac地址，两分钟
                        if (!string.IsNullOrEmpty(BusSwitchMac))
                        {
                            BusSwitchMac = BusSwitchMac.Replace("\"", "");
                            string[] macs = BusSwitchMac.Split(';');
                            List<host_device> deviceList = new List<host_device>();
                            foreach (var item in macs)
                            {
                                host_device host_Device = new host_device()//可能是个数组，有多个开关同时按的情况
                                {
                                    devmac = devmac + ";F1;" + BusSwitchMac,
                                    devtype = "03",
                                    devport = "0"
                                };
                                deviceList.Add(host_Device);
                            }

                            string zipStr = EncryptionHelp.Encryption(JsonConvert.SerializeObject(deviceList), true);
                            string msgResult = $"{appUser};876;all;Zip;{zipStr}$/r$";//拼接
                            log.Debug($"876 OK,返回设备类型的设备列表成功！返回信息：{msgResult}");
                            return msgResult;
                        }
                        return null;
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

        #region 8231总线控制器编辑，含mac地址
        /// <summary>
        ///1.修改总线开关mac地址  解压（总线开关01,0115170616344,58.57.32.162;F1;06,58.57.32.162;F1;01） 新名,房间id,老的地址,新的地址
        ///connect user:123_Server type:other msg:123_DCD9165057AD;8;823;01240943509560;5bCE54GvLDAxMTUxNzA2MTYzNDQ=$/r$
        ///2.主机返回改名成功renameok@1041140155612
        ///user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;823;1041140155612;Zip;H4sIAAAAAAAAACtKzUvMTc3PdjA0MDE0NDEwNDU1MzQCAFBUoP4WAAAA$/r$
        /// </summary>
        /// <param name="msg"></param>
        public static string Host8231(string msg)
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
                    string oldDevmac = newName_Pid.Split(',')[2];//总线开关旧的mac地址
                    string newDevmac = newName_Pid.Split(',')[3];//总线开关新的mac地址
                    if (!string.IsNullOrEmpty(oldDevmac) && !string.IsNullOrEmpty(newDevmac) && oldDevmac != newDevmac)
                    {
                        //发送修改mac地址指令到网关服务器
                        bool isModify = RYZigMacModifyMsg(oldDevmac, newDevmac);
                        if (isModify)
                        {
                            using (HsfDBContext hsfDBContext = new HsfDBContext())
                            {
                                using (RedisHashService service = new RedisHashService())
                                {
                                    //根据老mac检索出同一开关下的继电器，一起修改mac
                                    List<host_device> deviceList = hsfDBContext.host_device.Where(t => t.devmac == oldDevmac && t.devposition == posid && t.account == account && t.deletemark == 0).ToList();
                                    if (deviceList.Count() != 0)
                                    {
                                        string devtype = deviceList[0].devtype;
                                        foreach (var item in deviceList)
                                        {
                                            //修改mac，缓存状态也要对应的修改,否则显示离线
                                            //先拿到之前的key状态，删掉，再赋值给新的key
                                            string st = service.GetValueFromHash("DeviceStatus", item.cachekey);
                                            service.RemoveEntryFromHash("DeviceStatus", item.cachekey);

                                            //当前名称变动的话，修改名称
                                            if (deviceId == item.deviceid)
                                            {
                                                item.chinaname = newName;//改名
                                            }
                                            item.devmac = newDevmac;//改mac
                                            item.devip = newDevmac;//改mac
                                            item.cachekey = newDevmac + "_" + item.devport;//改mac
                                            item.modifiyuser = appUser;
                                            item.modifiytime = DateTime.Now;
                                            hsfDBContext.SaveChanges();

                                            //再赋值给新的key{"String 引用没有设置为 String 的实例。\r\n参数名: s"}
                                            if (!string.IsNullOrEmpty(st))
                                            {
                                                service.SetEntryInHash("DeviceStatus", item.cachekey, st);
                                                log.Debug($"改名key缓存新的状态 {item.cachekey} ：{st}");
                                            }
                                            else
                                            {
                                                service.SetEntryInHash("DeviceStatus", item.cachekey, "False");
                                                log.Debug($"改名key缓存新的状态 {item.cachekey} 默认：False");
                                            }

                                            //清除当前设备缓存
                                            service.RemoveEntryFromHash("DeviceEntity", item.deviceid);
                                            log.Debug($"清除设备缓存DeviceEntity {item.deviceid}");
                                        }
                                        //2.主机返回app修改成功
                                        msgResult = $"{appUser};8231;{deviceId};Zip;H4sIAAAAAAAEACtKzUvMTc3PBgC88yB7CAAAAA==$/r$";//拼接 renameok

                                        //清除房间设备列表缓存
                                        service.RemoveEntryFromHash("RoomDevices", account + "|" + posid);
                                        log.Debug($"清除房间设备列表缓存RoomDevices {account}|{posid}");
                                        //清除当前设备类型的设备列表缓存
                                        service.RemoveEntryFromHash("TypeDevices", account + "|" + devtype);
                                        log.Debug($"清除当前设备类型的设备列表缓存TypeDevices {account}|{devtype}");

                                        log.Debug($"8231 OK,设备重命名成功！返回信息：{msgResult}");
                                        return msgResult;
                                    }
                                    else
                                    {
                                        log.Debug($"8231 Fail,修改mac地址数据库失败，设备不存在！");
                                        return null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            log.Debug($"8231 Fail,修改mac地址失败，修改失败！");
                            msgResult = $"{appUser};8231;{deviceId};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//拼接 error
                            return msgResult;
                        }
                    }
                    else
                    {
                        log.Debug($"8231 Fail,修改mac地址失败，mac地址不能为空,新旧地址不能相同！");
                        msgResult = $"{appUser};8231;{deviceId};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";//拼接 error
                        return msgResult;
                    }
                }
                else
                {
                    log.Debug($"8231 Fail,设备重命名失败，命令不符合规范！");
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 瑞瀛网关修改新的mac地址
        /// 01 06 00 02 01 03 69 9B
        /// 第六位 03 为开关新的物理地址
        /// CC DD F1 01 06 00 02 01 06 a9 98
        /// 更新固件之后mac统一设置为01之后
        /// 发送修改：
        /// cc dd F1 01 06 00 02 01 05 e9 99
        /// cc dd f1 01 06 00 02 05 01 ea 9a 
        /// 
        /// cc dd f1 01 06 00 02 05 01 ea 9a 
        /// cc dd f1 01 06 00 02 02 01 e8 aa 
        /// </summary>
        /// <param name="devmac">192.168.1.55;f1;50000</param>
        /// <returns></returns>
        public static bool RYZigMacModifyMsg(string oldDevmac, string newDevmac)
        {
            string old_ip = oldDevmac.Split(';')[0];//192.168.1.55
            string old_ff = oldDevmac.Split(';')[1];//f1
            string old_mac = "01";//oldDevmac.Split(';')[2];//06老mac统一改为01

            string new_ip = newDevmac.Split(';')[0];//192.168.1.55
            string new_ff = newDevmac.Split(';')[1];//f1
            string new_mac = newDevmac.Split(';')[2];//06

            string msg1 = $"{old_mac} 06 00 02 01 {new_mac}";
            string crc = EncryptionHelp.CRCCalc(msg1);//crc校验
            string msg = $"cc dd {new_ff} {msg1} {crc}";
            string ipmsg = msg + "|" + newDevmac;//CC DD F1 01 06 00 02 01 06 a9 98|192.168.82.107;f1;06//普通字符串+16进制字符串，cc开头
            string mac16 = RYZigClient.ToHex(ipmsg, "utf-8", false);
            RYZigClient.Send(mac16);//发送到网关服务器YunZig

            log.Debug($"向RYZig网关发送指令{ipmsg}，确认器开始遍历mac是否改变 {new_mac}");
            if (MacResult("BusSwitch_" + new_ip, new_mac))
            {
                return true;
            }
            else
            {
                log.Debug($"设备状态改变超时失败！ cachekey：{new_mac}！");
                return false;
            }
        }
        #endregion

        #region 总线mac地址修改确认器
        /// <summary>
        /// 确认器，读取缓存状态，半秒读取一次，共读取3秒左右，没反应就返回失败
        /// </summary>
        /// <param name="cachekey"></param>
        /// <param name="ok"></param>
        /// <returns></returns>
        public static bool MacResult(string cachekey, string st)
        {
            using (RedisStringService service = new RedisStringService())
            {
                bool state = false;
                int i = 0;
                while (i < 30)//6s，次数限制redis
                {
                    i++;
                    Thread.Sleep(200);
                    //读取缓存状态
                    string BusSwitchMac = service.Get(cachekey);
                    if (!string.IsNullOrEmpty(BusSwitchMac))
                    {
                        BusSwitchMac = BusSwitchMac.Replace("\"", "");
                        if (BusSwitchMac == st)
                        {
                            log.Debug($"设备状态改变成功！ cachekey：{cachekey}！轮询次数： {i}");
                            state = true;
                            break;
                        }
                    }
                    else
                    {
                        log.Debug($"设备状态改变失败！ cachekey：{cachekey} 不存在！请按一下设备");
                    }
                }
                return state;
            }
        }
        #endregion

        #endregion
    }
}
