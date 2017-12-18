using System;
using System.Collections.Generic;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using TCPSERVER.Filter;
using System.Data.SqlClient;
using System.Threading;

///   App → AppServer
/// _______________________________________________________________________________________________
/// |#| 用户名 |#| 密码 |#| 模式 |# | 前 |# | 后 |# | 左 |# | 右 |# | 抬 |# | 低 |# | 实时视频 |#  | 
/// ------------------------------------------------------------------------------------------------
/// |0|1-4     |5| 6-8  |9| 10   |11| 12 |13| 14 |15|16  |17| 18 |19| 20 |21| 22 |23|  24      |25 |
/// ------------------------------------------------------------------------------------------------
/// | |0       | FF/00 | 01/02/03/|  00/01  | 00/01 | 00/01 | 00/01 | 00/01 | 00/01 | 00/01 |  
/// ------------------------------------------------------------------------------------------------ 
/// 
///使用固定分割符的过滤器（#）

/// <summary>
/// ///  AppServer → App 通信格式
/// ---------
/// | 0-3   | 
/// ---------
/// | #     | 
/// ---------
/// 0000    服务器 DOWN 
/// 0001    服务器 UP
/// 0010    登录 成功 
/// 0011    登录 失败
/// 0100    注册 成功
/// 0101    注册 失败
/// 0110    Session 类中 异常
/// 0111    请求格式不符合过滤器的格式
/// 1000    Session 关闭 
/// 1001    已有用户登录
/// 1010    服务器 未开启（app本地检测）   
/// </summary>
///

/// #nameOfPic          图片信息格式 
/// $100,100,100,100    测距信息格式

namespace TCPSERVER
{
    class MyAppServer : AppServer<MyAppSession, MyAppRequestInfo>, IDespatchServer
    {
        string TAG = "AppServer:";
        //标志全局的message
        string controlMessage;

        //标识控制命令是否正在执行
        bool isControling = false;

        //标识现在是否用户在线
        static bool  isUserOn = false;

        static bool  canPushSession=true;
        //定义保存session的栈
        Stack<MyAppSession> appSessionStack;

        //定义接口，供向上转型
        private IDespatchServer m_DespatchServer;
        private IDespatchServer cm_DespatchServer;

        public MyAppServer()
            : base(new DefaultReceiveFilterFactory<MyAppReceiveFilter, MyAppRequestInfo>())
        {
            NewSessionConnected += MyAppServer_NewSessionConnected;
            NewRequestReceived += MyAppServer_NewRequestReceived;
        }

        //实现接口，向App发送测距信息
        public void ServerDispatchDisMessageToApp(string message)
        {
            if(appSessionStack.Count>0)
            {
                MyAppSession session = appSessionStack.Pop();
                session.Send("$"+message);
                appSessionStack.Push(session);
                Console.WriteLine(TAG+"AppServer → App 测距信息");
            }
            else
            {
                Console.WriteLine(TAG + "AppServer → App 测距信息 appSessionStack.Count=0");
            }
            
        }
        
        public void DispatchNormalMessage(string rpiId, string message)
        {
            if(appSessionStack.Count>0)
            {
                MyAppSession session = appSessionStack.Pop();
                session.Send("#"+message);
                //Console.WriteLine(TAG + "AppServer → App 图片信息" );
                appSessionStack.Push(session);
            }
            else
            {
                Console.WriteLine(TAG + "AppServer → App 图片信息 appSessionStack.Count=0");
            }

        }

        //实现接口.Server → App 发送错误消息.根据错误的类型发送不同的消息
        public void ServerDispatchErrMessageToApp(MyAppSession session,string mode)
        {
            //MyAppSession session = appSessionStack.Pop();
            switch (mode)
            {
                case "1":
                    session.Send("0000");
                    Console.WriteLine(TAG + "AppServer → Rpi : 服务器DOWN");
                    break;
                case "2":
                    session.Send("0001");
                    Console.WriteLine(TAG + "APPServer → APP : 服务器UP");
                    break;
                case "3":
                    session.Send("0010");
                    Console.WriteLine(TAG + "APPServer → APP : APP 登录 成功");
                    break;
                case "4":
                    session.Send("0011");
                    Console.WriteLine(TAG + "APPServer → APP : APP 登录 失败");
                    break;
                case "5":
                    session.Send("0100");
                    Console.WriteLine(TAG + "APPServer → APP : APP 注册 成功");
                    break;
                case "6":
                    session.Send("0101");
                    Console.WriteLine(TAG + "APPServer → APP : APP 注册 失败");
                    break;
                case "7":
                    session.Send("0110");
                    Console.WriteLine("APPServer → APP : Session 类 异常");
                    break;
                case "8":
                    session.Send("0111");
                    Console.WriteLine("APPServer → APP : APP 发送信息格式不符合过滤器格式");
                    break;
                case "9":
                    session.Send("1000");
                    Console.WriteLine("APPServer → APP : Session 关闭");
                    break;
                case "10":
                    session.Send("1001");
                    Console.WriteLine("APPServer → APP : 已有用户 登录");
                    break;
                
            }
            //appSessionStack.Push(session);
        }

