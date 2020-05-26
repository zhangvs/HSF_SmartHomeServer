using LotServer.Session;
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
    /// HeartBeat
    /// </summary>
    public class Heartbeat : CommandBase<SmartSession, StringRequestInfo>
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Heartbeat");//log2.0.3->2.0.8
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if ("Heartbeat".Equals(requestInfo.Key))
            {
                session.LastHBTime = DateTime.Now;
                session.Send("NetHeart");
                //log.Debug($"心跳session：  {session.RemoteEndPoint.ToString()}");
            }
            else
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
