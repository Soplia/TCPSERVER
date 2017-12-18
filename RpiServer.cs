using System;
using System.Collections.Generic;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using TCPSERVER.Filter;
using System.Drawing;
using System.IO;

/// <summary>
/// ///  Rpi → Server 通信格式
/// ---------------------------------------
/// | 0-3 |        4  -  x-1    |x - x+3  | 
/// ---------------------------------------
/// | #   |                     |  $      | 
/// ---------------------------------------
/// 0-3,    固定开头    
/// 4，     消息类型  0：图片  
/// 5-x-1， 消息主体
/// x-x+3， 固定结尾
/// </summary>
/// 

/// <summary>
/// ///  Server → Rpi 通信格式
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
/// 0010    Rpi 连接 服务器 成功 
/// 0011    Rpi 连接 服务器 失败
/// 0100    Rpi 请求 成功
/// 0101    Rpi 请求 失败
/// 0110    Rpi 发送图片 成功
/// 0111    Server 请求 重发图片 


/// </summary>
/// 

namespace TCPSERVER
{
    class RpiServer : AppServer<RpiSession, RpiRequestInfo>, IDespatchServer
    {
        static string TAG = "RpiServer:";

        bool isVideo = false;
        bool isFirstVideo = true;
        bool isAppDown = false;
        //为服务器应用过滤器，并且为它添加回调函数
        public RpiServer()
        : base(new DefaultReceiveFilterFactory<RpiReceiveFilter, RpiRequestInfo>()) 
        {
            NewSessionConnected += RpiServer_NewSessionConnected;
            NewRequestReceived += RpiServer_NewRequestReceived;
        }

        //定义保存RpiSession的栈
        Stack<RpiSession> rpiSessionStack;

        //定义接口，供向上转型，通过这个对象将信息，从 RpiServer 发送到 App
        private IDespatchServer m_DespatchServer;

        
        //自定义函数。RpiServer → App (唯一标识符、消息）
        internal void DespatchMessage(string mode,string targetMacKey, string message)
        {
            switch(mode)
            {
                case "1"://向App发送图片信息
                    m_DespatchServer.DispatchNormalMessage(targetMacKey, message);
                    break;
            }
        }
    
        /// <summary>
        /// 实现接口
        /// 因为RpiServer是与Rpi链接的，所以该方法是向Rpi发送信息
        /// macKey：用来确定要向那个Rpi发送信息，在这一阶段的项目开发中应该是用不到的，那就不要考虑了。
        /// message：向Rpi传送的信息。就是要根据app发送上来的信息，构造一个如下格式的信息即可。
        /// 这段代码是要在AppServer中获得一个Rpi的实例，然后通过其来向Rpi发送信息，进而实现向Rpi发送信息。
        /// </summary>
        
        //设置标志位，每收到一张图片就要求发送另外一张图片
        public void DispatchNormalMessage(string macKey, string message)
        {
            Console.WriteLine(TAG + "收到APP 实时视频按钮 请求" + message);
            if (message.CompareTo("0000001") == 0)
                isVideo = true;
            else if (message.CompareTo("0000000") == 0)
                isVideo = false;

            if (isFirstVideo&&isVideo)
            {
                if(rpiSessionStack.Count>0)
                {
                    RpiSession session = rpiSessionStack.Pop();
                    session.Send(message);
                    Console.WriteLine(TAG + "RpiServer → Rpi : " + message);
                    rpiSessionStack.Push(session);
                    isFirstVideo = false;
                    isAppDown = false;
                }
            }
        }
        private void RpiServer_NewRequestReceived(RpiSession session, RpiRequestInfo requestInfo)
        {
            //Console.WriteLine(TAG+"NewRequestReceived");
            //设置请求类型
            string mode = requestInfo.reqType_Rpi;
            //Console.WriteLine(TAG+"Rpi请求类型：" + mode);

            //根据请求模式的不同，采取不同的动作
            switch (mode)
            {
                //图片信息
                case "0":
                    {
                        //将目标图片的名字发送给app
                        DespatchMessage("1", "", requestInfo.picData_Rpi);
                        //因为app停止之后，再收到一张图片之后，还要把上次的那个信息发送出去。
                        if (isVideo && (!isAppDown))
                        {
                            session.Send("0000001");
                            //Console.WriteLine("向图片服务器发送信息!视频开始");
                        }
                        else if (!isVideo && (!isAppDown))
                        {
                            session.Send("0000000");
                            isFirstVideo = true;
                            //Console.WriteLine("向图片服务器发送信息!视频结束");
                        }
                    };
                    break;
            }
        }

        public void DispatchAppDownMessageToRpi(string macKey, string message)
        {
            //app断了之后也要将其设置为true
            isFirstVideo = true;
            isAppDown = true;

            if (rpiSessionStack.Count>0)
            {
                RpiSession session = rpiSessionStack.Pop();
                //向树莓派，发送AppDOWN的消息
                ServerDispatchErrMessageToRpi(session, "15");
                rpiSessionStack.Push(session);
                //appdown之后将isFirstVideo设置为true
                isFirstVideo = true;
                Console.WriteLine("Rpi在线已经发送AppDown消息");
            }
            else
                Console.WriteLine("Rpi不在线已经发送AppDown消息");
        }

