using System;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using TCPSERVER.Filter;

namespace TCPSERVER
{
    class UdpServer : AppServer<UdpSession, UdpRequestInfo>
    {
        string TAG = "UdpServer:";

        public UdpServer()
            : base(new DefaultReceiveFilterFactory<UdpServerFilter, UdpRequestInfo>()) // 11 parts but 10 separators
        {
            NewSessionConnected += UdpServer_NewSessionConnected;
            NewRequestReceived += UdpServer_NewRequestReceived;
        }

        //在这里主要的就是要操作数据库里了
        private void UdpServer_NewRequestReceived(UdpSession session, UdpRequestInfo requestInfo)
        {
            Console.WriteLine(TAG + "App请求信息：" + requestInfo.totalCmd_App);
        }

        //session建立第一步到达这里
        private void UdpServer_NewSessionConnected(UdpSession session)
        {
            
            Console.WriteLine(TAG + "UdpServer_NewSessionConnected");
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            Console.WriteLine(TAG + "Setup");
            return base.Setup(rootConfig, config);
        }

        protected override void OnStopped()
        {
            Console.WriteLine(TAG + "OnStopped");
            base.OnStopped();
        }

        protected override void OnStartup()
        {
            Console.WriteLine(TAG + "OnStartup");
            base.OnStartup();
        }
       
    }
}
