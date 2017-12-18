using System;
using SuperSocket.SocketBase;

namespace TCPSERVER
{
    class RpiCommandSession :AppSession<RpiCommandSession, RpiCommandInfo>
    {
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
        }
        protected override void HandleUnknownRequest(RpiCommandInfo requestInfo)
        {
            //this.Send("Unknow request");
        }

        protected override void HandleException(Exception e)
        {
            //this.Send(e.StackTrace);
        }
        protected override void OnSessionClosed(CloseReason reason)
        {
            base.OnSessionClosed(reason);
        }
    }
}
