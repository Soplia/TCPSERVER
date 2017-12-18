using System;
using SuperSocket.SocketBase;

namespace TCPSERVER
{
    class UdpSession : AppSession<UdpSession, UdpRequestInfo>
    {
        string TAG = "UdpSession:";

        //session建立第二步到达这里
        protected override void OnSessionStarted()
        {
            Console.WriteLine(TAG + "OnSessionStarted");
        }
        protected override void HandleUnknownRequest(UdpRequestInfo requestInfo)
        {
            Console.WriteLine(TAG + "HandleUnknownRequest");
            //这个地方可以加上自己的信息格式
            //this.Send("Unknow request");
        }

        protected override void HandleException(Exception e)
        {
            Console.WriteLine(TAG + "HandleException");
            //这个地方可以加上自己的信息格式
            //this.Send(e.StackTrace);
        }

        //session关闭到达这里
        protected override void OnSessionClosed(CloseReason reason)
        {
            Console.WriteLine(TAG + "OnSessionClosed");
            base.OnSessionClosed(reason);
        }

    }



}
