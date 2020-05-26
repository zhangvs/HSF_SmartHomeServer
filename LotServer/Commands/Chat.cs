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
    public class Smart : CommandBase<SmartSession, StringRequestInfo>
    {
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            //ToId  Message  
            if (requestInfo.Parameters != null && requestInfo.Parameters.Length == 2)
            {
                string toId = requestInfo.Parameters[0];
                string message = requestInfo.Parameters[1];

                SmartSession toSession = session.AppServer.GetAllSessions().FirstOrDefault(s => toId.Equals(s.user));
                //用户在线
                string modelId = Guid.NewGuid().ToString();
                if (toSession != null)
                {
                    toSession.Send($"{session.user}给你发消息：{message} {modelId}");
                    //只能保证把数据发出去了，但是不保证目标一定收到
                    //需要客户端回发确认！
                    SmartDataManager.Add(toId, new SmartModel()
                    {
                        FromId = session.user,
                        ToId = toSession.user,
                        Message = message,
                        Id = modelId,
                        State = 1,//待确认
                        CreateTime = DateTime.Now
                    });
                }
                else
                {
                    SmartDataManager.Add(toId, new SmartModel()
                    {
                        FromId = session.user,
                        ToId = toId,
                        Message = message,
                        Id = modelId,
                        State = 0,//未发送
                        CreateTime = DateTime.Now
                    });
                    Console.WriteLine($"{toId}不在线，消息暂时没能送达！");
                }
            }
            else
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