        //实现接口.Server → App 发送测距消息.根据错误的类型发送不同的消息
        public void ServerDispatchDisMessageToApp(string message)
        {
            //仅仅需要在app端实现，在这里没有必要实现
        }

        //自定义函数：Server → Rpi 发送错误消息.根据错误的类型发送不同的消息
        public static void ServerDispatchErrMessageToRpi(RpiSession session,string mode)
        {
            //RpiSession session = rpiSessionStack.Pop();
            switch (mode)
            {
                case "1":
                    session.Send("0000");
                    Console.WriteLine(TAG + "RpiServer → Rpi : 服务器DOWN");
                    break;
                case "2":
                    session.Send("0001");
                    Console.WriteLine(TAG + "RpiServer → Rpi : 服务器UP");
                    break;
                case "3":
                    session.Send("0010");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 连接 服务器 成功");
                    break;
                case "4":
                    session.Send("0011");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 连接 服务器 失败");
                    break;
                case "5":
                    session.Send("0100");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 请求 成功");
                    break;
                case "6":
                    session.Send("0101");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 请求 失败");
                    break;
                case "7":
                    session.Send("0110");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 发送图片 成功");
                    break;
                case "8":
                    session.Send("0111");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 发送图片 失败");
                    break;
                case "9":
                    session.Send("1000");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 发送测距 成功");
                    break;
                case "10":
                    session.Send("1001");
                    Console.WriteLine(TAG + "RpiServer → Rpi : Rpi 发送测距 失败");
                    break;
                case "11":
                    session.Send("1010");
                    Console.WriteLine("RpiServer → Rpi : Session 类 异常");
                    break;
                case "12":
                    session.Send("1011");
                    Console.WriteLine("RpiServer → Rpi : Rpi 发送信息格式不符合过滤器格式");
                    break;
                case "13":
                    session.Send("1100");
                    Console.WriteLine("RpiServer → Rpi : Session 关闭");
                    break;
                case "14":
                    session.Send("1101");
                    Console.WriteLine("RpiServer → Rpi : AppDown");
                    break;
                case "15":
                    session.Send("0000000");
                    Console.WriteLine("RpiServer → Rpi : AppDown 停止视频");
                    break;
            }
            //rpiSessionStack.Push(session);
        }
        
        private void RpiServer_NewSessionConnected(RpiSession session)
        {
            closeAllSession();
            //这个地方需要压栈两次
            rpiSessionStack.Push(session);
            rpiSessionStack.Push(session);
            rpiSessionStack.Push(session);
            rpiSessionStack.Push(session);
            isVideo = false;
            isFirstVideo = true;
            isAppDown = false;
            //ServerDispatchErrMessageToRpi(session, "3");
            Console.WriteLine(TAG+ "RpiServer_NewSessionConnected");
        }

        public void closeAllSession()
        {
            while (rpiSessionStack.Count > 0)
            {
                rpiSessionStack.Pop().Close();
            }
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            return base.Setup(rootConfig, config);
        }

        protected override void OnStartup()
        {
            Console.WriteLine(TAG + "OnStartup");
            rpiSessionStack = new Stack<RpiSession>();
            m_DespatchServer = Bootstrap.GetServerByName("App") as IDespatchServer;
            base.OnStartup();
            //10S执行一次该函数
            //tr = new Timer(new TimerCallback(setF), this, 0, 100000);
        }

        protected override void OnStopped()
        {
            Console.WriteLine(TAG + "OnStopped");
            /*
            if(rpiSessionStack.Count>0)
            {
                RpiSession session = rpiSessionStack.Pop();
                //向树莓派，发送服务器DOWN的消息
                ServerDispatchErrMessageToRpi(session,"1");
                rpiSessionStack.Push(session);
            }
            */
            base.OnStopped();
        }

        //C#打开本地图片
        //在这个地方需要添加程序集引用
        //添加的方法是，右键项目→添加→引用
        private byte[] JpgConvertByte()
        {
            Image image = Image.FromFile("C:\\test\\2.jpg");
            MemoryStream ms1 = new MemoryStream();
            image.Save(ms1, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] temp = ms1.GetBuffer();
            printByte(temp);
            Console.WriteLine("图片数组大小:"+temp.Length);
            return temp;
        }

        //打印byte数组
        public void printByte(byte [] arr)
        {
            for(int i=0;i<arr.Length;i++)
            {
                Console.Write(arr[i].ToString("X2") + " ");
            }
        }

        //将byte数组转化成string
        public string byteToString(byte[] arr)
        {
            string temp = "";
            foreach (byte b in arr)
            {
                temp += b.ToString("X2");
            }
            return temp;
        }

    }
}

