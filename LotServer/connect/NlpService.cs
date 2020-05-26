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
    public class NlpService
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("NlpService");
        #region 513处理音响Nlp通讯
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
        ///25995_123;513;5omT5byA5byA5YWz;Zip;H4sIAAAAAAAEAHvWOfnpnob3e2Y9m7nrRfNeIPtp62YTk8cNTcgCpqZoAmZmaAIGBmgChoZoAkZGaALGxmgCieh8dIEkdD66QDI6H10gBY2fisZPQ+OnA/lPl2x8sWUpAHyIeM8pAQAA$/r$
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
                        bool state = false;
                        if (deviceStr.Contains("打开"))
                        {
                            actionStr = "打开";
                            code = "8135";
                            deviceStr = deviceStr.Replace("打开", "");
                            state = true;
                        }
                        else if (deviceStr.Contains("关闭"))
                        {
                            actionStr = "关闭";
                            code = "8145";
                            deviceStr = deviceStr.Replace("关闭", "");
                            state = false;
                        }


                        if (!string.IsNullOrEmpty(code))
                        {
                            using (RedisHashService service = new RedisHashService())
                            {
                                List<host_device> deviceList = null;
                                //获取当前房间的设备列表，先找缓存
                                string devices = service.GetValueFromHash("AccountDevices", account);

                                if (!string.IsNullOrEmpty(devices))
                                {
                                    deviceList = JsonConvert.DeserializeObject<List<host_device>>(devices);//list多件开关,ALL数组
                                }
                                else
                                {
                                    using (HsfDBContext hsfDBContext = new HsfDBContext())
                                    {
                                        deviceList = hsfDBContext.host_device.Where(t => t.account == account && t.deletemark == 0).ToList();
                                        service.SetEntryInHash("AccountDevices", account, JsonConvert.SerializeObject(deviceList));
                                    }
                                }
                                if (deviceList.Count() != 0)
                                {
                                    var deviceControl = deviceList.Where(t => t.chinaname.Contains(deviceStr)).ToList();//包含“开关”名称的所有设备
                                    if (deviceControl.Count() != 0)
                                    {
                                        string okDevices = "";
                                        string failDevices = "";
                                        foreach (var item in deviceControl)
                                        {
                                            //发送指令给网关，改变状态,避免两次查库
                                            //DeviceStateChange($"{session_account};8;{code};{item.deviceid};$/r$")
                                            if (ChangeStateMain.StateChangeByType(item, state))
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
                                        log.Debug($"{resultDevices}！ cachekey：{msgResult}");
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
