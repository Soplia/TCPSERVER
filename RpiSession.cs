using System;
using SuperSocket.SocketBase;

namespace TCPSERVER
{
    class RpiSession : AppSession<RpiSession, RpiRequestInfo>
    {
        string TAG = "RpiSession:";
        /// <summary>
        /// 标志Rpi是否登录
        /// </summary>
        public bool isLogin_Rpi { get; set; }

        /// <summary>
        /// 唯一标识session
        /// </summary>
        public string sn_Session { get; set; }

        /// <summary>
        ///session是否存活
        /// </summary>
         public bool isSessionAlive { get; set; }


        protected override void OnSessionStarted()
        {
            Console.WriteLine(TAG+"OnSessionStarted");
        }
        protected override void HandleUnknownRequest(RpiRequestInfo requestInfo)
        {
            //RpiServer.ServerDispatchErrMessageToRpi(this, "12");
        }

        protected override void HandleException(Exception e)
        {
            //RpiServer.ServerDispatchErrMessageToRpi(this, "11");
            //this.Send(e.StackTrace);
        }

        protected override void OnSessionClosed(CloseReason reason)
        {
            //RpiServer.setIsVideoFirstTrue();
            //RpiServer.ServerDispatchErrMessageToRpi(this, "13");
            base.OnSessionClosed(reason);
        }
        
    }
}
