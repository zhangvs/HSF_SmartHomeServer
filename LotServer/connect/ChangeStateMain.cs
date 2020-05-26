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
    /// <summary>
    /// 状态改变具体执行方法
    /// </summary>
    public class ChangeStateMain
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("ChangeStateMain");

        #region 室内状态改变
        /// <summary>
        /// 状态改变具体执行方法
        /// </summary>
        /// <param name="deviceEntity">设备实体</param>
        /// <param name="state">要改变成什么状态</param>
        /// <returns></returns>
        public static bool StateChangeByType(host_device deviceEntity, bool state)
        {
            object obj = null;
            switch (deviceEntity.devtype)
            {
                case "Smart_ZigbeeCurtain"://zigbee窗帘
                    obj = new { pt = state ? 100 : 0 };
                    return YunZigSendMsg(deviceEntity, obj, state);
                case "Panel_Zigbee"://zigbee开关
                    obj = new { on = state };
                    return YunZigSendMsg(deviceEntity, obj, state);
                case "Panel_485Bus"://总线开关
                    return RYZigSendMsg(deviceEntity, state);
                default:
                    return false;
            }
        }

        #region zigbee网关
        /// <summary>
        /// zigbee网关发送
        /// 拼装1002指令，发送给网关，执行改变状态操作
        /// 1.Zigbee窗帘(Smart_ZigbeeCurtain)：pt控制窗帘开度百分比，0为全关闭，100为全打开
        /// 速度：打开窗帘230、4289。关闭窗帘229、3599  等104速度太慢，存在关闭的时候104关闭不充分，打开13，关闭86
        /// {"code":1002,"id":"010000124b000f81eea6","ep":8,"serial":1,"control":{"on":false,"pt":0},"result":0,"zigbee":"00ff2c2c2c6a6f0057f3"}//关闭
        /// 2.Zigbee开关(Panel_Zigbee)：on  
        /// 速度：打开开关239、392。关闭开关228、411
        /// {"code":1002,"id":"010000124b0014c5d116","ep":1,"serial":1,"control":{"on":true},"result":0,"zigbee":"00ff2c2c2c6a6f0057f3"}
        /// </summary>
        /// <param name="deviceEntity"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool YunZigSendMsg(host_device deviceEntity, object obj, bool state)
        {
            string cachekey = deviceEntity.cachekey;//缓存处理--
            //发送到zigbee网关并遍历状态
            if (obj != null)
            {
                Zigbee1002 zigbee1002 = new Zigbee1002()
                {
                    code = 1002,
                    id = deviceEntity.devmac,//mac地址，010000124b0014c6aaee
                    ep = Convert.ToInt32(deviceEntity.devport),//端口
                    serial = 1,
                    control = obj,
                    result = 0,
                    zigbee = deviceEntity.devchannel//网关，00ff2c2c2c6a6f005979
                };
                string _1002 = JsonConvert.SerializeObject(zigbee1002);
                YunZigClient.SendMsg(_1002);//发送到网关服务器YunZig

                log.Debug($"向YunZig网关发送1002指令{_1002}，确认器开始遍历状态是否改变{cachekey}");
                if (StateResult(cachekey, state.ToString()))
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
                log.Debug($"设备状态改变失败！cachekey：{cachekey}！ 不存在设备类型：{deviceEntity.devtype}");
                return false;
            }
        }
        #endregion

        #region 瑞瀛网关
        /// <summary>
        /// 瑞瀛网关开关指令发送
        /// </summary>
        /// <param name="deviceEntity"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool RYZigSendMsg(host_device deviceEntity, bool state)
        {
            string devmac = deviceEntity.devmac;//192.168.1.55;f1;50000
            string ip = devmac.Split(';')[0];//192.168.1.55
            string ff = devmac.Split(';')[1];//f1
            string mac = devmac.Split(';')[2];//06
            string port = deviceEntity.devport;//0;1，0;1，0;3，自定义添加的时候做了+1，还多了分号
            int iport = int.Parse(port.Replace(";", "")) - 1;//去分号，减1
            string iportStr = "0" + iport;
            string st = state ? "01" : "00";
            string cachekey = deviceEntity.cachekey;//缓存处理--
            //发送到zigbee网关并遍历状态
            if (!string.IsNullOrEmpty(st))
            {
                //cc dd 开头固定
                //f1 第一个485通道
                //06 物理地址
                //05 00 08
                //00 继电器
                //01 为打开继电器
                //8D C8 为crc校验码
                string msg1 = $"{mac} 05 00 08 {iportStr} {st}";
                string crc = EncryptionHelp.CRCCalc(msg1);//crc校验
                string msg = $"cc dd {ff} {msg1} {crc}";
                string ipmsg = msg + "|" + devmac;//ccddf10605000800018c7f|192.168.82.107;f1;06//普通字符串+16进制字符串
                string mac16 = RYZigClient.ToHex(ipmsg, "utf-8", false);
                RYZigClient.Send(mac16);//发送到网关服务器YunZig

                log.Debug($"向RYZig网关发送指令{ipmsg}，确认器开始遍历状态是否改变 {state.ToString()}");
                if (StateResult(cachekey, state.ToString()))
                {
                    return true;
                }
                else
                {
                    log.Debug($"设备状态改变超时失败！ cachekey：{cachekey}！");
                    return false;
                }
            }
            else
            {
                log.Debug($"设备状态改变失败！cachekey：{cachekey}！ 不存在设备类型：{deviceEntity.devtype}");
                return false;
            }
        }
        #endregion

        #region 确认器
        /// <summary>
        /// 确认器，读取缓存状态，半秒读取一次，共读取3秒左右，没反应就返回失败
        /// </summary>
        /// <param name="cachekey"></param>
        /// <param name="ok"></param>
        /// <returns></returns>
        public static bool StateResult(string cachekey, string st)
        {
            using (RedisHashService service = new RedisHashService())
            {
                bool state = false;
                int i = 0;
                while (i < 30)//6s，次数限制redis
                {
                    i++;
                    Thread.Sleep(200);
                    //读取缓存状态
                    string status = service.GetValueFromHash("DeviceStatus", cachekey);
                    if (status == st)
                    {
                        log.Debug($"设备状态改变成功！ cachekey：{cachekey}！轮询次数： {i}");
                        state = true;
                        break;
                    }
                }
                return state;
            }
        }
        #endregion

        #endregion


        #region 室外状态改变
        /// <summary>
        /// 室外
        /// </summary>
        /// <param name="deviceEntity">设备实体</param>
        /// <param name="state">要改变成什么状态</param>
        /// <returns></returns>
        public static bool OutStateChangeByType(string appUser, hsf_outdevice deviceEntity, bool state)
        {
            switch (deviceEntity.devtype)
            {
                case "Dahua_UnitDoor"://大华单元门口机
                    return DaHuaDeviceService.UnitDoorSendMsg(deviceEntity.deviceid);
                case "Dahua_EntranceGuard"://门禁
                    return DaHuaDeviceService.EntranceGuardSendMsg(deviceEntity.deviceid);
                case "Elevator"://电梯
                    if (appUser.Split('-').Length >= 3)
                    {
                        string floor = appUser.Split('-')[3];
                        if (!string.IsNullOrEmpty(floor))
                        {
                            return ElevatorSendMsg(deviceEntity.deviceid, floor);
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }

        #region 梯控
        /// <summary>
        /// 梯控
        /// </summary>
        /// <param name="devmac">电梯mac</param>
        /// <returns>
        /// 
        /// </returns>
        public static bool ElevatorSendMsg(string devmac, string floor)
        {
            //string msg = TKcrc(devmac,floor);
            int ifloor = Convert.ToInt32(floor);
            string ifloor16 = ifloor.ToString("x2").ToUpper();
            //
            string dat = $"E3 CC {devmac} 01 01 {ifloor16}";
            //dat = dat.Replace("0x", "");
            string[] array = dat.Split(' ');
            int sum = 0;
            foreach (string arrayElement in array)
            {
                sum += int.Parse(arrayElement, System.Globalization.NumberStyles.HexNumber);
            }
            //命令到校验前的和，取反加1
            sum = (sbyte)~sum;
            sum += 1;
            string strB = sum.ToString("x2");
            string msg = dat + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " + strB + " 0D";

            RYZigClient.Send(msg);//发送到网关服务器YunZig

            if (StateResult(devmac, ifloor16))//对比16进制字符串
            {
                return true;
            }
            else
            {
                log.Debug($"电梯设备{devmac}状态改变超时失败！ 楼层：{floor}！");
                return false;
            }
        }
        #endregion

        #endregion
    }
}
