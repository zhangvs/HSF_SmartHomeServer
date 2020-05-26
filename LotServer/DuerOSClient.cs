using Hsf.Redis.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer
{
    public class DuerOSClient
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("DuerOSClient");
        public static void PutDeviceChangeQueue(string host)
        {
            //如果存在百度音响的设备列表缓存，则清空,再发布同步消息队列
            using (RedisHashService service1 = new RedisHashService())
            {
                string duerOSHost = service1.GetValueFromHash("DuerOS_DiscoverPayload", host);
                if (!string.IsNullOrEmpty(duerOSHost))
                {
                    service1.RemoveEntryFromHash("DuerOS_DiscoverPayload", host);
                    log.Debug($"删除掉DuerOS_DiscoverPayload：{host}");
                    using (RedisListService service = new RedisListService())
                    {
                        service.Publish("LotDeviceChangeQueue", host);
                        log.Debug($"放入设备同步队列LotDeviceChangeQueue：{host}");
                    }
                }
            }
        }
    }
}
