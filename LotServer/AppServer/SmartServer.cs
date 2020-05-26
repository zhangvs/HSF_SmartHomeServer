using LotServer.Session;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer.AppServer
{
    public class SmartServer : AppServer<SmartSession>
    {
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            Console.WriteLine("准备读取配置文件。。。。");
            Logger.Debug("准备读取配置文件。。。。");
            return base.Setup(rootConfig, config);
        }

        protected override void OnStarted()
        {
            Console.WriteLine("服务启动。。。");
            Logger.Debug("服务启动。。。");
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            Console.WriteLine("服务停止。。。");
            Logger.Debug("服务停止。。。");
            base.OnStopped();
        }

        /// <summary>
        /// 新的连接
        /// </summary>
        /// <param name="session"></param>
        protected override void OnNewSessionConnected(SmartSession session)
        {
            Console.WriteLine($"新加入的连接: {session.RemoteEndPoint.ToString()} { DateTime.Now.ToString()}");
            Logger.Debug($"新加入的连接: {session.RemoteEndPoint.ToString()} { DateTime.Now.ToString()}");
            base.OnNewSessionConnected(session);
        }


    }
}
