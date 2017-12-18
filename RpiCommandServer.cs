using System;
using System.Collections.Generic;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using TCPSERVER.Filter;

/// <summary>
/// ///  CommandServer →  Rpi通信格式
/// --------------------------------------------------------------
/// | 0 |  1   |  2    |  3    |   4  |  5  |  6  |   7   |  8   | 
/// --------------------------------------------------------------
/// |   |      |       |       |      |     |     |       |      |    
/// --------------------------------------------------------------
/// 0,    前    
/// 1，   后  
/// 2，   左
/// 3，   有
/// 4,    抬头    
/// 5，   低头  
/// 6，   视频
/// </summary>
///

/// <summary>
/// ///  Server → CommandRpi 通信格式
/// ---------
/// | 0-3   | 
/// ---------
/// | #     | 
/// ---------
/// 0000    服务器 DOWN  
/// 0001    服务器 UP
/// 1101    APP DOWN
/// 1110    APP UP
/// 1010    Session 类中 异常
/// 1011    请求格式不符合过滤器的格式
/// 1100    Session 关闭
/// 
/// 0110    Rpi 发送测距 成功
/// 0111    Server 请求 重发测距 
/// 0010    Rpi 连接 服务器 成功 
/// 0011    Rpi 连接 服务器 失败
/// 0100    Rpi 请求 成功
/// 0101    Rpi 请求 失败


/// </summary>
/// 


namespace TCPSERVER
{
    class RpiCommandServer : AppServer<RpiCommandSession, RpiCommandInfo>, IDespatchServer
    {
        static string TAG = "RpiCommandServer:";

        //定义接口，供向上转型，通过这个对象将信息，从 RpiServer 发送到 App
        private IDespatchServer m_DespatchServer;

        //定义保存session的栈
        Stack<RpiCommandSession> commandSessionStack;

        public void DispatchAppDownMessageToRpi(string macKey, string message)
        {
            if (commandSessionStack.Count > 0)
            {
                RpiCommandSession session = commandSessionStack.Pop();
                ServerDispatchErrMessageToRpi(session, "9");
                commandSessionStack.Push(session);
                Console.WriteLine("RpiCommand在线已经发送AppDown消息");
            }
            else
                Console.WriteLine("RpiCommand不在线已经发送AppDown消息");
        }
        //自定义函数：Server → Rpi 发送错误消息.根据错误的类型发送不同的消息

        public static void ServerDispatchErrMessageToRpi(RpiCommandSession session, string mode)
        {
            //RpiSession session = rpiSessionStack.Pop();
            switch (mode)
            {
                case "1":
                    session.Send("0000");
                    Console.WriteLine(TAG + "RpiCommandServer → Rpi : 服务器DOWN");
                    break;
                case "2":
                    session.Send("0001");
                    Console.WriteLine(TAG + "RpiCommandServer → Rpi : 服务器UP");
                    break;
                case "3":
                    session.Send("1000");
                    Console.WriteLine(TAG + "RpiCommandServer → Rpi : Rpi 发送测距 成功");
                    break;
                case "4":
                    session.Send("1001");
                    Console.WriteLine(TAG + "RpiCommandServer → Rpi : Rpi 发送测距 失败");
                    break;
                case "5":
                    session.Send("1010");
                    Console.WriteLine("RpiCommandServer → Rpi : Session 类 异常");
                    break;
                case "6":
                    session.Send("1011");
                    Console.WriteLine("RpiCommandServer → Rpi : Rpi 发送信息格式不符合过滤器格式");
                    break;
                case "7":
                    session.Send("1100");
                    Console.WriteLine("RpiCommandServer → Rpi : Session 关闭");
                    break;
                case "8":
                    session.Send("1101");
                    Console.WriteLine("RpiCommandServer → Rpi : AppDown");
                    break;
                case "9":
                    session.Send("0000000");
                    Console.WriteLine("RpiCommandServer → Rpi : AppDown 停止控制信息");
                    break;


            }
            //rpiSessionStack.Push(session);
        }

        //向树莓派发送信息
        public void DispatchNormalMessage(string macKey, string message)
        {
            lock(this)
            {
                if(commandSessionStack.Count>0)
                {
                    RpiCommandSession session = commandSessionStack.Pop();
                    //DateTime endTime = DateTime.Now;
                    //Console.WriteLine(endTime.Second + ":" + endTime.Millisecond +TAG +"RpiCommandServer → Rpi : " + message);
                    session.Send(message);
                    //Console.WriteLine(TAG+"RpiCommandServer → Rpi : " + message);
                    commandSessionStack.Push(session);
                }
            }
        }

        //自定义函数。RpiServer → App (唯一标识符、消息）
        internal void DespatchMessage(string mode, string targetMacKey, string message)
        {
            //向App发送测距信息
            m_DespatchServer.ServerDispatchDisMessageToApp(message);
        }

        //仅仅需要在App端实现
        public void ServerDispatchDisMessageToApp(string message)
        {
        }

        public RpiCommandServer()
        : base(new DefaultReceiveFilterFactory<RpiCommandFilter, RpiCommandInfo>()) //使用默认的接受过滤器工厂 (DefaultReceiveFilterFactory)
        {
            NewSessionConnected += RpiCommandServer_NewSessionConnected;
            NewRequestReceived += RpiCommandServer_NewRequestReceived;
        }

        public void closeAllSession()
        {
            while (commandSessionStack.Count > 0)
            {
                commandSessionStack.Pop().Close();
            }
        }

        private void RpiCommandServer_NewSessionConnected(RpiCommandSession session)
        {
            closeAllSession();
            commandSessionStack.Push(session);
            commandSessionStack.Push(session);
            Console.WriteLine(TAG+ "RpiCommandServer_NewSessionConnected");
        }

        private void RpiCommandServer_NewRequestReceived(RpiCommandSession session, RpiCommandInfo requestInfo)
        {
            DespatchMessage("1","",requestInfo.totalCmd_Rpi);
        }

        protected override void OnStartup()
        {
            Console.WriteLine(TAG + "OnStartup");
            commandSessionStack = new Stack<RpiCommandSession>();
            m_DespatchServer = this.Bootstrap.GetServerByName("App") as IDespatchServer;
            base.OnStartup();
            
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            return base.Setup(rootConfig, config);
        }

        protected override void OnStopped()
        {
            Console.WriteLine(TAG + "OnStopped");
            /*
            if (commandSessionStack.Count > 0)
            {
                RpiCommandSession session = commandSessionStack.Pop();
                ServerDispatchErrMessageToRpi(session, "1");
                commandSessionStack.Push(session);
            }
            */
            base.OnStopped();
        }
    }

}