        //自定义函数.App → Rpi 当需要发送信息时，需要调用的方法
        internal void DespatchMessage(string mode, string targetMacKey, string message)
        {
            switch (mode)
            {
                case "1"://控制信息,直接传送到RpiCommandServer
                    cm_DespatchServer.DispatchNormalMessage(targetMacKey, message);
                    break;
                case "4"://开始视频信息,直接传送到图片服务器
                    m_DespatchServer.DispatchNormalMessage(targetMacKey, message);
                    break;
                case "2"://APP DOWN 信息
                    {
                        //向CommandRpi发送appDown信息
                        cm_DespatchServer.DispatchAppDownMessageToRpi(targetMacKey, message);
                        //向Rpi发送appDown信息
                        m_DespatchServer.DispatchAppDownMessageToRpi(targetMacKey, message);
                    }
                    break;
                case "3"://APP UP 信息
                    //cm_DespatchServer.DispatchNormalMessage(targetMacKey, message);
                    //m_DespatchServer.DispatchNormalMessage(targetMacKey, message);
                    break;
            }
        }

        //在这里主要的就是要操作数据库里了
        private void MyAppServer_NewRequestReceived(MyAppSession session, MyAppRequestInfo requestInfo)
        {

            Console.WriteLine(TAG+"App请求类型：" + requestInfo.requestType_App);

            //为session设定唯一标识符
            session.sn_Session = "1";
            String userPassword = requestInfo.password;
            String tempPassword = "";

            switch(requestInfo.requestType_App)
            {
                //向CommandServer传送控制命令
                case "0010":
                    {
                        //构造一个message
                        string message = string.Format(requestInfo.front + requestInfo.back + requestInfo.left + requestInfo.right + requestInfo.up + requestInfo.down + requestInfo.video);
                        controlMessage = message;
                        isControling = true;

                        //开新线程
                        new Thread(new ThreadStart(OnTimedEvent)).Start();

                    }; break;
                //向CommandServer传送停止控制命令
                case "0111":
                    {
                        //Console.WriteLine("AppServer收到停止控制命令");

                        isControling = false;
                        //然后向树莓派发送停止信息
                        string message = "0000000";
                        DespatchMessage("1", "", message);
                        
                    }; break;
                //向RpiServer传送视频命令
                case "0101":
                    {
                        string message = string.Format(requestInfo.front + requestInfo.back + requestInfo.left + requestInfo.right + requestInfo.up + requestInfo.down + requestInfo.video);
                        DespatchMessage("4", "", message);

                    }; break;
                //登录
                case "0011":
                    {
                        /*
                        if (session.isSessionAlive)
                            Console.WriteLine("Session is alive");
                        else
                            Console.WriteLine("Session has Died");
                            */
                        if (!isUserOn)
                        {
                            //打开数据库
                            SqlConnection con = new SqlConnection();
                            con.ConnectionString = "server=.;database=PiRobot;user=sa;pwd=123qwE";
                            con.Open();
                            Console.WriteLine(TAG + "打开数据库成功！");

                            //从数据库中寻找匹配的password地址，进行校对。
                            string sql = "select userPassword from UserInfo where userName= '" + requestInfo.username + "'";
                            SqlCommand sc = new SqlCommand(sql, con);
                            SqlDataReader reader = sc.ExecuteReader();

                            //因为Login表中的MAC是主键，不可能有重复的
                            //所以只要查找结果集不为空就行。
                            if (reader.Read())
                                tempPassword = reader[0].ToString();
                            //关闭数据库
                            con.Close();
                            Console.WriteLine(TAG + "关闭数据库成功！");
                            //AppServer → App
                            //进行密码的比对，如果正确就返回1，否则返回0.
                            if (tempPassword.CompareTo(userPassword) == 0)
                            {
                                ServerDispatchErrMessageToApp(session, "3");
                                //登录注册成功之后，就不允许其它用户进行登录
                                //稍后进行改正
                                isUserOn = true;
                                //session.isSessionLogin = true;
                                //Console.WriteLine("Login isUerOn=true");

                            }  
                            else
                                ServerDispatchErrMessageToApp(session, "4");
                        }
                        else
                        {
                            //向app发送已经用户在线的消息
                            ServerDispatchErrMessageToApp(session, "10");
                        }
                    }; break;
                //注册
                case "0100":
                    {
                        if(!isUserOn)
                        {
                            //打开数据库
                            SqlConnection con = new SqlConnection();
                            con.ConnectionString = "server=.;database=PiRobot;user=sa;pwd=123qwE";
                            con.Open();
                            Console.WriteLine("APPServer打开数据库成功！");
                            //插入数据库。
                            string sql = "insert into UserInfo values('" + requestInfo.username + "','" + requestInfo.password + "')";
                            SqlCommand sc = new SqlCommand(sql, con);
                            int count = 0;
                            try
                            {
                                count = sc.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("用户名重复!");
                            }
                            //关闭数据库
                            con.Close();
                            Console.WriteLine("APPServer关闭数据库成功！");
                            //AppServer → App
                            if (count != 0)
                            {
                                ServerDispatchErrMessageToApp(session, "5");
                                //登录注册成功之后，就不允许其它用户进行登录
                                isUserOn = true;
                                //session.isSessionLogin = true;
                                //Console.WriteLine("Register isUerOn=true");
                            } 
                            else
                                ServerDispatchErrMessageToApp(session, "6");
                        }
                        else
                        {
                            //向app发送已经用户在线的消息
                            ServerDispatchErrMessageToApp(session, "10");
                        }
                    }; break;
                
                //APP DOWN
                case "0000":
                    {
                        //向树莓派/控制发送app Down信息
                        DespatchMessage("2", "", "1101");

                        closeAllSession();
                        //当app下线之后将isUserOn设置为false
                        isUserOn = false;
                        canPushSession = true;
                        //session.isSessionAlive = false;
                        Console.WriteLine(TAG + "APP DOWM");
                        
                        //Console.WriteLine("APPDown isUerOn=false");

                    }; break;
                 //APP UP
                //相关处理可以在这里写入
                case "0001":
                    {
                        //向树莓派发送app UP信息
                        //DespatchMessage("2", "00010203040506", "1110");

                        //只有当某一次链接的control——session创建之后才进行设置
                        session.isSessionLogin = true;
                        Console.WriteLine(TAG + "APP UP");

                        //找到当前的session
                        //将session是否存活设置为假
                    }; break;
                
            }
        }
        public void closeAllSession()
        {
            while(appSessionStack.Count>0)
            {
                appSessionStack.Pop().Close();
            }
        }


