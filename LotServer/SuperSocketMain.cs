using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer
{
    public class SuperSocketMain
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("SuperSocketMain");//log2.0.3->2.0.8
        public static void Init()
        {
            try
            {
                Console.WriteLine("Welcome to LotServer!");
                IBootstrap bootstrap = BootstrapFactory.CreateBootstrap();
                if (!bootstrap.Initialize())
                {
                    Console.WriteLine("LotServer初始化失败");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("LotServer启动中...");
                var result = bootstrap.Start();
                foreach (var server in bootstrap.AppServers)
                {
                    if (server.State == ServerState.Running)
                    {
                        Console.WriteLine("- {0} 运行中", server.Name);
                    }
                    else
                    {
                        Console.WriteLine("- {0} 启动失败", server.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }
    }
}
