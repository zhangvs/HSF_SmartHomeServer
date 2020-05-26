using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LotServer
{
    public class YunZigClient
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("YunZigClient");

        //客户端对象7
        static AsyncTcpSession tcpClient = null;
        static System.Timers.Timer xt_timer = null;
        static System.Timers.Timer cl_timer = null;

        public static void ConnectServer()
        {
            tcpClient = new AsyncTcpSession(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9004));//47.95.247.135,,192.168.1.104
            tcpClient.DataReceived += TcpClient_DataReceived;
            //tcpClient.Error += TcpClient_Error;
            tcpClient.Closed += TcpClient_Closed;
            tcpClient.Connect();
            log.Debug("YunZig网关客户端连接中……");
            Console.WriteLine("YunZig网关客户端连接中……");
        }

        public static void Heartbeat()
        {
            if (tcpClient != null)
            {
                if (tcpClient.IsConnected)
                {
                    RegSession();//注册长连接
                    if (xt_timer != null)
                    {
                        //先停止之前的心跳
                        xt_timer.Stop();
                        xt_timer.Enabled = false;
                        xt_timer.Dispose();
                    }

                    //每一分钟发一次心跳
                    xt_timer = new System.Timers.Timer(60000);
                    xt_timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>
                    {
                        SendMsg("{\"code\":010}");
                    });
                    xt_timer.Enabled = true;
                    xt_timer.Start();
                    log.Debug("YunZig网关客户端连接成功,心跳开始！");
                    Console.WriteLine("YunZig网关客户端连接成功,心跳开始！");
                }
                else
                {
                    log.Debug("YunZig网关客户端连接未启动,心跳失败！");
                    Console.WriteLine("YunZig网关客户端连接未启动,心跳失败！");
                    ReConnect();
                }
            }
            else
            {
                log.Debug("YunZig网关客户端连接未启动,心跳失败！");
                Console.WriteLine("YunZig网关客户端连接未启动,心跳失败！");
                ReConnect();
            }
        }

        //交给重连自然会判断连接是否存在，是否连接成功
        public static void ReConnect()
        {
            log.Debug("开启断线重连中……");
            if (cl_timer != null)
            {
                //先停止之前的重连
                cl_timer.Stop();
                cl_timer.Enabled = false;
                cl_timer.Dispose();
            }
            //每10秒重连一次
            cl_timer = new System.Timers.Timer(10000);
            //cl_timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>{});
            cl_timer.Elapsed += OnTimedEvent;
            cl_timer.Enabled = true;
            cl_timer.Start();
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            log.Debug("断线重连计时器中……");
            if (tcpClient != null)
            {
                if (tcpClient.IsConnected)
                {
                    cl_timer.Stop();
                    cl_timer.Enabled = false;
                    cl_timer.Dispose();
                    Heartbeat();
                    log.Debug("连接YunZig断线重连成功！！开始心跳！！");
                }
                else
                {
                    tcpClient = null;
                    ConnectServer();
                }
            }
            else
            {
                log.Debug("连接YunZig断线重连失败！！");
                ConnectServer();
            }
        }

        public static void RegSession()
        {
            if (tcpClient != null)
            {
                if (tcpClient.IsConnected)
                {
                    SendMsg("{\"lot\":9005}");
                }
            }
        }


        public static void SendMsg(string message)
        {
            if (tcpClient != null && tcpClient.IsConnected)
            {
                try
                {
                    #region 发送字符串
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    tcpClient.Send(data, 0, data.Length);
                    #endregion
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                log.Debug("YunZig网关客户端连接未启动,发送失败！" + message);
                Console.WriteLine("YunZig网关客户端连接未启动,发送失败！" + message);
            }
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_Closed(object sender, EventArgs e)
        {
            log.Debug("YunZig网关客户端断开连接！！");
            Console.WriteLine("YunZig网关客户端断开连接！！");
            ReConnect();
        }

        /// <summary>
        /// 连接发生异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            log.Debug("连接YunZig网关服务器异常！！");
            Console.WriteLine("连接YunZig网关服务器异常！！");
        }

        /// <summary>
        /// 接到的服务器消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_DataReceived(object sender, DataEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data, 0, e.Length);
            if (message != "alive")
            {
                log.Debug("收到YunZig网关服务器消息：" + message);
            }

        }
    }
}