        //当session意外断掉之后
        public static void onSessionClosed()
        {
            isUserOn = false;
            canPushSession = true;
            Console.WriteLine( "APP SESSION CLOSE!");
        }


        private void MyAppServer_NewSessionConnected(MyAppSession session)
        {
            if(!isUserOn||canPushSession)
            {
                session.isSessionOnline = true;
                closeAllSession();
                //将Session压栈三次
                appSessionStack.Push(session);
                appSessionStack.Push(session);
                appSessionStack.Push(session);
                if (isUserOn)
                    canPushSession = false;
            }
            Console.WriteLine(TAG+ "AppServer_NewSessionConnected");
        }
        //每隔一定时间向Rpi发送控制命令

        private void OnTimedEvent()
        {
            int num = 1;
            while(isControling)
            {
                num++;
                if((num%20000000)==0)
                {
                    DespatchMessage("1", "", controlMessage);
                    Console.WriteLine("向RPI发送消息:"+controlMessage);
                    num = 0;
                }
            }
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            //Console.WriteLine(TAG + "Setup");
            return base.Setup(rootConfig, config);
        }
        
        protected override void OnStopped()
        {
            Console.WriteLine(TAG + "OnStopped");
            //这段代码需要测试一下
            if(appSessionStack.Count>0)
            {
                MyAppSession session = appSessionStack.Pop();
                ServerDispatchErrMessageToApp(session,"1");
                appSessionStack.Push(session);
            }
            base.OnStopped();
        }

        protected override void OnStartup()
        {
            Console.WriteLine(TAG + "OnStartup");
            appSessionStack = new Stack<MyAppSession>();
            //这个地方的服务器的名字是配置文件中服务器实例的名字 
            //并将这个服务器向上转型变成接口 
            m_DespatchServer = this.Bootstrap.GetServerByName("Rpi") as IDespatchServer;
            cm_DespatchServer = this.Bootstrap.GetServerByName("RpiCommand") as IDespatchServer;
            base.OnStartup();
        }

        //字符串转化为16进制
        public static string tenToSixteen(string msg)
        {
            long number = Convert.ToInt64(msg);
            return Convert.ToString(number, 16);
        }

        public void DispatchAppDownMessageToRpi(string macKey, string message)
        {
            throw new NotImplementedException();
        }
    }
}
