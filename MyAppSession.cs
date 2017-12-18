using System;
using SuperSocket.SocketBase;

namespace TCPSERVER
{
    class MyAppSession : AppSession<MyAppSession, MyAppRequestInfo>
    {
        string TAG = "AppSession:";

        /// <summary>
        /// 唯一标识session
        /// </summary>
        public string sn_Session { get; set; }

        /// <summary>
        ///session是否存活
        /// </summary>
        public bool isSessionAlive { get; set; }

        /// <summary>
        ///session是否存活
        /// </summary>
        public bool isSessionLogin { get; set; }

        /// <summary>
        ///session是否存活
        /// </summary>
        public bool isSessionOnline { get; set; }
        //session建立第二步到达这里
        protected override void OnSessionStarted()
        {
            this.isSessionAlive = true;
            Console.WriteLine(TAG+"OnSessionStarted");
        }
        protected override void HandleUnknownRequest(MyAppRequestInfo requestInfo)
        {
            Console.WriteLine(TAG + "HandleUnknownRequest");
            //这个地方可以加上自己的信息格式
            //this.Send("Unknow request");
        }

        protected override void HandleException(Exception e)
        {
            this.isSessionAlive = false;
            Console.WriteLine(TAG + "HandleException:"+e.ToString());
            //这个地方可以加上自己的信息格式
            //this.Send(e.StackTrace);
        }

        //session关闭到达这里
        protected override void OnSessionClosed(CloseReason reason)
        {
            this.isSessionAlive = false;
            Console.WriteLine(TAG + "OnSessionClosed "+reason.ToString());
            //如果该session已经链接并且是controlsession，只要session断掉之后
            //就进行初始化
            if(isSessionOnline&&isSessionLogin)
                MyAppServer.onSessionClosed();
            base.OnSessionClosed(reason);
        }
    }
}
