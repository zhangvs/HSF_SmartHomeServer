using Hsf.EF.Model;
using Hsf.Framework;
using Hsf.Redis.Service;
using LotServer.connect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                //DuerOSClient.PutDeviceChangeQueue("MMSJ-1-1-1-102");
                //using (RedisListService service = new RedisListService())
                //{
                //    service.Publish("LotDeviceChangeQueue", "MMSJ-1-1-1-102");
                //}
                //using (RedisListService service = new RedisListService())
                //{
                //    service.RPush("BaiDuSoundControl1", "02ea45da-bf64-4ab2-a913-6ff5335f7c7d$" + true);
                //    service.Publish("YunZigStateChangeQueue1", "010000124b000f81f8c5_8");
                //}

                //using (RedisStringService service = new RedisStringService())
                //{
                //    service.StringSet("RedisStringService_key1", "RedisStringService_value1");
                //}
                //RedisHashService service = new RedisHashService();
                //    service.HashSet("Hsf", "Room1", "asdfjdkjas");
                //    string msgResult0 = service.HashGet("Room", "123");
                //    string msgResult1 = service.HashGet("RoomDevices", "123|0115170616344");
                //string msgResult2 = service.HashGet("AccountDevices", "123");

                //未找到具有固定名称“MySql.Data.MySqlClient”的 ADO.NET 提供程序的实体框架提供程序
                //使用到数据库的都要加载ef，mysql.data/entity,6.9.12
                //mysql -connector-net-8.0.13可以向下兼容
                //using (HsfDBContext hsfDBContext = new HsfDBContext())
                //{
                //    var deviceList = hsfDBContext.host_device.Where(t => t.devposition == "123" && t.deletemark == 0).ToList();
                //}

                //SmartHomeHost.SendJoin("18660996839");
                //string dd=SmartHomeHost.ValidateSmsCode("18660996839", "767742");

                //using (DaHuaService.MobPhoneServiceClient client = new DaHuaService.MobPhoneServiceClient())
                //{
                //    //{"status":"1","resultMessage":"信息发送成功"}
                //    //{"status":"0","resultMessage":"设备不存在"}
                //    //var result2 = client.openDoor("{\"deviceCode\":\"1000001\"} ");
                //    //var result3 = client.getRoomNumByPhone("1000033");
                //    string sendmsg = "{\"deviceCode\":\"1000001\"}";
                //    string result1 = client.openDoor(sendmsg);
                //}
                //string a = "qazwsx123";
                //byte[] b = System.Text.Encoding.Default.GetBytes(a);
                ////转成 Base64 形式的 System.String  
                //a = Convert.ToBase64String(b);

                //string mac16 = RYZigClient.ToHex("192.168.82.107;f1;06|cc dd f1 06 05 00 08 00 01 8c 7f", "utf-8", false);

                //string ss = RYZigClient.UnHex(mac16, "utf-8");

                //string mac16 = RYZigClient.ToHex("cc dd F1 05 05 00 08 01 01 8d dc|58.57.32.162;F1;05", "utf-8", false);
                //string strDataStr = RYZigClient.UnHex("63 63 20 64 64 20 ", "utf-8");

                //using (RedisHashService service = new RedisHashService())
                //{
                //    string st = service.GetValueFromHash("DeviceStatus", "58.57.32.162;F1;01_0;1");
                //    service.RemoveEntryFromHash("DeviceStatus", "58.57.32.162;F1;01_0;1");
                //    service.SetEntryInHash("DeviceStatus", "58.57.32.162;F1;04_0;1", st);
                //}
                //DaHuaDeviceService.EntranceGuardSendMsg("");
                //DaHuaDeviceService.EntranceGuardSendMsg("1000001$7$0$0");
                //DaHuaDeviceService.EntranceGuardSendMsg("1000000");
                //string dd= PingYinHelper.GetFirstSpell("名门世家");//MMSJ


                YunZigClient.ReConnect();//9004zige
                //RYZigClient.ReConnect();//50000

                SuperSocketMain.Init();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }


    }
}
