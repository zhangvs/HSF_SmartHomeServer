using LotServer.connect;
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
    /// 户外设备
    /// </summary>
    public class OutDeviceOpen : CommandBase<SmartSession, StringRequestInfo>
    {
        //OutDeviceOpen MMSJ-1#1-5-501 1000000
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if (requestInfo.Parameters != null && requestInfo.Parameters.Length == 2 )
            {
                if (DeviceService.OutDeviceStateChange(requestInfo.Parameters[0], requestInfo.Parameters[1]))
                {
                    session.Send("openok");
                }
                else
                {
                    session.Send("error");
                }
            }
            else//能进入这个方法，说明已经是Check
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
