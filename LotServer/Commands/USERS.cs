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
    public class USERS : CommandBase<SmartSession, StringRequestInfo>
    {
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if ("USERS".Equals(requestInfo.Key))
            {
                var sesssionList = session.AppServer.GetAllSessions();
                if (sesssionList != null)
                {
                    string users = "";
                    foreach (var item in sesssionList)
                    {
                        users += item.user + ",";
                    }
                    session.Send($"{users}");
                }
            }
            else
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
