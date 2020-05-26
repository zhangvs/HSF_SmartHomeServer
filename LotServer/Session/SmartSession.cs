using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer.Session
{
    public class SmartSession : AppSession<SmartSession>
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SmartSession");
        /// <summary>
        /// 用户的唯一标识
        /// </summary>
        public string user { get; set; }
        public bool IsLogin { get; set; }
        public DateTime LoginTime { get; set; }

        public DateTime LastHBTime { get; set; }

        public bool IsOnLine
        {
            get
            {
                return this.LastHBTime.AddSeconds(10) > DateTime.Now;
            }
        }

        public override void Send(string message)
        {
            //Console.WriteLine($"准备发送给{this.user}：{message}");
            //log.Debug($"准备发送给 {this.user}： {message}");
            base.Send(message.Format());
        }

        protected override void OnSessionStarted()
        {
            //this.Send("Welcome to SuperSocket  Server");
            this.Send("Even " + DateTime.Now.ToString());
        }

        protected override void OnInit()
        {
            this.Charset = Encoding.GetEncoding("gb2312");//utf-8
            base.OnInit();
        }

        protected override void HandleUnknownRequest(StringRequestInfo requestInfo)
        {
            Console.WriteLine("收到命令: " + requestInfo.Key.ToString());
            log.Debug("收到命令: key: " + requestInfo.Key.ToString()+" key: " + requestInfo.Body.ToString());
            this.Send("不知道如何处理 " + requestInfo.Key.ToString() + " 命令");
        }


        /// <summary>
        /// 异常捕捉
        /// </summary>
        /// <param name="e"></param>
        protected override void HandleException(Exception e)
        {
            this.Send($"\n\r异常信息： { e.Message}");
            log.Debug($"\n\r异常信息： { e.Message}");
            //base.HandleException(e);
        }

        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <param name="reason"></param>
        protected override void OnSessionClosed(CloseReason reason)
        {
            Console.WriteLine($"链接已关闭。。。");
            base.OnSessionClosed(reason);
        }
    }
}
