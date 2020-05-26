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
    public class Sms : CommandBase<SmartSession, StringRequestInfo>
    {
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if (requestInfo.Parameters != null && requestInfo.Parameters[0].Length == 11)
            {
                if (requestInfo.Parameters.Length == 1)
                {
                    string sendResult= SmsControler.SendJoin(requestInfo.Parameters[0]);//发送验证码
                    session.Send(sendResult);
                }
                else if (requestInfo.Parameters.Length == 2)
                {
                    string validateResult = SmsControler.ValidateSmsCode(requestInfo.Parameters[0], requestInfo.Parameters[1]);//验证验证码
                    session.Send(validateResult);
                }
                else
                {
                    session.Send("Wrong Parameter");
                }
            }
            else//能进入这个方法，说明已经是Check
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
