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
    public class KICK : CommandBase<SmartSession, StringRequestInfo>
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("connect");//log2.0.3->2.0.8
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if ("KICK".Equals(requestInfo.Key))
            {
                var sesssionList = session.AppServer.GetAllSessions();
                if (sesssionList != null)
                {
                    SmartSession oldSession = sesssionList.FirstOrDefault(s => requestInfo.Parameters[1].Equals(s.user));
                    if (oldSession != null)
                    {
                        log.Debug($"{requestInfo.Parameters[1]} 被踢下线");
                        oldSession.Close();
                    }
                    session.Send("KICK OK");
                }
            }
            else
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
