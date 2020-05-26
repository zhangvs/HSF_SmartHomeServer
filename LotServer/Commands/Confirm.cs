using LotServer.DataCenter;
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
    class Confirm : CommandBase<SmartSession, StringRequestInfo>
    {
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            //ToId  Message  
            if (requestInfo.Parameters != null && requestInfo.Parameters.Length == 1)
            {
                string modelId = requestInfo.Parameters[0];//确认时  把消息ID发回去
                Console.WriteLine($"用户{session.user} 已确认 收到 消息{modelId}");
                SmartDataManager.Remove(session.user, modelId);//改状态或者删除
            }
            else
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
