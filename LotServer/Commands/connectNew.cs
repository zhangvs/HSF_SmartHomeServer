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
    public class connectNew : CommandBase<SmartSession, StringRequestInfo>
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("connect");//log2.0.3->2.0.8

        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if (requestInfo.Parameters != null && requestInfo.Parameters.Length == 3)
            {
                log.Debug($"智能家居服务器收到新的消息：<<<<<< {requestInfo.Key} {requestInfo.Body} 请求session：  {session.RemoteEndPoint.ToString()}");
                string user = requestInfo.Parameters[0].Replace("user:", "");
                string type = requestInfo.Parameters[1].Replace("type:", "");
                string msg = requestInfo.Parameters[2].Replace("msg:", "");
                var sesssionList = session.AppServer.GetAllSessions();

                switch (type)
                {
                    case "isonline":
                        SmartSession us = sesssionList.FirstOrDefault(s => user.Equals(s.user));
                        ///校验当前服务器是否有同名的用户  
                        //登录用户和mac地址相同的：有同名的用户
                        if (us != null)
                        {
                            session.Send("online");//在线
                        }
                        else
                        {
                            session.Send("offline");//不存在
                        }
                        break;
                    case "home":
                        //校验当前服务器是否有同名的用户，，排除主机server情况，手机mac都是不同的
                        //connect user:test_Server type:home msg:001519009611
                        //connect user:123_C40BCB80050A type:home msg:C40BCB80050A
                        if (sesssionList != null)
                        {
                            SmartSession oldSession = sesssionList.FirstOrDefault(s => user.Equals(s.user));
                            if (oldSession != null)
                            {
                                oldSession.Send("login other，you kick off！");
                                oldSession.Close();
                            }
                        }

                        session.user = user;
                        session.IsLogin = true;
                        session.LoginTime = DateTime.Now;
                        session.Send("regist sucess");
                        break;
                    case "other":
                        string code = msg.Split(';')[2];
                        SmartSession sessionUser = sesssionList.FirstOrDefault(s => user.Equals(s.user));
                        if (sessionUser != null)
                        {
                            log.Debug($"user: {user}  othercode： {code}  响应session： {sessionUser.RemoteEndPoint.ToString()}");
                            switch (code)
                            {
                                case "111":
                                    //1.请求主机登录信息
                                    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;1;111;all;admin,admin,shouquanma,DAJCHSF_Server,2047DABEF936,192.168.1.101$/r$
                                    //2.主机返回登录结果  错误返回值error,  成功返回值：一串密钥字符串1@@@@88sdf888823xv8888
                                    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$
                                    sessionUser.Send($"{user};111;all;Zip;H4sIAAAAAAAAADN0AAILi+KUNAsgMDKuKAPRAHas4VgWAAAA$/r$");
                                    break;
                                case "835":
                                    //1.请求主机房间列表
                                    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;835;admin$/r$
                                    //2.主机返回房间列表
                                    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;835;admin;Zip;H4sIAAAAAAAAAIuuViotTi3KTFGyUlLSUSouSSxJBTJLikpTgdzkjMy8xLzEXJDQs46Jz2e1PF23DSiemZyfB9GQmZuYngrTXZBfDGYaQNgFiUWJSlbVSimpZSX5JYk5QBlDS5AliWmpxaklJZl56TCrasEaSioLUqHa40EGGego+aWWB6Um5xcBeSCFtTrY3Yvm1qfrFj3ta8Xh0KL8/FxDJNcaGhgbmpoYG1iam5iiOJzajupd/nTdEtIcBcRmBjR1VNe8p61rSHaXkampBW0Da13nyxmbSHOUsYmBkTl5jooFAHQFerEIAwAA$/r$
                                    msg = SmartHomeHost2.Host835(msg);
                                    sessionUser.Send(msg);
                                    break;
                                case "836":
                                    //新增房间 8;836;+Base64(zip(Position对象jhson串))，无返回
                                    SmartHomeHost2.Host836(msg);
                                    break;
                                case "837":
                                    //删除房间8;837;+posid
                                    SmartHomeHost2.Host837(msg);
                                    break;
                                case "815":
                                    //获得当前房间的设备列表的命令8;815;+ posid
                                    //1.请求主机房间列表
                                    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;815;103154315460$/r$
                                    //2.主机返回结果
                                    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;815;103154315460;Zip;H4sIAAAAAAAAAN2Vz0vDMBTH/5eehyRp0607Kbt70Jsi4y1Lu0CbljRzjDEQxL9BBc8iXr2IP/CvqfPPMGm7edphYwVZQ1O+3/deXpPPIeczZ5xzJYZO13FaTpZOLiEec6OKh6fF25fx2EhIkJBY8/v+7ef6s/i4Km5ekI3FkOfhsnrITTGoZKXYCKTksdEIhSFhdvjghwjRwKdVjmC8rMfIxW2XEup2KKpDmfVJ+wCZgQ8xruwEmF0RI/Ng4g3M7DEauDio4hkocLqzeSVSpe0qdSjNhRaprPtRz75+3S7XoO0mtTIH8Ofg1XZKSVZSTzOb3oNMxGbb0yOheqkclg2EjEySSCDiy9OJo7z8F9Ry+tYz32M+OeEsVUbZpvPW9jRwYzR8vHMaZN9pkMZotMnOabj7TsNtjoa7cxreBjRCiPN/h2Px+L4Ox+L5tni9a+TiCGjgBXjTiwOFHRwGjK7H0WkQx2kCSvfPRDTgvDdWGoTcgsTFL5qU1AfEBwAA$/r$
                                    msg = SmartHomeHost2.Host815(msg);
                                    sessionUser.Send(msg);
                                    break;
                                case "855":
                                    //获得当前设备类型的设备列表的命令8;815;+ posid
                                    //1.请求设备类型的设备列表
                                    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;855;0;Panel_Smartsix,Panel_Wired_Control,Panel_Zigbee,Panel_SingleZigbee,Panel_485Gao,Panel_485Hotel,Panel_485Bus$/r$
                                    //2.主机返回结果
                                    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;855;0;Zip;H4sIAAAAAAAAAOWZb2vTQBjAv4rkdZG7y92l2YcQ8aUi45pdukD/kaQOGQOH5oWFjb0Qq0wcOtGiMgcbUoqyT3Pr+i28a9LON2240kObNjTluculye8JvzyXPNq12hEPgy1rw7JKVqu584TV2lxG4l1vOLiSbd520GANVh83nn0Uh4k46gz3f6iuGosifzJ4i8uxLKxPI2+bNRq8JmMAfB95aqGM+gAQl5J0m8Dj4/EQ2JCiMoYUY5B1tVQ7cu4CucC0rc48tTsI5AciXJFrB/kU80ra32IhszZ299KgGcZqF1lXMwrioNnI/oxgG7gOzg4jilmsTtBntYj/1QSnJzMO0TSMn7bU9veZPMHNh0G1wtWwoM6qfMKjVo3GBwBK1qZqk7/3+M4D7jVDGcVhm++VFsJ//sIkfgKXix8VDL9kL/qfzeFHy8VvFwy/ON03dPW7kABXWz7cdytkHeRz/XZw8/y3+PVMJBfQGH9t++Tw17GPovLf4k+v/uvX56PTQ2P0qa58cugXSD4H34avTkbdS3HUH334aUZBuIwAwlBPQdgDNuFgHRSkbr3JS5H0pIWM4Ud6BsrFX5z65xZ/cmEOv56CcvEXR0E3V8ejbi+dfN1ZPn8HIgApcBewj78W9kn5myr/U/wQLGCfefiLY5/s8jc0+c3wL2KfefiLYx9x8EWcfTL16MexCSJ2mejKh7g2dBeQj/xSsHr4jcknxU915ZODf6Z8VhS/Mfmk+B1d+eTgnymfFcVvavI7wW8vFz8uGP7h167ov1k2fgxcRCB0gOaTN3XrdZk9G3+B3H98KQYdM+6f4td88JaLv3DuNzLtvcWvX3jOxV8g93fei+S7sdoTQ4gBJIRoTnzVixfqLVJ7IkLKq1X6TzNgpP6ZZIBqzn1zMzBTQaubAUM3gUkGoP7Lx7kZmGmhf5+Bx38AI07FAvsgAAA=$/r$
                                    msg = SmartHomeHost2.Host855(msg);
                                    sessionUser.Send(msg);
                                    break;
                                case "8211":
                                    //1.告诉主机添加设备
                                    //user:DAJCHSF_Server type:other msg:DAJCHSF_2047DABEF936;8;8211;ALL;H4sIAAAAAAAAAFVRTUsDMRD9LznvIdkvu71696A3Rco0nd0GdjdLkrVIKQji3Vv1LwjiwYuI/pz68S862SwVE0Ly5r3MvEku1uwEV6cotVmwqTM9Rmym6MgjJpeqhRYaZFP29fj+c/v5/bTdvT1wRlwN1pZeyCYEF3gFNZiGYEByCW2LNWHOyzKWfuaQl5xnRZ4FjZI4JOCCJ+IoKbIiLQQfuY6ITKQBNSCDjoaI0zlt5USUhRwzdWCATdfkSlvvdnf3+rt9jtIkiUTk/ekOWx//uNndvxzim3BZG/fXRqetckp7tbeVpX7loyuDldQLX0HESQhZBw4PbQ9I/IfxAbrrzkvPGjBudq6qOeJxbxyolnjVQBXeg5SC+87qyg7e6C86vaIX7sdKvUUzSNnmcg8kEEBHwgEAAA==$/r$
                                    //2.主机返回app添加成功
                                    //user:DAJCHSF_2047DABEF936 type:other msg:DAJCHSF_2047DABEF936;8211;ALL;Zip;H4sIAAAAAAAAAHNMScnPBgD0Si5gBQAAAA==$/r$

                                    //新增安防设备8;8211;All;+Base64(zip(设备对象jhson串))
                                    msg = SmartHomeHost2.AddDeviceToRoom(msg, "8211");
                                    sessionUser.Send(msg);
                                    break;
                                case "822":
                                    //1.告诉主机删除设备
                                    //user:123_Server type:other msg:123_DCD9165057AD;8;822;01120818544930$/r$
                                    //2.主机返回删除成功delok@105 124612 6590
                                    //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;822;01120818544930$;Zip;H4sIAAAAAAAAAEtJzcnPdjA0MDU0MjEzNDIztTQAALsdu8YTAAAA$/r$
                                    msg = SmartHomeHost2.Host822(msg);
                                    sessionUser.Send(msg);
                                    break;
                                case "8135":
                                    //1.收到打开指令
                                    //user:123_Server type:other msg:123_DCD9165057AD;8;8135;1041657481380;2;8$/r$
                                    //2.返回结果openok@1041657481380
                                    //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8135;1041657481380;Zip;H4sIAAAAAAAAAMsvSM3Lz3YwNDAxNDM1N7EwNLYwAAD0EGgCFAAAAA==$/r$
                                    string device3 = msg.Split(';')[3];
                                    if (SmartHomeHost2.DeviceStateChange(msg))//, "8135", true, "H4sIAAAAAAAEAEvOyS9Ozc8GALpv9UwHAAAA", out user
                                    {
                                        msg = $"{user};8135;{device3};Zip;H4sIAAAAAAAEAEvOyS9Ozc8GALpv9UwHAAAA$/r$";
                                    }
                                    else
                                    {
                                        msg = $"{user};8135;{device3};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";
                                    }
                                    sessionUser.Send(msg);
                                    break;
                                case "8145":
                                    //1.收到关闭指令
                                    //user:123_Server type:other msg:123_DCD9165057AD;8;8145;808181248576;3,0$/r$
                                    //2.返回结果closeok@808181248576
                                    //user:123_DCD9165057AD type:other msg:123_DCD9165057AD;8145;808181248576;Zip;H4sIAAAAAAAAAEvOyS9Ozc92sDCwMLQwNDKxMDU3AwCjJ+18FAAAAA==$/r$
                                    //3.群发
                                    //user:hiddenpath_% type:other msg:hiddenpath_Server;devrefresh;924150429051,false,hiddenpath_ASDFDSSE123$/r$
                                    string device4 = msg.Split(';')[3];
                                    if (SmartHomeHost2.DeviceStateChange(msg))//, "8145", false, "H4sIAAAAAAAEAMsvSM3LzwYAQNIGxwYAAAA=", out user
                                    {
                                        msg = $"{user};8145;{device4};Zip;H4sIAAAAAAAEAMsvSM3LzwYAQNIGxwYAAAA=$/r$";
                                    }
                                    else
                                    {
                                        msg = $"{user};8145;{device4};Zip;H4sIAAAAAAAEAEstKsovAgBxvN1dBQAAAA==$/r$";
                                    }
                                    sessionUser.Send(msg);
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
                                    msg = SmartHomeHost2.HostNlpRequest(msg);
                                    sessionUser.Send(msg);
                                    break;
                                case "8212":
                                    //注册音响:user:test_Server type:other msg:test_C40BCB80050A;8;8212;ALL;H4sIAAAAAAAAAG2QvU7DMBDH38VzBjvfytZ2YoCBFaHIci6ppcSObIcKVZVY2NkYeQJAqCuvE6G+BZcPhYLY/Lv76e7vu9mTK9hdg9CmIJkzHXgkl/ikHhFbqbjiDZCMnF6OX28fBIs1t7YcDJIiFnDHa24axInElisF9cJSwCgznyU+Y0kYR1FA5147NoKJGi4QuUiDMmApjcNyqrfccJLtiW5BodB/PvRP7x7O8pj3xxa1tkPa/vF4en79zzlMI7VxqDmwbt6hrXRSq5+ggZ/ELJq6BiqhCzgPax13sHxyJPYb/QXdfTuoq4v8ElRV6A7LsuHVdBgUGB0W1ZUdY+HlW73Ds3bzgs6CGdUhb74J6XqzTimN6Iocbr8BfQIiVMEBAAA=$/r$
                                    //8212的第二条带Zip的不是真实的注册指令
                                    msg = SmartHomeHost2.AddDeviceToRoom(msg, "8212");
                                    sessionUser.Send(msg);
                                    break;
                                case "8215"://connect user:123_C40BCB80050A type:other msg:123_C40BCB80050A;8;8215;abcabc;1$/r$
                                case "8216"://123_C40BCB80050A;8;8216;abcabc;1$/r$
                                case "301"://123_C40BCB80050A;3;301;slfjdsfj;H4sIAAAAAAAEAAESAO3/6Z2S5bKb5ZWk6YWS5omT5oqYdF3lPRIAAAA=;1$/r$
                                    //是否自身播放音乐状态反转：user:wali_Server type:other msg:wali_C40BCB80050A;8;8215;88888888;1$/r$
                                    //音箱自动升级的指令：user:wali_Server type:other msg:wali_C40BCB80050A;8;8216;88888888;1$/r$
                                    //音箱提醒：user:wali_Server type:other msg:wali_C40BCB80050A;3;301;slfjdsfj;64(中文);1$/r$
                                    SmartSession toSession = sesssionList.FirstOrDefault(s => "Nlp_Server".Equals(s.user));
                                    if (toSession != null)
                                    {
                                        toSession.Send(msg);
                                        //string modelId = Guid.NewGuid().ToString();
                                        //toSession.Send(msg+" "+modelId);
                                        //log.Debug($"{user}给NlpServer发消息：{msg} {modelId}");
                                        ////只能保证把数据发出去了，但是不保证目标一定收到
                                        ////需要客户端回发确认！
                                        //SmartDataManager.Add("Nlp_Server", new SmartModel()
                                        //{
                                        //    FromId = session.user,
                                        //    ToId = toSession.user,
                                        //    Message = msg,
                                        //    Id = modelId,
                                        //    State = 1,//待确认
                                        //    CreateTime = DateTime.Now
                                        //});
                                    }
                                    else
                                    {
                                        log.Debug($"Nlp_Server不在线，消息暂时没能送达！{msg}");
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            log.Debug($"{user}不在线，消息暂时没能送达！{msg}");
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
