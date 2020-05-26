using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LotServer
{
    public class RYZigClient
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("RYZigClient");

        //客户端对象7
        static AsyncTcpSession tcpClient = null;
        static System.Timers.Timer xt_timer = null;
        static System.Timers.Timer cl_timer = null;

        public static void ConnectServer()
        {
            tcpClient = new AsyncTcpSession(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50000));//47.95.247.135,,192.168.1.104//192.168.82.107
            tcpClient.DataReceived += TcpClient_DataReceived;
            //tcpClient.Error += TcpClient_Error;
            tcpClient.Closed += TcpClient_Closed;
            tcpClient.Connect();
            log.Debug("RYZig网关客户端连接中……");
            Console.WriteLine("RYZig网关客户端连接中……");
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
                        SendMsg("010");
                    });
                    xt_timer.Enabled = true;
                    xt_timer.Start();
                    log.Debug("RYZig网关客户端连接成功,心跳开始！");
                    Console.WriteLine("RYZig网关客户端连接成功,心跳开始！");
                }
                else
                {
                    log.Debug("RYZig网关客户端连接未启动,心跳失败！");
                    Console.WriteLine("RYZig网关客户端连接未启动,心跳失败！");
                    ReConnect();
                }
            }
            else
            {
                log.Debug("RYZig网关客户端连接未启动,心跳失败！");
                Console.WriteLine("RYZig网关客户端连接未启动,心跳失败！");
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
                    log.Debug("连接RYZig断线重连成功！！开始心跳！！");
                }
                else
                {
                    tcpClient = null;
                    ConnectServer();
                }
            }
            else
            {
                log.Debug("连接RYZig断线重连失败！！");
                ConnectServer();
            }
        }

        public static void RegSession()
        {
            if (tcpClient != null)
            {
                if (tcpClient.IsConnected)
                {
                    SendMsg("9005");
                }
            }
        }


        /// <summary>
        /// 服务器向客户端发送消息
        /// </summary>
        /// <param name="str"></param>
        public static void Send(string str)
        {
            //byte[] buffer = Encoding.UTF8.GetBytes(str);
            //socketSend.Send(buffer);

            //发送数据（只支持十六进制数据）
            if (str.Trim().Length < 1) return;
            string strData = str.Trim();
            if (null != tcpClient)
            {
                if (SendData(strData))
                {
                    Console.WriteLine("发送成功：" + strData);
                }
                else
                {
                    Console.WriteLine("发送失败：" + strData);
                }
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="strData">十六进制字符串</param>
        /// <returns>是否发送成功</returns>
        public static bool SendData(string strData)
        {
            if (string.IsNullOrEmpty(strData))
            {
                return false;
            }
            byte[] bytes = strToToHexByte(strData);

            try
            {
                tcpClient.Send(bytes, 0, bytes.Length);
                return true;
            }
            catch
            {
                //socketSend.Shutdown(SocketShutdown.Both);
                //socketSend.Close();
                //if (Connect())
                //{
                //    return SendData(strData);
                //}
            }
            return false;
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
                log.Debug("RYZig网关客户端连接未启动,发送失败！" + message);
                Console.WriteLine("RYZig网关客户端连接未启动,发送失败！" + message);
            }
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_Closed(object sender, EventArgs e)
        {
            log.Debug("RYZig网关客户端断开连接！！");
            Console.WriteLine("RYZig网关客户端断开连接！！");
            ReConnect();
        }

        /// <summary>
        /// 连接发生异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TcpClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            log.Debug("连接RYZig网关服务器异常！！");
            Console.WriteLine("连接RYZig网关服务器异常！！");
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
                log.Debug("收到RYZig网关服务器消息：" + message);
            }

        }






        /// <summary>
        /// 十六进制字符串转十六进制字节数组
        /// </summary>
        /// <param name="hexString">十六进制字符串</param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }
        //将字符串转为16进制字符，允许中文
        public static string StringToHexString(string s, Encoding encode)
        {
            byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符
            {
                result += Convert.ToString(b[i], 16) + " ";
            }
            return result;
        }


        //将16进制字符串转为字符串
        public static string HexStringToString(string hs, Encoding encode)
        {
            string strTemp = "";
            byte[] b = new byte[hs.Length / 2];
            for (int i = 0; i < hs.Length / 2; i++)
            {
                strTemp = hs.Substring(i * 2, 2);
                b[i] = Convert.ToByte(strTemp, 16);
            }
            //按照指定编码将字节数组变为字符串
            return encode.GetString(b);
        }









        /// <summary>
        /// 从汉字转换到16进制
        /// </summary>
        /// <param name="s"></param>
        /// <param name="charset">编码,如"utf-8","gb2312"</param>
        /// <param name="fenge">是否每字符用逗号分隔</param>
        /// <returns></returns>
        public static string ToHex(string s, string charset, bool fenge)
        {
            if ((s.Length % 2) != 0)
            {
                s += " ";//空格
                         //throw new ArgumentException("s is not valid chinese string!");
            }
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding(charset);
            byte[] bytes = chs.GetBytes(s);
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format("{0:X}", bytes[i]);
                if (fenge && (i != bytes.Length - 1))
                {
                    str += string.Format("{0}", ",");
                }
            }
            return str.ToLower();
        }

        ///<summary>
        /// 从16进制转换成汉字
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="charset">编码,如"utf-8","gb2312"</param>
        /// <returns></returns>
        public static string UnHex(string hex, string charset)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            hex = hex.Replace(",", "");
            hex = hex.Replace("\n", "");
            hex = hex.Replace("\\", "");
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                hex += "20";//空格
            }
            // 需要将 hex 转换成 byte 数组。 
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    // 每两个字符是一个 byte。 
                    bytes[i] = byte.Parse(hex.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    // Rethrow an exception with custom message. 
                    throw new ArgumentException("hex is not a valid hex number!", "hex");
                }
            }
            System.Text.Encoding chs = System.Text.Encoding.GetEncoding(charset);
            return chs.GetString(bytes);
        }
    }
}