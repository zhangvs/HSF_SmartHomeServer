using LotServer.connect;
using LotServer.DataCenter;
using LotServer.Session;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer.Commands
{
    /// <summary>
    /// 智能家居主机服务器连接
    /// </summary>
    public class US
    {
        public string user;
        public string mac;
        public SmartSession sesson;
    }

    public class connect : CommandBase<SmartSession, StringRequestInfo>
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("connect");//log2.0.3->2.0.8
        //智能家居主机服务器连接集合
        public static List<US> ls = new List<US>();
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if (requestInfo.Parameters != null && requestInfo.Parameters.Length == 3)
            {
                log.Debug($"新的消息：<<<<<< {requestInfo.Key} {requestInfo.Body}  请求session：  {session.RemoteEndPoint.ToString()}");
                string user = requestInfo.Parameters[0].Replace("user:", "");
                string type = requestInfo.Parameters[1].Replace("type:", "");
                string msg = requestInfo.Parameters[2].Replace("msg:", "");

                switch (type)
                {
                    case "isonline":
                        var us = ls.Where(t => t.user == user && t.mac == msg).FirstOrDefault();
                        ///校验当前服务器是否有同名的用户  
                        //登录用户和mac地址相同的：有同名的用户
                        if (us != null)
                        {
                            log.Debug(us.user + "@" + us.mac);
                            session.Send("online");//在线
                        }
                        else
                        {
                            session.Send("offline");//不存在
                        }
                        break;
                    case "home":
                        //校验当前服务器是否有同名的用户，，排除主机server情况，手机mac都是不同的
                        //user:test_Server type:home msg:001519009611
                        //user:test_C40BCB80050A type:home msg:C40BCB80050A
                        //connect user:MMSJ-1-1-1-102_D89B3B513CF4 type:home msg:D89B3B513CF4
                        var usList = ls.Where(t => t.user == user);
                        if (usList.Count() > 0)
                        {
                            foreach (var us2 in usList)
                            {
                                //手机相同的mac
                                if (us2.mac == msg)
                                {
                                    if (!us2.sesson.Equals(session))
                                    {
                                        us2.sesson.Close();
                                        us2.sesson = session;//替换成新的session
                                    }
                                    session.Send("regist sucess");
                                    log.Debug($"{us2.user}@ { user} {us2.mac}@ {msg}重复连接");
                                }
                                else
                                {
                                    //针对于主机，mac不同
                                    session.Send("repeat link");
                                    session.Close();
                                    log.Debug($"{us2.user}@ { user} {us2.mac}@ {msg}异地登陆被强制下线");
                                }
                            }
                        }
                        else
                        {
                            //缓存当前socket连接
                            ls.Add(new US() { sesson = session, user = user, mac = msg });
                            session.Send("regist sucess");
                            log.Info($"{user} regist sucess 登陆智能家居Socket服务器 在线人数{ ls.Count} {DateTime.Now.ToString()}");
                            Console.WriteLine($"{user} 登陆智能家居Socket服务器 在线人数{ ls.Count} {DateTime.Now.ToString()}");
                        }
                        break;
                    case "other":
                        string code = msg.Split(';')[2];
                        user = msg.Split(';')[0];
                        var sessionUser = ls.Where(t => t.user == user).FirstOrDefault();
                        //log.Debug($"user: {user}  othercode： {code}  响应session： {sessionUser.sesson.RemoteEndPoint.ToString()}");
                        switch (code)
                        {
                            case "111":
                                //1.请求主机登录信息
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;1;111;all;admin,admin,shouquanma,DAJCHSF_Server,2047DABEF936,192.168.1.101$/r$
                                //2.主机返回登录结果  错误返回值error,  成功返回值：一串密钥字符串1@@@@88sdf888823xv8888
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$

                                //connect user:MMSJ-1-1-1-102_Server type:other msg:MMSJ-1-1-1-102_D89B3B513CF4;1;111;all;admin,admin,shouquanma,DAJCHSF,D89B3B513CF4,192.168.1.111$/r$
                                sessionUser.sesson.Send($"{user};111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$");
                                break;
                            case "113":
                                //1.请求主机登录信息
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;1;111;all;admin,admin,shouquanma,DAJCHSF_Server,2047DABEF936,192.168.1.101$/r$
                                //2.主机返回登录结果  错误返回值error,  成功返回值：一串密钥字符串1@@@@88sdf888823xv8888
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$
                                sessionUser.sesson.Send($"1;113;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$");
                                break;

                            #region 房间
                            case "835":
                                //1.请求主机房间列表
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$
                                //2.主机返回房间列表
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;835;admin;Zip;H4sIAAAAAAAAAIuuViotTi3KTFGyUlLSUSouSSxJBTJLikpTgdzkjMy8xLzEXJDQs46Jz2e1PF23DSiemZyfB9GQmZuYngrTXZBfDGYaQNgFiUWJSlbVSimpZSX5JYk5QBlDS5AliWmpxaklJZl56TCrasEaSioLUqHa40EGGego+aWWB6Um5xcBeSCFtTrY3Yvm1qfrFj3ta8Xh0KL8/FxDJNcaGhgbmpoYG1iam5iiOJzajupd/nTdEtIcBcRmBjR1VNe8p61rSHaXkampBW0Da13nyxmbSHOUsYmBkTl5jooFAHQFerEIAwAA$/r$
                                msg = RoomService.Host835(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "836":
                                //新增房间 8;836;+Base64(zip(Position对象jhson串))，无返回
                                msg = RoomService.Host836(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "837":
                                //删除房间8;837;+posid
                                msg = RoomService.Host837(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "815":
                                //获得当前房间的设备列表的命令8;815;+ posid
                                //1.请求主机房间列表
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;815;103154315460$/r$
                                //2.主机返回结果
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;815;103154315460;Zip;H4sIAAAAAAAAAN2Vz0vDMBTH/5eehyRp0607Kbt70Jsi4y1Lu0CbljRzjDEQxL9BBc8iXr2IP/CvqfPPMGm7edphYwVZQ1O+3/deXpPPIeczZ5xzJYZO13FaTpZOLiEec6OKh6fF25fx2EhIkJBY8/v+7ef6s/i4Km5ekI3FkOfhsnrITTGoZKXYCKTksdEIhSFhdvjghwjRwKdVjmC8rMfIxW2XEup2KKpDmfVJ+wCZgQ8xruwEmF0RI/Ng4g3M7DEauDio4hkocLqzeSVSpe0qdSjNhRaprPtRz75+3S7XoO0mtTIH8Ofg1XZKSVZSTzOb3oNMxGbb0yOheqkclg2EjEySSCDiy9OJo7z8F9Ry+tYz32M+OeEsVUbZpvPW9jRwYzR8vHMaZN9pkMZotMnOabj7TsNtjoa7cxreBjRCiPN/h2Px+L4Ox+L5tni9a+TiCGjgBXjTiwOFHRwGjK7H0WkQx2kCSvfPRDTgvDdWGoTcgsTFL5qU1AfEBwAA$/r$
                                msg = DeviceService.Host815(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            #endregion


                            #region 设备
                            case "855":
                                //获得当前设备类型的设备列表的命令8;815;+ posid
                                //1.请求设备类型的设备列表
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;855;0;Panel_Smartsix,Panel_Wired_Control,Panel_Zigbee,Panel_SingleZigbee,Panel_485Gao,Panel_485Hotel,Panel_485Bus$/r$
                                //2.主机返回结果
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;855;0;Zip;H4sIAAAAAAAAAOWZb2vTQBjAv4rkdZG7y92l2YcQ8aUi45pdukD/kaQOGQOH5oWFjb0Qq0wcOtGiMgcbUoqyT3Pr+i28a9LON2240kObNjTluculye8JvzyXPNq12hEPgy1rw7JKVqu584TV2lxG4l1vOLiSbd520GANVh83nn0Uh4k46gz3f6iuGosifzJ4i8uxLKxPI2+bNRq8JmMAfB95aqGM+gAQl5J0m8Dj4/EQ2JCiMoYUY5B1tVQ7cu4CucC0rc48tTsI5AciXJFrB/kU80ra32IhszZ299KgGcZqF1lXMwrioNnI/oxgG7gOzg4jilmsTtBntYj/1QSnJzMO0TSMn7bU9veZPMHNh0G1wtWwoM6qfMKjVo3GBwBK1qZqk7/3+M4D7jVDGcVhm++VFsJ//sIkfgKXix8VDL9kL/qfzeFHy8VvFwy/ON03dPW7kABXWz7cdytkHeRz/XZw8/y3+PVMJBfQGH9t++Tw17GPovLf4k+v/uvX56PTQ2P0qa58cugXSD4H34avTkbdS3HUH334aUZBuIwAwlBPQdgDNuFgHRSkbr3JS5H0pIWM4Ud6BsrFX5z65xZ/cmEOv56CcvEXR0E3V8ejbi+dfN1ZPn8HIgApcBewj78W9kn5myr/U/wQLGCfefiLY5/s8jc0+c3wL2KfefiLYx9x8EWcfTL16MexCSJ2mejKh7g2dBeQj/xSsHr4jcknxU915ZODf6Z8VhS/Mfmk+B1d+eTgnymfFcVvavI7wW8vFz8uGP7h167ov1k2fgxcRCB0gOaTN3XrdZk9G3+B3H98KQYdM+6f4td88JaLv3DuNzLtvcWvX3jOxV8g93fei+S7sdoTQ4gBJIRoTnzVixfqLVJ7IkLKq1X6TzNgpP6ZZIBqzn1zMzBTQaubAUM3gUkGoP7Lx7kZmGmhf5+Bx38AI07FAvsgAAA=$/r$
                                msg = DeviceService.Host855(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "8211":
                                //1.告诉主机添加设备
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;8211;ALL;H4sIAAAAAAAAAFVRTUsDMRD9LznvIdkvu71696A3Rco0nd0GdjdLkrVIKQji3Vv1LwjiwYuI/pz68S862SwVE0Ly5r3MvEku1uwEV6cotVmwqTM9Rmym6MgjJpeqhRYaZFP29fj+c/v5/bTdvT1wRlwN1pZeyCYEF3gFNZiGYEByCW2LNWHOyzKWfuaQl5xnRZ4FjZI4JOCCJ+IoKbIiLQQfuY6ITKQBNSCDjoaI0zlt5USUhRwzdWCATdfkSlvvdnf3+rt9jtIkiUTk/ekOWx//uNndvxzim3BZG/fXRqetckp7tbeVpX7loyuDldQLX0HESQhZBw4PbQ9I/IfxAbrrzkvPGjBudq6qOeJxbxyolnjVQBXeg5SC+87qyg7e6C86vaIX7sdKvUUzSNnmcg8kEEBHwgEAAA==$/r$
                                //2.主机返回app添加成功
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;8211;ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$
                                //新增安防设备8;8211;All;+Base64(zip(设备对象jhson串))
                                msg = DeviceService.AddDeviceToRoom(msg, "8211");
                                sessionUser.sesson.Send(msg);
                                break;
                            case "822":
                                //1.告诉主机删除设备
                                //user:123_Server type:other msg:123_DCD9165057AD;8;822;01120818544930$/r$
                                //2.主机返回删除成功delok@105 124612 6590
                                //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;822;01120818544930$;Zip;H4sIAAAAAAAAAEtJzcnPdjA0MDU0MjEzNDIztTQAALsdu8YTAAAA$/r$
                                msg = DeviceService.Host822(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "823":
                                //1.告诉主机重命名设备名称
                                //connect user:123_Server type:other msg:123_DCD9165057AD;8;823;01240943509560;5bCE54GvLDAxMTUxNzA2MTYzNDQ=$/r$
                                //2.主机返回改名成功renameok@1041140155612
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;823;1041140155612;Zip;H4sIAAAAAAAAACtKzUvMTc3PdjA0MDE0NDEwNDU1MzQCAFBUoP4WAAAA$/r$
                                msg = DeviceService.Host823(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "8135":
                                //1.收到打开指令
                                //user:123_Server type:other msg:123_DCD9165057AD;8;8135;1041657481380;2;8$/r$
                                //2.返回结果openok@1041657481380
                                //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8135;1041657481380;Zip;H4sIAAAAAAAAAMsvSM3Lz3YwNDAxNDM1N7EwNLYwAAD0EGgCFAAAAA==$/r$
                                string device3 = msg.Split(';')[3].Replace("$/r$", "");
                                if (DeviceService.DeviceStateChange(msg))//, "8135", true, "H4sIAAAAAAAEAEvOyS9Ozc8GALpv9UwHAAAA", out user
                                {
                                    msg = $"{user};8135;{device3};Zip;H4sIAAAAAAAEAMsvSM3LzwYAQNIGxwYAAAA=$/r$";//openok
                                }
                                else
                                {
                                    msg = $"{user};8135;{device3};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";
                                }
                                sessionUser.sesson.Send(msg);
                                break;
                            case "8145":
                                //1.收到关闭指令
                                //user:123_Server type:other msg:123_DCD9165057AD;8;8145;808181248576;3,0$/r$
                                //2.返回结果closeok@808181248576
                                //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8145;808181248576;Zip;H4sIAAAAAAAAAEvOyS9Ozc92sDCwMLQwNDKxMDU3AwCjJ+18FAAAAA==$/r$
                                //3.群发
                                //user:hiddenpath_% type:other msg:hiddenpath_Server;devrefresh;924150429051,false,hiddenpath_ASDFDSSE123$/r$
                                string device4 = msg.Split(';')[3].Replace("$/r$", "");
                                if (DeviceService.DeviceStateChange(msg))//, "8145", false, "H4sIAAAAAAAEAMsvSM3LzwYAQNIGxwYAAAA=", out user
                                {
                                    msg = $"{user};8145;{device4};Zip;H4sIAAAAAAAEAEvOyS9Ozc8GALpv9UwHAAAA$/r$";//closeok
                                }
                                else
                                {
                                    msg = $"{user};8145;{device4};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";
                                }
                                sessionUser.sesson.Send(msg);
                                break;
                            case "8133":
                                //1.收到打开指令
                                //connect user:MMSJ-1-1-30-3001_C40BCB80050A type:other msg:MMSJ-1-1-30-3001_C40BCB80050A;8;8133;08$/r$
                                //connect user:MMSJ-1-1-30-3001_C40BCB80050A type:other msg:MMSJ-1-1-30-3001_C40BCB80050A;8;8133;1000001$7$0$0$/r$
                                //connect user:MMSJ-1-1-30-3001_C40BCB80050A type:other msg:MMSJ-1-1-30-3001_C40BCB80050A;8;8133;1000000$/r$
                                string appUser = msg.Split(';')[0];
                                string deviceid = msg.Split(';')[3].Replace("$/r$", "");
                                if (DeviceService.OutDeviceStateChange(appUser, deviceid))//, "8135", true, "H4sIAAAAAAAEAEvOyS9Ozc8GALpv9UwHAAAA", out user
                                {
                                    msg = $"{user};8133;{deviceid};Zip;H4sIAAAAAAAEAMsvSM3LzwYAQNIGxwYAAAA=$/r$";//openok
                                }
                                else
                                {
                                    msg = $"{user};8133;{deviceid};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";
                                }
                                sessionUser.sesson.Send(msg);
                                break;
                            #endregion



                            case "876":
                                //获得总线网关要添加的设备信息，8;876;Safe_Center;192.168.82.18
                                //1.请求设备类型的设备列表
                                //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;876;Safe_Center;192.168.82.18$/r$
                                //2.主机返回结果
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;855;0;Zip;H4sIAAAAAAAAAOWZb2vTQBjAv4rkdZG7y92l2YcQ8aUi45pdukD/kaQOGQOH5oWFjb0Qq0wcOtGiMgcbUoqyT3Pr+i28a9LON2240kObNjTluculye8JvzyXPNq12hEPgy1rw7JKVqu584TV2lxG4l1vOLiSbd520GANVh83nn0Uh4k46gz3f6iuGosifzJ4i8uxLKxPI2+bNRq8JmMAfB95aqGM+gAQl5J0m8Dj4/EQ2JCiMoYUY5B1tVQ7cu4CucC0rc48tTsI5AciXJFrB/kU80ra32IhszZ299KgGcZqF1lXMwrioNnI/oxgG7gOzg4jilmsTtBntYj/1QSnJzMO0TSMn7bU9veZPMHNh0G1wtWwoM6qfMKjVo3GBwBK1qZqk7/3+M4D7jVDGcVhm++VFsJ//sIkfgKXix8VDL9kL/qfzeFHy8VvFwy/ON03dPW7kABXWz7cdytkHeRz/XZw8/y3+PVMJBfQGH9t++Tw17GPovLf4k+v/uvX56PTQ2P0qa58cugXSD4H34avTkbdS3HUH334aUZBuIwAwlBPQdgDNuFgHRSkbr3JS5H0pIWM4Ud6BsrFX5z65xZ/cmEOv56CcvEXR0E3V8ejbi+dfN1ZPn8HIgApcBewj78W9kn5myr/U/wQLGCfefiLY5/s8jc0+c3wL2KfefiLYx9x8EWcfTL16MexCSJ2mejKh7g2dBeQj/xSsHr4jcknxU915ZODf6Z8VhS/Mfmk+B1d+eTgnymfFcVvavI7wW8vFz8uGP7h167ov1k2fgxcRCB0gOaTN3XrdZk9G3+B3H98KQYdM+6f4td88JaLv3DuNzLtvcWvX3jOxV8g93fei+S7sdoTQ4gBJIRoTnzVixfqLVJ7IkLKq1X6TzNgpP6ZZIBqzn1zMzBTQaubAUM3gUkGoP7Lx7kZmGmhf5+Bx38AI07FAvsgAAA=$/r$
                                msg = BusSwitchTestTool.Host876(msg);
                                sessionUser.sesson.Send(msg);
                                break;
                            case "8231":
                                //1.告诉主机重命名设备名称
                                //connect user:123_Server type:other msg:123_DCD9165057AD;8;823;01240943509560;5bCE54GvLDAxMTUxNzA2MTYzNDQ=$/r$
                                //2.主机返回改名成功renameok@1041140155612
                                //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;823;1041140155612;Zip;H4sIAAAAAAAAACtKzUvMTc3PdjA0MDE0NDEwNDU1MzQCAFBUoP4WAAAA$/r$
                                msg = BusSwitchTestTool.Host8231(msg);
                                sessionUser.sesson.Send(msg);
                                break;


                            case "912":
                                //1.收到打开场景指令
                                //user:JYWH_Server type:other msg:JYWH_2047DABEF936;9;912;all;2;662;20170624163253852$/r$
                                //2.返回结果openok
                                //user:JYWH_2047DABEF936 type:other msg:JYWH_2047DABEF936;912;all;Zip;H4sIAAAAAAAAAMsvSM3LzwYAQNIGxwYAAAA=$/r$
                                break;

                            case "513":
                                //1.二楼全开
                                //connect user:123_Server type:other msg:25995_ac83f317b8c7;5;513;5omT5byA5byA5YWz$/r$   //5LqM5qW85YWo5byA
                                //2.返回结果  开启所有灯光、音乐、窗帘4
                                //connect user:25995_ac83f317b8c7 type:other msg:25995_ac83f317b8c7;513;5LqM5qW85YWo5byA;Zip;H4sIAAAAAAAAAHu6p+HphPXPOhuezel83rj+aWvn44bGl/M3P9k5Ach4vmr60x0zTAAnYIhxJQAAAA==$/r$
                                var nlpUser513 = ls.Where(t => t.user == "Nlp_Server").FirstOrDefault();
                                msg = NlpService.HostNlpRequest(msg);
                                nlpUser513.sesson.Send(msg);
                                break;

                            case "8212":
                                //注册音响:user:test_Server type:other msg:test_C40BCB80050A;8;8212;ALL;H4sIAAAAAAAAAG2QvU7DMBDH38VzBjvfytZ2YoCBFaHIci6ppcSObIcKVZVY2NkYeQJAqCuvE6G+BZcPhYLY/Lv76e7vu9mTK9hdg9CmIJkzHXgkl/ikHhFbqbjiDZCMnF6OX28fBIs1t7YcDJIiFnDHa24axInElisF9cJSwCgznyU+Y0kYR1FA5147NoKJGi4QuUiDMmApjcNyqrfccJLtiW5BodB/PvRP7x7O8pj3xxa1tkPa/vF4en79zzlMI7VxqDmwbt6hrXRSq5+ggZ/ELJq6BiqhCzgPax13sHxyJPYb/QXdfTuoq4v8ElRV6A7LsuHVdBgUGB0W1ZUdY+HlW73Ds3bzgs6CGdUhb74J6XqzTimN6Iocbr8BfQIiVMEBAAA=$/r$
                                //8212的第二条带Zip的不是真实的注册指令
                                msg = DeviceService.AddDeviceToRoom(msg, "8212");
                                sessionUser.sesson.Send(msg);
                                break;
                            case "8215":
                            case "8216":
                            case "301":
                                //是否自身播放音乐状态反转：user:wali_Server type:other msg:wali_C40BCB80050A;8;8215;88888888;1$/r$
                                //音箱自动升级的指令：user:wali_Server type:other msg:wali_C40BCB80050A;8;8216;88888888;1$/r$
                                //音箱提醒：user:wali_Server type:other msg:wali_C40BCB80050A;3;301;slfjdsfj;64(中文);1$/r$
                                var nlpUser = ls.Where(t => t.user == "Nlp_Server").FirstOrDefault();
                                nlpUser.sesson.Send(msg);
                                break;
                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }
            else//能进入这个方法，说明已经是Check
            {
                session.Send("Wrong Parameter");
            }
        }
    }

}
