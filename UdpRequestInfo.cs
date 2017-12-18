using SuperSocket.SocketBase.Protocol;

namespace TCPSERVER
{
    class UdpRequestInfo : IRequestInfo
    {
        public string Key
        {
            get; set;
        }
        //app请求
        public string totalCmd_App
        { get; set; }
    }
}
