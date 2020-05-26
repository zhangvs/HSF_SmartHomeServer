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
    public class Check : CommandBase<SmartSession, StringRequestInfo>
    {
        public override void ExecuteCommand(SmartSession session, StringRequestInfo requestInfo)
        {
            if (requestInfo.Parameters != null && requestInfo.Parameters.Length == 2)
            {
                var sesssionList = session.AppServer.GetAllSessions();
                if (sesssionList != null)
                {
                    SmartSession oldSession = sesssionList.FirstOrDefault(s => requestInfo.Parameters[0].Equals(s.user));
                    if (oldSession != null)
                    {
                        oldSession.Send("login other computer，you kick off！");
                        oldSession.Close();
                    }
                }

                //不去数据库查询了
                session.user = requestInfo.Parameters[0];
                session.IsLogin = true;
                session.LoginTime = DateTime.Now;

                session.Send("Login Success");

                { //登陆获取离线消息--好友列表
                    SmartDataManager.SendLogin(session.user, c =>
                    {
                        session.Send($"{c.FromId}给你发消息：{c.Message} {c.Id}");
                    });
                }
            }
            else//能进入这个方法，说明已经是Check
            {
                session.Send("Wrong Parameter");
            }
        }
    }
}
